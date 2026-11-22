# -*- coding: utf-8 -*-
"""
Sprint 9 (impl plan docs/sprint-9/impl-plan.md, spec specs/sprint-9/spec.md): nhan vat Blender.
  - SK_Khach  (F-080): mot than khach dung chung, 14 xuong, 7 kieu toc / mu bat tat theo ten, ao doi mau trong game.
  - SK_ChuNo  (F-082 AC-1): chu no 3,6 dau (ca mu), vai vuong, vest #4A3F3A, day vang #F2C14E; cung ten xuong voi khach.
  - SM_Hope, SM_ConGai (F-082 AC-2): tu the tinh (khong xuong).
Dong tac (F-081, Art Bible muc 12): Cho (2 giay lap, nhun +-3%), Vui, Gian (0,6-1,2 giay, giu dinh 2-4 khung), VayChao, Di (buoc, lap),
NgacNhien (chu no). Key "tren 2": lay mau moi 2 khung (24 khung / giay -> 12 hinh / giay), noi suy tuyen tinh; game chon muot hay
Stepped (spec F-081 AC-3, Q-67). Squash & stretch <= 15%.

Chay: "D:\\Blender\\blender.exe" -b --factory-startup -P SourceArt/Blender/scripts/build_characters.py -- [ten ...]
Toa do viet theo Unity (x phai, y len, mat nhin ve -z) qua U() cua build_models.py; goc o giua hai ban chan.
Tay chan: ong cao su (2 xuong / tay, trong so muot) thay Bendy Bones vi FBX khong xuat Bendy Bones (Q-69).
"""
import bpy
import bmesh
import math
import os
import sys
from mathutils import Matrix, Quaternion, Vector

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)
import build_models as bmod  # noqa: E402
from build_models import Model, U, srgb_to_linear  # noqa: E402

FPS = 24
PREVIEW = os.path.join(bmod.ROOT, "SourceArt", "Blender", "preview")

SKIN, WHITE, INK = "#FBE0C3", "#FFFFFF", "#2B2321"
HAIR_DARK, HAIR_SILVER, CORAL, STRAW, LEAF = "#5A4036", "#EFE7DA", "#E8604F", "#D69B68", "#7DB87A"
SHIRT_KEY = "#E9A0A0"   # mau ao mac dinh = Hong rem; game doi mau vat lieu nay theo kieu khach (CustomerRig)
VEST, GOLD, CREAM, TIE, BROWN = "#4A3F3A", "#F2C14E", "#FFF3DC", "#B8463C", "#6B4A3A"


def Bu(p):
    """Blender -> Unity."""
    return Vector((-p.x, p.z, -p.y))


# ---------------------------------------------------------------- ong cao su


def tube(m, hexcode, pts, radius, seg=10, sub=4, end_radius=None):
    """Ong tron di qua cac diem (Unity), noi suy Catmull-Rom, hai dau bit. Tra ve verts."""
    P = [Vector(p) for p in pts]
    path = []
    for i in range(len(P) - 1):
        p0 = P[max(0, i - 1)]
        p1, p2 = P[i], P[i + 1]
        p3 = P[min(len(P) - 1, i + 2)]
        for k in range(sub):
            t = k / sub
            t2, t3 = t * t, t * t * t
            path.append(0.5 * ((2 * p1) + (-p0 + p2) * t + (2 * p0 - 5 * p1 + 4 * p2 - p3) * t2 + (-p0 + 3 * p1 - 3 * p2 + p3) * t3))
    path.append(P[-1])
    rings = []
    verts = []
    n = len(path)
    for i, c in enumerate(path):
        d = (path[min(n - 1, i + 1)] - path[max(0, i - 1)]).normalized()
        a = Vector((0, 0, 1)) if abs(d.z) < 0.9 else Vector((1, 0, 0))
        x = d.cross(a).normalized()
        y = d.cross(x).normalized()
        r = radius if end_radius is None else radius + (end_radius - radius) * i / (n - 1)
        ring = []
        for k in range(seg):
            b = 2 * math.pi * k / seg
            p = c + (x * math.cos(b) + y * math.sin(b)) * r
            v = m.bm.verts.new(U(p.x, p.y, p.z))
            ring.append(v)
            verts.append(v)
        rings.append(ring)
    for i in range(n - 1):
        for k in range(seg):
            m.bm.faces.new((rings[i][k], rings[i][(k + 1) % seg], rings[i + 1][(k + 1) % seg], rings[i + 1][k]))
    m.bm.faces.new(list(reversed(rings[0])))
    m.bm.faces.new(rings[-1])
    idx = m.mat(hexcode)
    for f in {f for v in verts for f in v.link_faces}:
        f.material_index = idx
        f.smooth = True
    return verts


# ---------------------------------------------------------------- xuong


class Rig:
    """Danh sach xuong (Unity): ten, dau, duoi, cha."""

    def __init__(self):
        self.bones = []

    def add(self, name, head, tail, parent=None):
        self.bones.append((name, Vector(head), Vector(tail), parent))
        return self

    def seg(self, name):
        for b in self.bones:
            if b[0] == name:
                return b[1], b[2]
        raise KeyError(name)


def humanoid(hip_y, chest_y, head_top, shoulder, elbow, wrist, hand, hipx, knee_y, ankle_y, toe_z):
    r = Rig()
    r.add("Root", (0, 0, 0), (0, hip_y * 0.5, 0))
    r.add("Hips", (0, hip_y, 0), (0, (hip_y + chest_y) / 2, 0), "Root")
    r.add("Spine", (0, (hip_y + chest_y) / 2, 0), (0, chest_y, 0), "Hips")
    r.add("Head", (0, chest_y, 0), (0, head_top, 0), "Spine")
    for side, s in (("L", 1), ("R", -1)):
        sx = lambda p: (p[0] * s, p[1], p[2])  # noqa: E731
        r.add("Arm_" + side, sx(shoulder), sx(elbow), "Spine")
        r.add("Forearm_" + side, sx(elbow), sx(wrist), "Arm_" + side)
        r.add("Hand_" + side, sx(wrist), sx(hand), "Forearm_" + side)
        r.add("Leg_" + side, (hipx * s, hip_y, 0), (hipx * s, ankle_y, 0), "Hips")
        r.add("Foot_" + side, (hipx * s, ankle_y, 0), (hipx * s, 0.02, toe_z), "Leg_" + side)
    return r


def seg_dist(p, a, b):
    ab = b - a
    t = max(0.0, min(1.0, (p - a).dot(ab) / max(1e-9, ab.dot(ab))))
    return (p - (a + ab * t)).length


# ---------------------------------------------------------------- dung object


def make_material(hexcode):
    name = "M_Toon_" + hexcode
    m = bpy.data.materials.get(name)
    if m is None:
        m = bpy.data.materials.new(name)
        rgb = [srgb_to_linear(int(hexcode[i:i + 2], 16) / 255.0) for i in (0, 2, 4)]
        m.diffuse_color = (rgb[0], rgb[1], rgb[2], 1.0)
        m.use_nodes = True
        bsdf = m.node_tree.nodes.get("Principled BSDF")
        if bsdf is not None:
            bsdf.inputs["Base Color"].default_value = (rgb[0], rgb[1], rgb[2], 1.0)
            bsdf.inputs["Roughness"].default_value = 1.0
    return m


def to_object(model, name, rig=None, weights=None):
    """Model (bmesh) -> mesh object; weights: list ten xuong ung vien (nghich dao khoang cach) hoac {ten: 1}."""
    bmesh.ops.recalc_face_normals(model.bm, faces=model.bm.faces[:])
    mesh = bpy.data.meshes.new(name)
    model.bm.to_mesh(mesh)
    model.bm.free()
    obj = bpy.data.objects.new(name, mesh)
    bpy.context.scene.collection.objects.link(obj)
    for hexcode in model.mats:
        mesh.materials.append(make_material(hexcode))
    if rig is not None and weights:
        groups = {b: obj.vertex_groups.new(name=b) for b in weights}
        segs = {b: [Vector(x) for x in rig.seg(b)] for b in weights}
        for v in mesh.vertices:
            p = Bu(v.co)
            if len(weights) == 1:
                groups[weights[0]].add([v.index], 1.0, 'REPLACE')
                continue
            ws = {}
            for b in weights:
                d = seg_dist(p, *segs[b])
                ws[b] = 1.0 / (d + 0.02) ** 4
            tot = sum(ws.values())
            for b, w in ws.items():
                w /= tot
                if w > 0.04:
                    groups[b].add([v.index], w, 'REPLACE')
    return obj


def build_armature(rig, name="Rig"):
    data = bpy.data.armatures.new(name)
    obj = bpy.data.objects.new(name, data)
    bpy.context.scene.collection.objects.link(obj)
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    bpy.ops.object.mode_set(mode='EDIT')
    eb = {}
    for n, h, t, p in rig.bones:
        b = data.edit_bones.new(n)
        b.head = U(*h)
        b.tail = U(*t)
        b.roll = 0.0
        b.use_deform = True
        if p:
            b.parent = eb[p]
            b.use_connect = False
        eb[n] = b
    bpy.ops.object.mode_set(mode='OBJECT')
    for pb in obj.pose.bones:
        pb.rotation_mode = 'QUATERNION'
    return obj


def skin(objs, arm):
    for o in objs:
        o.parent = arm
        mod = o.modifiers.new("Armature", 'ARMATURE')
        mod.object = arm


# ---------------------------------------------------------------- dong tac


def qU(axis, deg):
    """Xoay quanh truc Unity (do, chieu Unity) -> quaternion Blender (U() la phep doi xung nen doi dau goc)."""
    return Quaternion(U(*axis).normalized(), math.radians(-deg))


def lerp_pose(a, b, t):
    out = {}
    for k in set(a) | set(b):
        pa, pb = a.get(k, {}), b.get(k, {})
        out[k] = {
            "rot": [(ax, pa_deg + (pb_deg - pa_deg) * t) for (ax, pa_deg), (_, pb_deg) in zip_rot(pa.get("rot", []), pb.get("rot", []))],
            "loc": tuple(x + (y - x) * t for x, y in zip(pa.get("loc", (0, 0, 0)), pb.get("loc", (0, 0, 0)))),
            "sy": pa.get("sy", 1.0) + (pb.get("sy", 1.0) - pa.get("sy", 1.0)) * t,
        }
    return out


def zip_rot(ra, rb):
    """Ghep danh sach xoay (truc, goc) cua hai tu the theo truc; truc thieu = 0 do."""
    axes = []
    for ax, _ in ra + rb:
        if ax not in axes:
            axes.append(ax)
    da, db = dict(ra), dict(rb)
    return [((ax, da.get(ax, 0.0)), (ax, db.get(ax, 0.0))) for ax in axes]


X, Y, Z = (1, 0, 0), (0, 1, 0), (0, 0, 1)


def apply_pose(arm, pose):
    for pb in arm.pose.bones:
        pb.rotation_quaternion = Quaternion()
        pb.location = Vector()
        pb.scale = Vector((1, 1, 1))
    for name, p in pose.items():
        pb = arm.pose.bones.get(name)
        if pb is None:
            continue
        rest = pb.bone.matrix_local.to_quaternion()
        q = Quaternion()
        for ax, deg in p.get("rot", []):
            q = qU(ax, deg) @ q
        pb.rotation_quaternion = rest.inverted() @ q @ rest
        loc = Vector(U(*p.get("loc", (0, 0, 0))))
        pb.location = rest.inverted() @ loc
        sy = p.get("sy", 1.0)
        sxz = 1.0 / math.sqrt(max(0.5, sy))   # giu the tich (squash & stretch)
        pb.scale = Vector((sxz, sy, sxz))


def bake_action(arm, name, keys, frames):
    """keys: [(khung, tu_the)], noi suy tuyen tinh, lay mau moi 2 khung (Art Bible muc 12 'tren 2')."""
    act = bpy.data.actions.new(name)
    act.use_fake_user = True
    arm.animation_data_create()
    arm.animation_data.action = act
    for f in range(0, frames + 1, 2):
        for i in range(len(keys) - 1):
            if keys[i][0] <= f <= keys[i + 1][0]:
                f0, p0 = keys[i]
                f1, p1 = keys[i + 1]
                t = 0.0 if f1 == f0 else (f - f0) / (f1 - f0)
                t = t * t * (3 - 2 * t)   # em dau em cuoi
                pose = lerp_pose(p0, p1, t)
                break
        else:
            pose = keys[-1][1]
        apply_pose(arm, pose)
        for pb in arm.pose.bones:
            pb.keyframe_insert("rotation_quaternion", frame=f)
            pb.keyframe_insert("location", frame=f)
            pb.keyframe_insert("scale", frame=f)
    act.frame_range = (0, frames)
    return act


def actions_for(arm, height, boss=False):
    """Bo dong tac dung chung cho moi than (ten xuong giong nhau). height: chieu cao that (m) de tinh % nhun."""
    acts = []
    b = 0.03 * height   # nhun +-3% chieu cao (Art Bible muc 12)
    # --- Cho: 2 giay, lap
    keys = []
    for k in range(0, 49, 6):
        s = math.sin(2 * math.pi * k / 48)
        keys.append((k, {
            "Hips": {"loc": (0, 0.6 * b * s, 0)},
            "Spine": {"sy": 1 + 0.07 * s},
            "Head": {"rot": [(Z, 3 * s)]},
            "Arm_L": {"rot": [(Z, 4 * s)]}, "Arm_R": {"rot": [(Z, 4 * s)]},
        }))
    acts.append(bake_action(arm, "Cho", keys, 48))
    # --- Vui: 20 khung (0,83 giay), dinh giu khung 8-12
    up = {"Hips": {"loc": (0, 0.09 * height, 0)}, "Spine": {"sy": 1.12},
          "Arm_L": {"rot": [(Z, 105)]}, "Arm_R": {"rot": [(Z, -105)]}, "Forearm_L": {"rot": [(Z, 40)]}, "Forearm_R": {"rot": [(Z, -40)]},
          "Leg_L": {"rot": [(X, -20)]}, "Leg_R": {"rot": [(X, -20)]}, "Head": {"rot": [(X, -8)]}}
    sq = {"Hips": {"loc": (0, -0.035 * height, 0)}, "Spine": {"sy": 0.87}, "Arm_L": {"rot": [(Z, 20)]}, "Arm_R": {"rot": [(Z, -20)]}}
    acts.append(bake_action(arm, "Vui", [(0, {}), (4, sq), (8, up), (12, up), (16, sq), (20, {})], 20))
    # --- Gian: 24 khung (1 giay): nghieng toi, nam tay gio len, lac dau, dam chan; dinh giu 10-14
    def angry(shake):
        return {"Spine": {"rot": [(X, -9)], "sy": 0.95}, "Head": {"rot": [(Y, 12 * shake), (X, -6)]},
                "Arm_L": {"rot": [(Z, 65), (X, -25)]}, "Arm_R": {"rot": [(Z, -65), (X, -25)]},
                "Forearm_L": {"rot": [(Z, 70)]}, "Forearm_R": {"rot": [(Z, -70)]},
                "Hips": {"loc": (0, -0.02 * height * abs(shake), 0)}}
    acts.append(bake_action(arm, "Gian", [(0, {}), (4, angry(1)), (7, angry(-1)), (10, angry(1)), (14, angry(1)), (17, angry(-1)),
                                          (20, angry(0)), (24, {})], 24))
    # --- VayChao: 28 khung, tay trai (Unity +x) gio cao vay 3 lan
    def wave(w):
        return {"Arm_L": {"rot": [(Z, 95)]}, "Forearm_L": {"rot": [(Z, 50 + 28 * w)]}, "Head": {"rot": [(Z, -5)]}, "Spine": {"rot": [(Z, -4)]}}
    acts.append(bake_action(arm, "VayChao", [(0, {}), (6, wave(0)), (10, wave(1)), (14, wave(-1)), (18, wave(1)), (22, wave(-1)), (28, {})], 28))
    # --- Di: 16 khung lap (0,67 giay mot chu ky 2 buoc)
    keys = []
    for k in range(0, 17, 2):
        p = 2 * math.pi * k / 16
        s = math.sin(p)
        keys.append((k, {"Leg_L": {"rot": [(X, 26 * s)]}, "Leg_R": {"rot": [(X, -26 * s)]},
                         "Foot_L": {"rot": [(X, -10 * max(0, -s))]}, "Foot_R": {"rot": [(X, -10 * max(0, s))]},
                         "Arm_L": {"rot": [(X, -18 * s), (Z, 6)]}, "Arm_R": {"rot": [(X, 18 * s), (Z, -6)]},
                         "Hips": {"loc": (0, 0.025 * height * abs(math.cos(p)), 0), "rot": [(Y, 5 * s)]},
                         "Spine": {"rot": [(Y, -4 * s), (X, -3)]}, "Head": {"rot": [(Z, 2 * s)]}}))
    acts.append(bake_action(arm, "Di", keys, 16))
    if boss:
        # --- NgacNhien: 20 khung, giat lui, tay bat ra, giu dinh 6-10
        top = {"Spine": {"rot": [(X, 10)], "sy": 1.08}, "Head": {"rot": [(X, 8)]}, "Hips": {"loc": (0, 0.03 * height, 0)},
               "Arm_L": {"rot": [(Z, 60)]}, "Arm_R": {"rot": [(Z, -60)]}, "Forearm_L": {"rot": [(Z, 30)]}, "Forearm_R": {"rot": [(Z, -30)]}}
        acts.append(bake_action(arm, "NgacNhien", [(0, {}), (6, top), (10, top), (20, {})], 20))
    arm.animation_data.action = acts[0]
    apply_pose(arm, {})
    return acts


# ---------------------------------------------------------------- xuat


def empty(name, pos, parent=None, bone=None):
    e = bpy.data.objects.new(name, None)
    e.empty_display_type = 'SPHERE'
    e.empty_display_size = 0.03
    bpy.context.scene.collection.objects.link(e)
    if parent is not None:
        e.parent = parent
        if bone:
            e.parent_type = 'BONE'
            e.parent_bone = bone
            pb = parent.pose.bones[bone]
            # vi tri the gioi -> trong khung xuong (duoi xuong la goc khi parent_type BONE)
            mw = parent.matrix_world @ pb.bone.matrix_local @ Matrix.Translation(Vector((0, pb.bone.length, 0)))
            e.matrix_parent_inverse = mw.inverted()
    e.location = U(*pos) if parent is None or not bone else U(*pos)
    return e


def export(name, group, objs, animated):
    os.makedirs(bmod.BLEND_DIR, exist_ok=True)
    bpy.ops.wm.save_as_mainfile(filepath=os.path.join(bmod.BLEND_DIR, name + ".blend"), check_existing=False)
    out_dir = os.path.join(bmod.UNITY_DIR, group)
    os.makedirs(out_dir, exist_ok=True)
    for o in bpy.context.scene.objects:
        o.select_set(o in objs)
    bpy.ops.export_scene.fbx(
        filepath=os.path.join(out_dir, name + ".fbx"), use_selection=True, object_types={'MESH', 'EMPTY', 'ARMATURE'},
        axis_forward='-Z', axis_up='Y', apply_unit_scale=True, apply_scale_options='FBX_SCALE_ALL', bake_space_transform=False,  # Unity xoay 180 do luc nhap (ModelImportSettings)
        mesh_smooth_type='FACE', use_mesh_modifiers=False, add_leaf_bones=False, primary_bone_axis='Y', secondary_bone_axis='X',
        use_armature_deform_only=True, bake_anim=animated, bake_anim_use_all_bones=True, bake_anim_use_nla_strips=False,
        bake_anim_use_all_actions=animated, bake_anim_force_startend_keying=True, bake_anim_step=1.0, bake_anim_simplify_factor=0.0,
        path_mode='STRIP', embed_textures=False, use_custom_props=False)
    tris = 0
    for o in objs:
        if o.type == 'MESH':
            tris += sum(len(p.vertices) - 2 for p in o.data.polygons)
    bones = len([o for o in objs if o.type == 'ARMATURE'][0].data.bones) if any(o.type == 'ARMATURE' for o in objs) else 0
    print("NHAN VAT %-12s tam_giac=%5d xuong=%2d dong_tac=%s" % (name, tris, bones, ",".join(a.name for a in bpy.data.actions)))
    return tris, bones


def render_pose(arm, name, action, frame, cam_dir=(0.45, 1.0, 0.35)):
    """Anh xem truoc mot khung cua mot dong tac (Workbench)."""
    scene = bpy.context.scene
    if arm is not None and action is not None:
        arm.animation_data.action = bpy.data.actions[action]
        scene.frame_set(frame)
    meshes = [o for o in scene.objects if o.type == 'MESH' and o.visible_get()]
    pts = []
    dg = bpy.context.evaluated_depsgraph_get()
    for o in meshes:
        ev = o.evaluated_get(dg)
        for v in ev.to_mesh().vertices:
            pts.append(o.matrix_world @ v.co)
        ev.to_mesh_clear()
    lo = Vector((min(p.x for p in pts), min(p.y for p in pts), min(p.z for p in pts)))
    hi = Vector((max(p.x for p in pts), max(p.y for p in pts), max(p.z for p in pts)))
    center = (lo + hi) / 2
    size = (hi - lo).length
    cam = scene.camera
    if cam is None:
        cam = bpy.data.objects.new("Cam", bpy.data.cameras.new("Cam"))
        scene.collection.objects.link(cam)
        scene.camera = cam
    d = size * 1.6
    cam.location = center + Vector(cam_dir).normalized() * d
    cam.rotation_euler = (center - cam.location).to_track_quat('-Z', 'Y').to_euler()
    scene.render.engine = 'BLENDER_WORKBENCH'
    scene.display.shading.light = 'STUDIO'
    scene.display.shading.color_type = 'MATERIAL'
    scene.display.shading.show_object_outline = True
    scene.display.shading.object_outline_color = (0.17, 0.14, 0.13)
    scene.display.shading.background_type = 'VIEWPORT'
    scene.display.shading.background_color = (0.95, 0.87, 0.72)
    scene.render.resolution_x = 360
    scene.render.resolution_y = 400
    os.makedirs(PREVIEW, exist_ok=True)
    scene.render.filepath = os.path.join(PREVIEW, "%s_%s_%02d.png" % (name, action or "tinh", frame))
    bpy.ops.render.render(write_still=True)


def reset():
    bpy.ops.wm.read_factory_settings(use_empty=True)
    scene = bpy.context.scene
    scene.unit_settings.system = 'METRIC'
    scene.render.fps = FPS
    return scene


# ================================================================ khach (F-080)

HEAD_C = (0.0, 2.2, 0.0)
HEAD_S = (1.3, 1.3, 1.2)


def khach():
    reset()
    rig = humanoid(hip_y=0.62, chest_y=1.62, head_top=2.85, shoulder=(0.45, 1.5, 0), elbow=(0.64, 1.22, -0.02),
                   wrist=(0.72, 0.98, -0.06), hand=(0.75, 0.86, -0.07), hipx=0.22, knee_y=0.35, ankle_y=0.14, toe_z=-0.22)
    arm = build_armature(rig)
    objs = [arm]
    # than (ao): qua le, mau ao doi trong game
    m = Model("Than", "x")
    m.sphere(SHIRT_KEY, (1.1, 1.25, 0.95), (0, 1.15, 0), seg=16, rings=10)
    objs.append(to_object(m, "Than", rig, ["Hips", "Spine"]))
    # chan: ong cao su ngan + giay (tung ben)
    for side, s in (("L", 1), ("R", -1)):
        m = Model("Chan_" + side, "x")
        tube(m, HAIR_DARK, [(0.22 * s, 0.7, 0), (0.22 * s, 0.4, 0), (0.23 * s, 0.14, -0.01)], 0.12, seg=10, sub=3)
        m.sphere(INK, (0.3, 0.17, 0.4), (0.24 * s, 0.085, -0.07), seg=12, rings=6)
        objs.append(to_object(m, "Chan_" + side, rig, ["Leg_" + side, "Foot_" + side, "Hips"]))
    # tay: ong cao su + gang trang; TayPhai (Unity -x) la tay cam dua luc an (EatingFx an no)
    for side, s, nm in (("L", 1, "TayTrai"), ("R", -1, "TayPhai")):
        m = Model(nm, "x")
        tube(m, SHIRT_KEY, [(0.42 * s, 1.52, 0.0), (0.64 * s, 1.22, -0.02), (0.72 * s, 0.98, -0.06)], 0.09, seg=10, sub=4)
        m.sphere(WHITE, (0.28, 0.28, 0.28), (0.75 * s, 0.9, -0.07), seg=12, rings=7)
        objs.append(to_object(m, nm, rig, ["Spine", "Arm_" + side, "Forearm_" + side, "Hand_" + side]))
    # dau
    m = Model("Dau", "x")
    m.sphere(SKIN, HEAD_S, HEAD_C, seg=20, rings=12)
    objs.append(to_object(m, "Dau", rig, ["Head"]))
    # 7 kieu toc / mu (bat tat theo ten trong game, F-080 AC-2)
    hx, hy, hz = HEAD_C

    def hair_cap(m, col, back=0.1, top=0.62, ry=0.36):
        m.sphere(col, (1.4, ry * 2, 1.3), (0, hy + top - 0.02, back), seg=16, rings=8)

    styles = []
    m = Model("Toc_BuiBac", "x")                 # ba cu: toc bac, bui tron, tram cai
    hair_cap(m, HAIR_SILVER)
    m.sphere(HAIR_SILVER, (0.5, 0.5, 0.5), (0, hy + 0.78, 0.12), seg=12, rings=7)
    m.cyl("#B97F52", (0.05, 0.62, 0.05), (0.2, hy + 0.9, 0.12), seg=6, rot=(0, 0, -52))
    m.sphere(CORAL, (0.13, 0.13, 0.13), (0.45, hy + 1.1, 0.12), seg=8, rings=5)
    styles.append(m)
    m = Model("Toc_Ngan", "x")                   # toc ngan sam + chom toc
    hair_cap(m, HAIR_DARK)
    m.sphere(HAIR_DARK, (0.3, 0.22, 0.2), (0.18, hy + 0.66, -0.42), (0, 0, -25), seg=10, rings=5)
    m.sphere(HAIR_DARK, (0.26, 0.2, 0.2), (-0.12, hy + 0.68, -0.44), (0, 0, 20), seg=10, rings=5)
    styles.append(m)
    m = Model("Toc_HaiBui", "x")                 # co van phong: hai bui hai ben + no
    hair_cap(m, HAIR_DARK)
    for s in (-1, 1):
        m.sphere(HAIR_DARK, (0.44, 0.44, 0.44), (s * 0.72, hy + 0.25, 0.08), seg=12, rings=7)
    m.sphere(CORAL, (0.22, 0.13, 0.1), (0.3, hy + 0.66, -0.25), seg=8, rings=5)
    styles.append(m)
    m = Model("Mu_LuoiTrai", "x")                # cau hoc sinh: mu luoi trai san ho
    hair_cap(m, HAIR_DARK, top=0.5)
    m.sphere(CORAL, (1.34, 0.6, 1.26), (0, hy + 0.5, 0.02), seg=16, rings=8)
    m.cyl(CORAL, (0.8, 0.05, 0.55), (0, hy + 0.48, -0.62), seg=14)
    styles.append(m)
    m = Model("Mu_Non", "x")                     # chu nong dan: non rom vanh rong tron
    hair_cap(m, HAIR_DARK, top=0.45)
    m.cyl(STRAW, (1.85, 0.06, 1.85), (0, hy + 0.55, 0.02), seg=24, bevel=0.02)
    m.sphere(STRAW, (1.05, 0.55, 1.05), (0, hy + 0.66, 0.02), seg=16, rings=8)
    m.cyl(CORAL, (1.07, 0.07, 1.07), (0, hy + 0.6, 0.02), seg=16)
    styles.append(m)
    m = Model("Mu_Noi", "x")                     # ong hoa si: mu noi det, toc bac hai ben
    for s in (-1, 1):
        m.sphere(HAIR_SILVER, (0.3, 0.42, 0.5), (s * 0.6, hy + 0.12, 0.12), seg=10, rings=6)
    m.cyl(CORAL, (1.35, 0.16, 1.28), (-0.08, hy + 0.6, 0.05), seg=18, rot=(0, 0, 9), bevel=0.04)
    m.sphere(CORAL, (0.12, 0.12, 0.12), (-0.08, hy + 0.72, 0.05), seg=8, rings=5)
    styles.append(m)
    m = Model("Mam", "x")                        # em be: chiec la mam tren dinh, no
    hair_cap(m, HAIR_DARK)
    m.cyl(LEAF, (0.05, 0.25, 0.05), (0, hy + 0.75, 0.05), seg=6)
    m.sphere(LEAF, (0.3, 0.07, 0.16), (0.13, hy + 0.9, 0.05), (0, 0, 20), seg=10, rings=5)
    m.sphere(CORAL, (0.22, 0.13, 0.1), (0.5, hy + 0.36, -0.1), seg=8, rings=5)
    styles.append(m)
    for st in styles:
        objs.append(to_object(st, st.name, rig, ["Head"]))
    skin([o for o in objs if o.type == 'MESH'], arm)
    face = empty("Face", (0, hy - 0.02, -0.6), arm, "Head")
    mouth = empty("Mieng", (0, hy - 0.22, -0.62), arm, "Head")
    objs += [face, mouth]
    actions_for(arm, 2.85)
    tris, bones = export("SK_Khach", "Characters", objs, True)
    for o in objs:
        if o.name.startswith(("Toc_", "Mu_", "Mam")):
            o.hide_render = o.name != "Toc_BuiBac"
    for act, f in (("Cho", 12), ("Vui", 10), ("Gian", 12), ("VayChao", 10), ("Di", 4)):
        render_pose(arm, "SK_Khach", act, f)
    for o in objs:
        if o.name.startswith(("Toc_", "Mu_", "Mam")):
            o.hide_render = o.name != "Mu_Non"
    render_pose(arm, "SK_Khach_MuNon", "Cho", 0, (0.7, 1.0, 0.4))
    return tris, bones


# ================================================================ chu no (F-082 AC-1)


def chu_no():
    reset()
    rig = humanoid(hip_y=0.55, chest_y=1.78, head_top=3.3, shoulder=(0.62, 1.72, 0), elbow=(0.7, 1.3, 0),
                   wrist=(0.73, 0.92, -0.02), hand=(0.74, 0.8, -0.03), hipx=0.24, knee_y=0.3, ankle_y=0.12, toe_z=-0.24)
    arm = build_armature(rig)
    objs = [arm]
    m = Model("Than", "x")                     # vest chu nhat bo nho (Art Bible muc 5: quyen luc), vai vuong
    m.box(VEST, (1.3, 1.32, 0.82), (0, 1.16, 0), round_=0.08, segs=2)
    for s in (-1, 1):
        m.box(VEST, (0.36, 0.2, 0.84), (s * 0.55, 1.74, 0), round_=0.12, segs=2)
    m.box(CREAM, (0.3, 0.3, 0.04), (0, 1.66, -0.42), (0, 0, 45), round_=0.2)
    m.box(TIE, (0.11, 0.42, 0.04), (0, 1.42, -0.43), round_=0.3)
    objs.append(to_object(m, "Than", rig, ["Hips", "Spine"]))
    m = Model("DayVang", "x")
    m.torus(GOLD, 0.36, 0.028, (0, 1.36, -0.44), (90, 0, 0), seg=14, rseg=5, arc=150)
    m.sphere(GOLD, (0.11, 0.11, 0.06), (0, 1.0, -0.46), seg=10, rings=6)
    objs.append(to_object(m, "DayVang", rig, ["Spine"]))
    for side, s in (("L", 1), ("R", -1)):
        m = Model("Chan_" + side, "x")
        tube(m, "#3A3230", [(0.24 * s, 0.6, 0), (0.24 * s, 0.35, 0), (0.24 * s, 0.12, 0)], 0.1, seg=10, sub=3)
        m.box(INK, (0.3, 0.13, 0.44), (0.25 * s, 0.065, -0.08), round_=0.3)
        objs.append(to_object(m, "Chan_" + side, rig, ["Leg_" + side, "Foot_" + side, "Hips"]))
    for side, s, nm in (("L", 1, "TayTrai"), ("R", -1, "TayPhai")):
        m = Model(nm, "x")
        tube(m, VEST, [(0.6 * s, 1.74, 0.0), (0.7 * s, 1.3, 0.0), (0.73 * s, 0.92, -0.02)], 0.1, seg=10, sub=4)
        m.sphere(WHITE, (0.3, 0.3, 0.3), (0.74 * s, 0.82, -0.03), seg=12, rings=7)
        objs.append(to_object(m, nm, rig, ["Spine", "Arm_" + side, "Forearm_" + side, "Hand_" + side]))
    m = Model("Dau", "x")                      # dau khoi chu nhat bo goc (ban ve cot truyen)
    m.box(SKIN, (0.9, 0.92, 0.8), (0, 2.28, 0), round_=0.22, segs=3)
    objs.append(to_object(m, "Dau", rig, ["Head"]))
    m = Model("Mu", "x")                       # mu chop: vanh, than, bang vang
    m.box(VEST, (1.08, 0.06, 0.95), (0, 2.75, 0.02), round_=0.3)
    m.box(VEST, (0.66, 0.56, 0.58), (0, 3.04, 0.02), round_=0.08)
    m.box(GOLD, (0.67, 0.09, 0.59), (0, 2.84, 0.02), round_=0.1)
    objs.append(to_object(m, "Mu", rig, ["Head"]))
    skin([o for o in objs if o.type == 'MESH'], arm)
    objs += [empty("Face", (0, 2.26, -0.41), arm, "Head"), empty("Mieng", (0, 2.08, -0.42), arm, "Head")]
    actions_for(arm, 3.3, boss=True)
    tris, bones = export("SK_ChuNo", "Characters", objs, True)
    for act, f in (("Cho", 12), ("NgacNhien", 8), ("Gian", 12), ("Di", 4)):
        render_pose(arm, "SK_ChuNo", act, f)
    return tris, bones


# ================================================================ Hope, con gai (tinh, F-082 AC-2)


def hope():
    reset()
    objs = []
    hy = 2.15                                   # 2,4 dau (Art Bible muc 6): dau kem toc ~1,2 m, cao ~2,85 m
    m = Model("Than", "x")                     # ao bac ha + tap de san ho co trai tim kem (ban ve cot truyen)
    m.sphere("#8CC9AE", (0.98, 1.3, 0.84), (0, 1.07, 0), seg=16, rings=9)
    m.box(CORAL, (0.68, 0.86, 0.06), (0, 1.0, -0.4), (8, 0, 0), round_=0.25)
    m.sphere(CREAM, (0.16, 0.14, 0.04), (0, 1.12, -0.45), seg=8, rings=5)
    objs.append(to_object(m, "Than"))
    m = Model("Chan", "x")
    for s in (-1, 1):
        tube(m, "#5A6B8C", [(0.18 * s, 0.62, 0), (0.18 * s, 0.32, 0), (0.18 * s, 0.1, 0)], 0.1, seg=8, sub=2)
        m.sphere(INK, (0.26, 0.14, 0.34), (0.19 * s, 0.07, -0.06), seg=10, rings=5)
    objs.append(to_object(m, "Chan"))
    m = Model("Tay", "x")                      # tay phai (Unity -x) vay chao, tay trai buong
    tube(m, "#8CC9AE", [(-0.4, 1.45, 0), (-0.66, 1.72, -0.05), (-0.72, 2.08, -0.08)], 0.08, seg=8, sub=4)
    tube(m, "#8CC9AE", [(0.4, 1.45, 0), (0.58, 1.18, -0.02), (0.63, 0.95, -0.05)], 0.08, seg=8, sub=4)
    m.sphere(WHITE, (0.25, 0.25, 0.25), (-0.73, 2.17, -0.08), seg=10, rings=6)
    m.sphere(WHITE, (0.25, 0.25, 0.25), (0.65, 0.88, -0.05), seg=10, rings=6)
    objs.append(to_object(m, "Tay"))
    m = Model("Dau", "x")
    m.sphere(SKIN, (1.0, 1.0, 0.94), (0, hy, 0), seg=18, rings=10)
    m.sphere(BROWN, (1.06, 0.56, 1.0), (0, hy + 0.3, 0.06), seg=16, rings=8)
    m.torus(BROWN, 0.1, 0.035, (0.12, hy + 0.58, -0.05), (90, 30, 0), seg=10, rseg=5, arc=220)
    objs.append(to_object(m, "Dau"))
    objs.append(empty("Face", (0, hy - 0.04, -0.46)))
    export("SM_Hope", "Characters", objs, False)
    render_pose(None, "SM_Hope", None, 0)


def con_gai():
    reset()
    objs = []
    hy = 1.62
    m = Model("Than", "x")                     # vay mau vest, vien vang, khoi tron (Art Bible muc 6)
    m.sphere(VEST, (1.0, 1.0, 0.86), (0, 0.78, 0), seg=16, rings=9, deform=lambda v: Vector((v.x * (1.25 - v.z), v.y * (1.25 - v.z), v.z)))
    m.torus(GOLD, 0.5, 0.03, (0, 0.42, 0), seg=20, rseg=5)
    objs.append(to_object(m, "Than"))
    m = Model("Chan", "x")
    for s in (-1, 1):
        tube(m, SKIN, [(0.16 * s, 0.36, 0), (0.16 * s, 0.1, 0)], 0.07, seg=8, sub=2)
        m.sphere(CORAL, (0.22, 0.12, 0.3), (0.17 * s, 0.06, -0.05), seg=10, rings=5)
    objs.append(to_object(m, "Chan"))
    m = Model("Tay", "x")                      # hai tay om truoc bung
    for s in (-1, 1):
        tube(m, VEST, [(0.36 * s, 1.08, 0), (0.42 * s, 0.85, -0.18), (0.12 * s, 0.75, -0.42)], 0.07, seg=8, sub=4)
        m.sphere(WHITE, (0.22, 0.22, 0.22), (0.08 * s, 0.74, -0.45), seg=10, rings=6)
    objs.append(to_object(m, "Tay"))
    m = Model("Dau", "x")
    m.sphere(SKIN, (0.92, 0.92, 0.86), (0, hy, 0), seg=18, rings=10)
    m.sphere(BROWN, (0.98, 0.52, 0.92), (0, hy + 0.28, 0.06), seg=16, rings=8)
    for s in (-1, 1):
        m.sphere(BROWN, (0.36, 0.36, 0.36), (s * 0.5, hy + 0.32, 0.08), seg=12, rings=6)
    objs.append(to_object(m, "Dau"))
    m = Model("No", "x")
    for s in (-1, 1):
        m.sphere(GOLD, (0.2, 0.14, 0.08), (0.28 + s * 0.11, hy + 0.5, -0.2), (0, 0, s * 20), seg=8, rings=5)
    objs.append(to_object(m, "No"))
    objs.append(empty("Face", (0, hy - 0.04, -0.42)))
    export("SM_ConGai", "Characters", objs, False)
    render_pose(None, "SM_ConGai", None, 0)


ALL = {"SK_Khach": khach, "SK_ChuNo": chu_no, "SM_Hope": hope, "SM_ConGai": con_gai}


def main():
    argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    lines = []
    for name, fn in ALL.items():
        if argv and name not in argv:
            continue
        r = fn()
        lines.append("%s\t%s" % (name, r))
    with open(os.path.join(bmod.BLEND_DIR, "_report_characters.txt"), "w", encoding="utf-8") as f:
        f.write("\n".join(lines) + "\n")
    print("XONG %d nhan vat" % len(lines))


if __name__ == "__main__":
    main()

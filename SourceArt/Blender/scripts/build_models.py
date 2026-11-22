# -*- coding: utf-8 -*-
"""
Dung model sprint 8 (F-071 nguyen lieu vong 1, F-072 do bep) bang Python cua Blender (yeu cau 21/11/2026:
"tao prj blender vao thu muc du an nay va lam cac task blender cua sprint").

Chay (Blender 5.3, khong mo giao dien):
    "D:\\Blender\\blender.exe" -b -P SourceArt/Blender/scripts/build_models.py -- [ten_model ...]

Moi model:
  - luu file goc SourceArt/Blender/models/<ten>.blend (Art Bible muc 14: file goc .blend ngoai Assets/, Git LFS)
  - xuat FBX vao My project/Assets/QuanAnHope/Resources/Models/<Nhom>/<ten>.fbx (game tu nap, F-070)
Quy cach (Art Bible muc 5, 10, 14): 1 don vi = 1 m, goc o day giua (dao: goc o dau chuoi), khoi tron bo goc,
mau phang (vat lieu ten M_Toon_RRGGBB, toi da 3 / mesh), khong dung vien trong Blender (shader Unity ve vien),
ngan sach tam giac: nguyen lieu <= 600, do vat nho <= 800. Empty "Face" danh dau tam dat mat (F-070 AC-5).

Toa do trong file nay viet theo Unity (x phai, y len, z vao trong); ham U() doi sang Blender (Z len) sao cho sau khi
xuat FBX (-Z forward, Y up, bake transform) va Unity nhap vao thi dung chieu.
"""
import bpy
import bmesh
import math
import os
import sys
from mathutils import Matrix, Vector

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.abspath(os.path.join(HERE, "..", "..", ".."))
BLEND_DIR = os.path.join(ROOT, "SourceArt", "Blender", "models")
UNITY_DIR = os.path.join(ROOT, "My project", "Assets", "QuanAnHope", "Resources", "Models")

# ---------------------------------------------------------------- toa do Unity -> Blender


def U(x, y, z):
    """Diem Unity (x, y, z) -> Blender. Unity = (-bx, bz, -by)."""
    return Vector((-x, -z, y))


def S(sx, sy, sz):
    """Kich thuoc Unity -> he so co gian Blender."""
    return Vector((sx, sz, sy))


def R(rx=0.0, ry=0.0, rz=0.0):
    """Goc Euler Unity (do, thu tu Z, X, Y) -> ma tran xoay Blender."""
    return (Matrix.Rotation(math.radians(-ry), 4, 'Z') @ Matrix.Rotation(math.radians(rx), 4, 'X')
            @ Matrix.Rotation(math.radians(rz), 4, 'Y'))


def srgb_to_linear(c):
    return c / 12.92 if c <= 0.04045 else ((c + 0.055) / 1.055) ** 2.4


# ---------------------------------------------------------------- dung model


class Model:
    def __init__(self, name, group, keep_origin=False):
        self.name = name
        self.group = group
        self.keep_origin = keep_origin
        self.bm = bmesh.new()
        self.mats = []
        self.face = None

    def mat(self, hexcode):
        hexcode = hexcode.lstrip('#').upper()
        if hexcode not in self.mats:
            self.mats.append(hexcode)
        return self.mats.index(hexcode)

    def _place(self, verts, hexcode, size, pos, rot):
        m = Matrix.Translation(U(*pos)) @ R(*rot) @ Matrix.Diagonal(S(*size).to_4d())
        bmesh.ops.transform(self.bm, matrix=m, verts=verts)
        idx = self.mat(hexcode)
        for f in {f for v in verts for f in v.link_faces}:
            f.material_index = idx
            f.smooth = True
        return verts

    # khoi don vi (duong kinh / canh = 1) roi co gian theo kich thuoc that
    def sphere(self, hexcode, size, pos, rot=(0, 0, 0), seg=12, rings=7, deform=None):
        r = bmesh.ops.create_uvsphere(self.bm, u_segments=seg, v_segments=rings, radius=0.5)
        verts = r['verts']
        if deform:
            for v in verts:
                v.co = deform(v.co)
        return self._place(verts, hexcode, size, pos, rot)

    def cyl(self, hexcode, size, pos, rot=(0, 0, 0), seg=12, top=1.0, bevel=0.0):
        r = bmesh.ops.create_cone(self.bm, cap_ends=True, cap_tris=False, segments=seg, radius1=0.5, radius2=0.5 * top, depth=1.0)
        verts = r['verts']
        verts = self._place(verts, hexcode, size, pos, (0, 0, 0))
        if bevel > 0:
            # chi bo vanh: canh giap mat nap (da giac > 4 dinh)
            edges = [e for e in {e for v in verts for e in v.link_edges} if any(len(f.verts) > 4 for f in e.link_faces)]
            res = bmesh.ops.bevel(self.bm, geom=edges, offset=bevel, offset_type='OFFSET', segments=2, profile=0.5, affect='EDGES')
            verts = [v for v in set(verts) | set(res.get('verts', [])) if v.is_valid]
            idx = self.mat(hexcode)
            for f in {f for v in verts for f in v.link_faces}:
                f.material_index = idx
                f.smooth = True
        if rot != (0, 0, 0):
            c = U(*pos)
            m = Matrix.Translation(c) @ R(*rot) @ Matrix.Translation(-c)
            bmesh.ops.transform(self.bm, matrix=m, verts=verts)
        return verts

    def box(self, hexcode, size, pos, rot=(0, 0, 0), round_=0.25, segs=2):
        r = bmesh.ops.create_cube(self.bm, size=1.0)
        verts = r['verts']
        bmesh.ops.transform(self.bm, matrix=Matrix.Diagonal(S(*size).to_4d()), verts=verts)
        if round_ > 0:
            edges = list({e for v in verts for e in v.link_edges})
            off = min(size) * round_
            res = bmesh.ops.bevel(self.bm, geom=edges, offset=off, offset_type='OFFSET', segments=segs, profile=0.5, affect='EDGES')
            verts = [v for v in set(verts) | set(res.get('verts', [])) if v.is_valid]
        m = Matrix.Translation(U(*pos)) @ R(*rot)
        bmesh.ops.transform(self.bm, matrix=m, verts=verts)
        idx = self.mat(hexcode)
        for f in {f for v in verts for f in v.link_faces}:
            f.material_index = idx
            f.smooth = True
        return verts

    def torus(self, hexcode, major, minor, pos, rot=(0, 0, 0), seg=16, rseg=6, scale_y=1.0, arc=360.0):
        """Vong xuyen nam ngang (truc Unity y). arc < 360: cung ho."""
        verts = []
        rings = []
        full = arc >= 359.9
        n = seg if full else seg + 1
        for i in range(n):
            a = math.radians(arc) * i / seg
            ring = []
            for k in range(rseg):
                b = 2 * math.pi * k / rseg
                rr = major + minor * math.cos(b)
                p = Vector((rr * math.cos(a), rr * math.sin(a), minor * math.sin(b) * scale_y))
                v = self.bm.verts.new(p)
                ring.append(v)
                verts.append(v)
            rings.append(ring)
        for i in range(n if full else n - 1):
            r0, r1 = rings[i], rings[(i + 1) % n]
            for k in range(rseg):
                self.bm.faces.new((r0[k], r0[(k + 1) % rseg], r1[(k + 1) % rseg], r1[k]))
        if not full:
            for ring in (rings[0], rings[-1]):
                try:
                    self.bm.faces.new(ring if ring is rings[-1] else list(reversed(ring)))
                except ValueError:
                    pass
        m = Matrix.Translation(U(*pos)) @ R(*rot)
        bmesh.ops.transform(self.bm, matrix=m, verts=verts)
        idx = self.mat(hexcode)
        for f in {f for v in verts for f in v.link_faces}:
            f.material_index = idx
            f.smooth = True
        return verts

    def set_face(self, x, y, z):
        self.face = (x, y, z)

    # ---------------------------------------------------------------- luu + xuat
    def build(self):
        bpy.ops.wm.read_factory_settings(use_empty=True)
        scene = bpy.context.scene
        scene.unit_settings.system = 'METRIC'
        scene.unit_settings.scale_length = 1.0
        bmesh.ops.recalc_face_normals(self.bm, faces=self.bm.faces[:])
        if not self.keep_origin:
            # goc o day, giua (Art Bible muc 14)
            xs = [v.co.x for v in self.bm.verts]
            ys = [v.co.y for v in self.bm.verts]
            zs = [v.co.z for v in self.bm.verts]
            off = Vector((-(min(xs) + max(xs)) / 2, -(min(ys) + max(ys)) / 2, -min(zs)))
        else:
            off = Vector((0, 0, -min(v.co.z for v in self.bm.verts)))
        bmesh.ops.translate(self.bm, vec=off, verts=self.bm.verts[:])
        mesh = bpy.data.meshes.new(self.name)
        self.bm.to_mesh(mesh)
        self.bm.free()
        obj = bpy.data.objects.new(self.name, mesh)
        scene.collection.objects.link(obj)
        for hexcode in self.mats:
            m = bpy.data.materials.get("M_Toon_" + hexcode) or bpy.data.materials.new("M_Toon_" + hexcode)
            rgb = [srgb_to_linear(int(hexcode[i:i + 2], 16) / 255.0) for i in (0, 2, 4)]
            m.diffuse_color = (rgb[0], rgb[1], rgb[2], 1.0)
            m.use_nodes = True
            bsdf = m.node_tree.nodes.get("Principled BSDF")
            if bsdf is not None:
                bsdf.inputs["Base Color"].default_value = (rgb[0], rgb[1], rgb[2], 1.0)
                bsdf.inputs["Roughness"].default_value = 1.0
            mesh.materials.append(m)
        objs = [obj]
        if self.face is not None:
            e = bpy.data.objects.new("Face", None)
            e.empty_display_type = 'SPHERE'
            e.empty_display_size = 0.01
            e.location = U(*self.face) + off
            scene.collection.objects.link(e)
            e.parent = obj
            objs.append(e)
        tris = sum(len(p.vertices) - 2 for p in mesh.polygons)

        os.makedirs(BLEND_DIR, exist_ok=True)
        bpy.ops.wm.save_as_mainfile(filepath=os.path.join(BLEND_DIR, self.name + ".blend"), check_existing=False)
        out_dir = os.path.join(UNITY_DIR, self.group)
        os.makedirs(out_dir, exist_ok=True)
        for o in scene.objects:
            o.select_set(o in objs)
        bpy.context.view_layer.objects.active = obj
        bpy.ops.export_scene.fbx(
            filepath=os.path.join(out_dir, self.name + ".fbx"), use_selection=True, object_types={'MESH', 'EMPTY'},
            axis_forward='-Z', axis_up='Y', apply_unit_scale=True, apply_scale_options='FBX_SCALE_ALL', bake_space_transform=True,
            mesh_smooth_type='FACE', use_mesh_modifiers=True, add_leaf_bones=False, path_mode='STRIP', embed_textures=False,
            use_custom_props=False)
        print("MODEL %-22s nhom=%-11s tam_giac=%4d vat_lieu=%d" % (self.name, self.group, tris, len(self.mats)))
        return tris


# ---------------------------------------------------------------- mau (Art Bible muc 4 + mau rieng cua tung nguyen lieu)
LEAF, LEAF_DARK, STEM, TAN, CREAM = "#6FA855", "#4F8A4C", "#9CCB7A", "#D69B68", "#FFF3DC"
STEEL, STEEL_DARK, STEEL_LIGHT = "#9AA0A6", "#5E6268", "#C9CDD1"
WOOD_LIGHT, WOOD_DARK, WOOD_HANDLE = "#D69B68", "#B97F52", "#8C5A3A"


def ring(r, deg, y):
    a = math.radians(deg)
    return (math.cos(a) * r, y, math.sin(a) * r)


# ================================================================ nguyen lieu vong 1 (F-071)

def ca_chua():
    m = Model("SM_NL_ca_chua", "Ingredients")
    m.sphere("#E8604F", (0.076, 0.058, 0.076), (0, 0.029, 0), seg=14, rings=9,
             deform=lambda c: Vector((c.x, c.y, c.z - 0.06 * max(0.0, c.z) * (1 - 4 * (c.x * c.x + c.y * c.y)))))
    for i in range(5):
        m.sphere("#5E8F3E", (0.028, 0.006, 0.011), ring(0.012, 72 * i, 0.058), rot=(0, -72 * i, -14), seg=6, rings=3)
    m.cyl("#5E8F3E", (0.008, 0.014, 0.008), (0, 0.064, 0), seg=6)
    m.set_face(0, 0.03, 0)
    return m


def ca_chua_so_che():
    m = Model("SM_NL_ca_chua_SoChe", "Ingredients")
    for i in range(5):
        a = 72 * i
        m.sphere("#E8604F", (0.05, 0.026, 0.022), ring(0.022, a, 0.013), rot=(0, -a, 0), seg=8, rings=5)
        m.sphere("#F59A8C", (0.04, 0.012, 0.012), ring(0.024, a, 0.022), rot=(0, -a, 0), seg=6, rings=3)
    m.set_face(0, 0.016, 0)
    return m


def hanh_tay():
    m = Model("SM_NL_hanh_tay", "Ingredients")
    # cu hanh: tron, vuot nhon len tren
    taper = lambda c: Vector((c.x * (1 - 0.55 * max(0.0, c.z * 2) ** 2), c.y * (1 - 0.55 * max(0.0, c.z * 2) ** 2), c.z))
    m.sphere("#D98FB0", (0.068, 0.062, 0.068), (0, 0.031, 0), seg=14, rings=9, deform=taper)
    for i in range(3):
        m.sphere("#B96F92", (0.005, 0.058, 0.07), (0, 0.031, 0), rot=(0, 60 * i, 0), seg=6, rings=6, deform=taper)
    m.cyl(TAN, (0.012, 0.016, 0.012), (0, 0.068, 0), seg=6, top=0.5)
    for i in range(3):
        m.cyl(TAN, (0.003, 0.009, 0.003), (-0.006 + 0.006 * i, 0.002, 0.002 * i), rot=(0, 0, -20 + 20 * i), seg=4)
    m.set_face(0, 0.03, 0)
    return m


def hanh_tay_so_che():
    m = Model("SM_NL_hanh_tay_SoChe", "Ingredients")
    for i in range(3):
        p = (-0.012 + 0.012 * i, 0.004 + 0.006 * i, -0.006 + 0.006 * i)
        m.torus("#D98FB0", 0.023, 0.005, p, rot=(6, 0, 0), seg=12, rseg=4)
        m.torus("#FBEFF4", 0.014, 0.004, p, rot=(6, 0, 0), seg=10, rseg=3)
    m.set_face(0, 0.012, 0)
    return m


def rau_muong():
    m = Model("SM_NL_rau_muong", "Ingredients")
    for k, (z, yaw, w) in enumerate(((0.0, 0, 0.012), (0.013, 12, 0.011), (-0.013, -12, 0.011))):
        m.cyl(STEM, (w, 0.11, w), (0, 0.012, z), rot=(0, yaw, 90), seg=6)
    for i in range(5):
        m.sphere(LEAF_DARK, (0.038, 0.006, 0.016), (0.046 + 0.008 * (i % 2), 0.022, -0.024 + 0.012 * i), rot=(0, -30 + 15 * i, 0), seg=6, rings=3)
    m.torus(TAN, 0.016, 0.004, (-0.012, 0.012, 0), rot=(0, 0, 90), seg=10, rseg=4)
    m.set_face(0.01, 0.02, 0)
    return m


def rau_muong_so_che():
    m = Model("SM_NL_rau_muong_SoChe", "Ingredients")
    for i in range(6):
        m.cyl(STEM, (0.009, 0.032, 0.009), ring(0.018 + 0.006 * (i % 2), 60 * i, 0.006), rot=(0, 25 + 60 * i, 90), seg=6)
    for i in range(3):
        m.sphere(LEAF_DARK, (0.034, 0.005, 0.014), ring(0.01, 120 * i + 30, 0.013), rot=(0, -120 * i, 0), seg=6, rings=3)
    m.set_face(0, 0.012, 0)
    return m


def trung():
    m = Model("SM_NL_trung", "Ingredients")
    # qua trung: nua duoi phinh, dau tren thon
    egg = lambda c: Vector((c.x * (1 - 0.22 * max(0.0, c.z * 2)), c.y * (1 - 0.22 * max(0.0, c.z * 2)), c.z))
    m.sphere("#F4DDB8", (0.056, 0.072, 0.056), (0, 0.036, 0), seg=14, rings=9, deform=egg)
    m.set_face(0, 0.034, 0)
    return m


def trung_so_che():
    m = Model("SM_NL_trung_SoChe", "Ingredients")
    # bat trung danh: than bat thon day, vanh bo tron, long trung vang, doi dua
    m.cyl("#F7F3EA", (0.078, 0.03, 0.078), (0, 0.015, 0), seg=14, top=1.0, bevel=0.006)
    m.cyl("#F7F3EA", (0.05, 0.006, 0.05), (0, 0.003, 0), seg=12)
    m.torus("#F7F3EA", 0.038, 0.004, (0, 0.03, 0), seg=14, rseg=4)
    m.cyl("#F6C445", (0.068, 0.004, 0.068), (0, 0.03, 0), seg=14)
    m.cyl("#C98E5A", (0.004, 0.09, 0.004), (0.006, 0.05, -0.004), rot=(0, 30, 65), seg=4)
    m.cyl("#C98E5A", (0.004, 0.09, 0.004), (0.006, 0.05, 0.006), rot=(0, 20, 62), seg=4)
    m.set_face(0, 0.03, 0)
    return m


def heo():
    m = Model("SM_NL_heo", "Ingredients")
    m.box("#F2A0A6", (0.08, 0.032, 0.048), (0, 0.016, 0.004), round_=0.35)
    m.box(CREAM, (0.08, 0.026, 0.016), (0, 0.013, -0.026), round_=0.4)
    m.box(CREAM, (0.05, 0.003, 0.005), (0.008, 0.031, 0.008), rot=(0, 15, 0), round_=0)
    m.box(CREAM, (0.03, 0.003, 0.004), (-0.012, 0.031, 0.016), rot=(0, -20, 0), round_=0)
    m.set_face(0, 0.017, 0)
    return m


def heo_so_che():
    m = Model("SM_NL_heo_SoChe", "Ingredients")
    for i in range(4):
        p = (-0.012 + 0.008 * i, 0.004 + 0.0045 * i, -0.012 + 0.012 * i)
        m.box("#F2A0A6", (0.062, 0.005, 0.022), p, rot=(10, 6 * i, 0), round_=0.4)
        m.box(CREAM, (0.062, 0.0052, 0.005), (p[0], p[1], p[2] - 0.012), rot=(10, 6 * i, 0), round_=0)
    m.set_face(0, 0.012, 0)
    return m


def dau_phu():
    m = Model("SM_NL_dau_phu", "Ingredients")
    m.box("#F1EAD8", (0.062, 0.048, 0.054), (0, 0.024, 0), round_=0.22)
    m.set_face(0, 0.024, 0)
    return m


def dau_phu_so_che():
    m = Model("SM_NL_dau_phu_SoChe", "Ingredients")
    for i in range(4):
        x = -0.016 if i < 2 else 0.016
        z = -0.016 if i % 2 == 0 else 0.016
        m.box("#F1EAD8", (0.026, 0.024, 0.026), (x, 0.012, z), rot=(0, 8 * i, 0), round_=0.3)
    m.set_face(0, 0.012, 0)
    return m


# ================================================================ do bep (F-072)

def chao():
    # Cung kich thuoc khoi cu (RoomView.BuildTool, scale 1 = tren gia): long 0,36 x 0,06 m, can go toi x = 0,43 m.
    m = Model("SM_Chao", "Kitchen", keep_origin=True)
    m.cyl(STEEL, (0.36, 0.05, 0.36), (0, 0.025, 0), seg=20, top=1.12, bevel=0.012)
    m.cyl(STEEL_DARK, (0.33, 0.004, 0.33), (0, 0.052, 0), seg=20)
    m.box(WOOD_HANDLE, (0.24, 0.032, 0.05), (0.31, 0.05, 0), rot=(0, 0, 6), round_=0.45)
    m.cyl(STEEL_DARK, (0.03, 0.03, 0.03), (0.19, 0.045, 0), rot=(0, 0, 90), seg=8)
    return m


def noi():
    m = Model("SM_Noi", "Kitchen", keep_origin=True)
    m.cyl("#E8604F", (0.32, 0.22, 0.32), (0, 0.11, 0), seg=20, bevel=0.02)
    m.torus("#E8604F", 0.165, 0.014, (0, 0.225, 0), seg=20, rseg=5)
    m.cyl("#9E3329", (0.3, 0.004, 0.3), (0, 0.205, 0), seg=20)
    for s in (-1, 1):
        m.torus("#2B2321", 0.03, 0.009, (s * 0.175, 0.17, 0), rot=(90, 0, 0), seg=8, rseg=4, arc=180)
    return m


def vi_nuong():
    m = Model("SM_ViNuong", "Kitchen", keep_origin=True)
    m.box(STEEL, (0.42, 0.03, 0.03), (0, 0.015, -0.135), round_=0.45)
    m.box(STEEL, (0.42, 0.03, 0.03), (0, 0.015, 0.135), round_=0.45)
    m.box(STEEL, (0.03, 0.03, 0.3), (-0.195, 0.015, 0), round_=0.45)
    m.box(STEEL, (0.03, 0.03, 0.3), (0.195, 0.015, 0), round_=0.45)
    for i in range(5):
        m.cyl(STEEL_LIGHT, (0.016, 0.27, 0.016), (-0.14 + 0.07 * i, 0.03, 0), rot=(90, 0, 0), seg=6)
    for s in (-1, 1):
        m.torus(STEEL_DARK, 0.03, 0.007, (s * 0.21, 0.02, 0), rot=(0, -90 * s, 0), seg=8, rseg=4, arc=180)
    return m


def thot():
    # Thot cua canh: 1,0 x 0,06 x 0,6 m (BuildNhaHang "CuttingBoard"), bo goc tron, lo treo o dau trai
    m = Model("SM_Thot", "Kitchen")
    m.box("#F2D7B6", (1.0, 0.05, 0.6), (0, 0.025, 0), round_=0.45)
    m.box("#E3BE93", (0.96, 0.004, 0.02), (0, 0.051, 0.27), round_=0)
    m.box("#E3BE93", (0.96, 0.004, 0.02), (0, 0.051, -0.27), round_=0)
    m.cyl("#B97F52", (0.07, 0.006, 0.07), (-0.43, 0.05, 0), seg=12)
    return m


def dao():
    # Dao: goc o dau chuoi, luoi chay theo -x (PrepFx). Luoi bo tron mui, chuoi go, khau thep.
    m = Model("SM_Dao", "Kitchen", keep_origin=True)
    m.box(WOOD_HANDLE, (0.11, 0.026, 0.026), (-0.052, 0.013, 0), round_=0.48)
    m.cyl("#B8B2A8", (0.03, 0.01, 0.03), (-0.12, 0.014, 0), rot=(0, 0, 90), seg=8)
    m.box("#D8D6D0", (0.22, 0.06, 0.006), (-0.235, 0.03, 0), round_=0.4)
    m.sphere("#D8D6D0", (0.06, 0.06, 0.006), (-0.34, 0.03, 0), seg=8, rings=4)
    m.box("#B8B2A8", (0.2, 0.006, 0.0062), (-0.235, 0.004, 0), round_=0)
    return m


def dia():
    m = Model("SM_Dia", "Kitchen")
    m.cyl("#FFFFFF", (0.26, 0.016, 0.26), (0, 0.008, 0), seg=20)
    m.torus("#FFFFFF", 0.15, 0.016, (0, 0.016, 0), seg=20, rseg=5, scale_y=0.7)
    m.torus("#E9E2D6", 0.172, 0.006, (0, 0.022, 0), seg=20, rseg=4)
    return m


def _crate(name, wood, accent, band=False, chest=False):
    # Thung 0,55 x 0,46 m: day 4 cm, thanh truoc 7 cm, sau 12 cm, hai ben 9 cm (CrateView, Art Bible muc 7)
    W, D = 0.55, 0.46
    sg = 1 if chest else 2
    m = Model(name, "Kitchen")
    m.box(wood, (W, 0.04, D), (0, 0.02, 0), round_=0.3, segs=sg)
    m.box(wood, (W, 0.07, 0.03), (0, 0.035, -D / 2), round_=0.35, segs=sg)
    m.box(wood, (W, 0.12, 0.03), (0, 0.06, D / 2), round_=0.35, segs=sg)
    m.box(wood, (0.03, 0.09, D), (-W / 2, 0.045, 0), round_=0.35, segs=sg)
    m.box(wood, (0.03, 0.09, D), (W / 2, 0.045, 0), round_=0.35, segs=sg)
    if not chest:
        # nan go: khe toi tren thanh truoc / sau
        m.box(accent, (W - 0.04, 0.006, 0.004), (0, 0.04, -D / 2 - 0.0155), round_=0)
        m.box(accent, (W - 0.04, 0.006, 0.004), (0, 0.06, D / 2 + 0.0155), round_=0)
    if band:
        for s in (-1, 1):
            m.box(accent, (0.035, 0.1, 0.035), (s * (W / 2 - 0.004), 0.05, -D / 2 + 0.004), round_=0)
            m.box(accent, (0.035, 0.13, 0.035), (s * (W / 2 - 0.004), 0.065, D / 2 - 0.004), round_=0)
    if chest:
        for sx in (-1, 1):
            for sz in (-1, 1):
                m.box(accent, (0.045, 0.13, 0.045), (sx * (W / 2 + 0.005), 0.065, sz * (D / 2 + 0.005)), round_=0)
        # nap vom mo ngua ra sau
        m.cyl(wood, (0.14, W, 0.24), (0, 0.2, D / 2 + 0.06), rot=(-20, 0, 90), seg=14, bevel=0.01)
        m.box(accent, (W + 0.02, 0.03, 0.03), (0, 0.09, D / 2 + 0.02), round_=0)
    return m


def thung_phu():
    return _crate("SM_ThungPhu", WOOD_LIGHT, "#B97F52")


def thung_chinh():
    return _crate("SM_ThungChinh", WOOD_DARK, "#6E6A66", band=True)


def ruong_dot_pha():
    return _crate("SM_RuongDotPha", "#C9B3F0", "#F2C14E", chest=True)


ALL = [ca_chua, ca_chua_so_che, hanh_tay, hanh_tay_so_che, rau_muong, rau_muong_so_che, trung, trung_so_che,
       heo, heo_so_che, dau_phu, dau_phu_so_che, chao, noi, vi_nuong, thot, dao, dia, thung_phu, thung_chinh, ruong_dot_pha]


def main():
    argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    report = []
    for fn in ALL:
        m = fn()
        if argv and m.name not in argv:
            m.bm.free()
            continue
        tris = m.build()
        report.append((m.name, m.group, tris, len(m.mats)))
    path = os.path.join(ROOT, "SourceArt", "Blender", "models", "_report.txt")
    with open(path, "w", encoding="utf-8") as f:
        for r in report:
            f.write("%s\t%s\t%d\t%d\n" % r)
    print("XONG %d model" % len(report))


if __name__ == "__main__":
    main()

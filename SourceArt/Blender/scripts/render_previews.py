# -*- coding: utf-8 -*-
"""
Anh xem truoc cac model sprint 8 (Workbench, mau phang + vien) -> SourceArt/Blender/preview/<ten>.png va bang tong hop.
Chay: "D:\\Blender\\blender.exe" -b -P SourceArt/Blender/scripts/render_previews.py
Goc nhin: tu phia nguoi choi (Unity -z = Blender +y), hoi tu tren xuong, nhu dung o loi di nhin vao quay.
"""
import bpy
import math
import os
from mathutils import Vector

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.abspath(os.path.join(HERE, "..", "..", ".."))
MODELS = os.path.join(ROOT, "SourceArt", "Blender", "models")
OUT = os.path.join(ROOT, "SourceArt", "Blender", "preview")


def render(blend):
    bpy.ops.wm.open_mainfile(filepath=blend)
    scene = bpy.context.scene
    meshes = [o for o in scene.objects if o.type == 'MESH']
    if not meshes:
        return None
    obj = meshes[0]
    pts = [obj.matrix_world @ Vector(c) for c in obj.bound_box]
    lo = Vector((min(p.x for p in pts), min(p.y for p in pts), min(p.z for p in pts)))
    hi = Vector((max(p.x for p in pts), max(p.y for p in pts), max(p.z for p in pts)))
    center = (lo + hi) / 2
    size = max((hi - lo).length, 0.05)
    cam_data = bpy.data.cameras.new("Cam")
    cam_data.lens = 50
    cam = bpy.data.objects.new("Cam", cam_data)
    scene.collection.objects.link(cam)
    d = size * 1.9
    cam.location = center + Vector((0.35 * d, 0.85 * d, 0.55 * d))
    direction = center - cam.location
    cam.rotation_euler = direction.to_track_quat('-Z', 'Y').to_euler()
    scene.camera = cam
    scene.render.engine = 'BLENDER_WORKBENCH'
    scene.display.shading.light = 'STUDIO'
    scene.display.shading.color_type = 'MATERIAL'
    scene.display.shading.show_object_outline = True
    scene.display.shading.object_outline_color = (0.17, 0.14, 0.13)
    scene.display.shading.show_cavity = False
    scene.render.resolution_x = 320
    scene.render.resolution_y = 320
    scene.render.film_transparent = False
    scene.world = bpy.data.worlds.new("W") if scene.world is None else scene.world
    scene.display.shading.background_type = 'VIEWPORT'
    scene.display.shading.background_color = (0.95, 0.87, 0.72)
    os.makedirs(OUT, exist_ok=True)
    path = os.path.join(OUT, os.path.splitext(os.path.basename(blend))[0] + ".png")
    scene.render.filepath = path
    bpy.ops.render.render(write_still=True)
    return path


def main():
    import sys
    argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    done = []
    for f in sorted(os.listdir(MODELS)):
        if f.endswith(".blend") and (not argv or any(f.startswith(a) for a in argv)):
            p = render(os.path.join(MODELS, f))
            if p:
                done.append(p)
                print("ANH", p)
    # bang tong hop 6 cot
    if not done:
        return
    imgs = [bpy.data.images.load(p) for p in done]
    cols = 6
    rows = (len(imgs) + cols - 1) // cols
    w = h = 320
    sheet = bpy.data.images.new("Sheet", cols * w, rows * h)
    px = [0.0] * (cols * w * rows * h * 4)
    for i, im in enumerate(imgs):
        src = list(im.pixels)
        cx, cy = (i % cols) * w, (rows - 1 - i // cols) * h
        for y in range(h):
            row = src[y * w * 4:(y + 1) * w * 4]
            start = ((cy + y) * cols * w + cx) * 4
            px[start:start + w * 4] = row
    sheet.pixels = px
    sheet.filepath_raw = os.path.join(OUT, "_tong_hop" + ("_" + "_".join(argv) if argv else "") + ".png")
    sheet.file_format = 'PNG'
    sheet.save()
    print("TONG HOP", sheet.filepath_raw)


main()

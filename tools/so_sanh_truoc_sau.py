# -*- coding: utf-8 -*-
"""
Anh so sanh truoc / sau cho F-071 AC-4 (sprint 8): truoc = hinh ghep khoi (docs/sprint-2/ui/nguyen_lieu/<id>.png o commit
truoc sprint 8), sau = model Blender (anh test PlayMode F012 vua chup cung goc nhin).
Chay sau khi chay PlayMode:  python tools/so_sanh_truoc_sau.py
Ket qua: docs/sprint-8/ui/so_sanh/<id>.png va _tat_ca.png. Xong thi lenh tra anh sprint 2 ve ban cu (L-018).
"""
import io
import os
import subprocess
from PIL import Image, ImageDraw, ImageFont

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), ".."))
IDS = ["ca_chua", "hanh_tay", "rau_muong", "trung", "heo", "dau_phu"]
NAMES = {"ca_chua": "Cà chua", "hanh_tay": "Hành tây", "rau_muong": "Rau muống", "trung": "Trứng gà", "heo": "Thịt heo", "dau_phu": "Đậu phụ"}
BASE = "c246bf5"   # commit sprint 7 (truoc sprint 8)
OUT = os.path.join(ROOT, "docs", "sprint-8", "ui", "so_sanh")
FONT = os.path.join(ROOT, "docs", "art-bible", "fonts")


def font(size):
    for root, _, files in os.walk(FONT):
        for f in files:
            if f.lower().startswith("baloo") and f.lower().endswith(".ttf"):
                return ImageFont.truetype(os.path.join(root, f), size)
    return ImageFont.load_default()


def old_image(i):
    rel = "docs/sprint-2/ui/nguyen_lieu/%s.png" % i
    data = subprocess.run(["git", "show", "%s:%s" % (BASE, rel)], cwd=ROOT, capture_output=True).stdout
    if data.startswith(b"version https://git-lfs"):
        data = subprocess.run(["git", "lfs", "smudge"], cwd=ROOT, input=data, capture_output=True).stdout
    return Image.open(io.BytesIO(data)).convert("RGB")


def main():
    os.makedirs(OUT, exist_ok=True)
    tiles = []
    f = font(34)
    for i in IDS:
        before = old_image(i)
        after = Image.open(os.path.join(ROOT, "docs", "sprint-2", "ui", "nguyen_lieu", i + ".png")).convert("RGB")
        w, h = 640, 360
        before = before.resize((w, h))
        after = after.resize((w, h))
        img = Image.new("RGB", (w * 2 + 12, h + 56), (43, 35, 33))
        img.paste(before, (0, 56))
        img.paste(after, (w + 12, 56))
        d = ImageDraw.Draw(img)
        d.text((16, 8), NAMES[i] + " · trước (ghép khối)", fill=(255, 243, 220), font=f)
        d.text((w + 28, 8), "sau (Blender)", fill=(242, 193, 78), font=f)
        path = os.path.join(OUT, i + ".png")
        img.save(path)
        tiles.append(img)
        print("anh", path)
    sheet = Image.new("RGB", (tiles[0].width, tiles[0].height * len(tiles)), (43, 35, 33))
    for k, t in enumerate(tiles):
        sheet.paste(t, (0, k * t.height))
    sheet = sheet.resize((sheet.width // 2, sheet.height // 2))
    sheet.save(os.path.join(OUT, "_tat_ca.png"))
    print("tong hop", os.path.join(OUT, "_tat_ca.png"))


if __name__ == "__main__":
    main()

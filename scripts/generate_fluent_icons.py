import os
import math
from PIL import Image, ImageDraw, ImageFilter, ImageChops

def create_linear_gradient(size, color1, color2, vertical=True, diagonal=False):
    """Create a smooth RGBA linear gradient image."""
    w, h = size
    base = Image.new("RGBA", (w, h))
    # Generate gradient at 256x256 and resize with BICUBIC for speed & smoothness
    gw, gh = 256, 256
    grad = Image.new("RGBA", (gw, gh))
    px = grad.load()
    for y in range(gh):
        for x in range(gw):
            if diagonal:
                t = (x / (gw - 1) * 0.45) + (y / (gh - 1) * 0.55)
            elif vertical:
                t = y / (gh - 1)
            else:
                t = x / (gw - 1)
            t = max(0.0, min(1.0, t))
            r = int(color1[0] + (color2[0] - color1[0]) * t)
            g = int(color1[1] + (color2[1] - color1[1]) * t)
            b = int(color1[2] + (color2[2] - color1[2]) * t)
            a = int(color1[3] + (color2[3] - color1[3]) * t)
            px[x, y] = (r, g, b, a)
    return grad.resize((w, h), Image.Resampling.BICUBIC)

def draw_fluent_folder_base(theme):
    """
    Renders a 1024x1024 Windows 11 Fluent 3D-style folder base.
    theme dict has:
      back_top, back_bot: RGBA tuples for back flap
      front_top, front_bot: RGBA tuples for front folder plate
      accent_glow: RGBA tuple for top-left specular glow
    """
    S = 1024
    canvas = Image.new("RGBA", (S, S), (0, 0, 0, 0))

    # 1. Outer ambient drop shadow under the folder
    shadow = Image.new("RGBA", (S, S), (0, 0, 0, 0))
    sdraw = ImageDraw.Draw(shadow)
    sdraw.rounded_rectangle([96, 240, 928, 895], radius=82, fill=(0, 0, 0, 125))
    sdraw.rounded_rectangle([120, 830, 904, 910], radius=50, fill=(0, 0, 0, 95))
    shadow = shadow.filter(ImageFilter.GaussianBlur(36))
    canvas = Image.alpha_composite(canvas, shadow)

    # 2. Back Folder Plate + Tab (Windows 11 smooth geometry)
    back_mask = Image.new("L", (S, S), 0)
    bdraw = ImageDraw.Draw(back_mask)
    # Main back body
    bdraw.rounded_rectangle([84, 210, 940, 850], radius=76, fill=255)
    # Top-left tab
    bdraw.rounded_rectangle([84, 136, 430, 320], radius=64, fill=255)
    # Smooth diagonal slope connecting tab to back body
    bdraw.polygon([(380, 136), (510, 210), (510, 320), (350, 320)], fill=255)
    # Slight blur + threshold to round the polygon join nicely
    back_mask = back_mask.filter(ImageFilter.GaussianBlur(10))
    back_mask = back_mask.point(lambda p: min(255, max(0, int((p - 110) * 8))))

    back_grad = create_linear_gradient((S, S), theme["back_top"], theme["back_bot"], diagonal=True)
    back_layer = Image.new("RGBA", (S, S), (0, 0, 0, 0))
    back_layer.paste(back_grad, (0, 0), back_mask)

    # Top edge highlight on back tab
    tab_hi_mask = ImageChops.subtract(back_mask, ImageChops.offset(back_mask, 0, 6))
    tab_hi = Image.new("RGBA", (S, S), (255, 255, 255, 90))
    back_layer.paste(tab_hi, (0, 0), tab_hi_mask)

    canvas = Image.alpha_composite(canvas, back_layer)

    # 3. Inner white/frosted document sheet peeking out
    doc_shadow = Image.new("RGBA", (S, S), (0, 0, 0, 0))
    ds_draw = ImageDraw.Draw(doc_shadow)
    ds_draw.rounded_rectangle([136, 232, 888, 420], radius=44, fill=(0, 0, 0, 85))
    doc_shadow = doc_shadow.filter(ImageFilter.GaussianBlur(18))
    canvas = Image.alpha_composite(canvas, doc_shadow)

    doc_mask = Image.new("L", (S, S), 0)
    ddraw = ImageDraw.Draw(doc_mask)
    ddraw.rounded_rectangle([140, 238, 884, 450], radius=42, fill=255)
    doc_grad = create_linear_gradient((S, S), (255, 255, 255, 245), (225, 235, 248, 230), vertical=True)
    doc_layer = Image.new("RGBA", (S, S), (0, 0, 0, 0))
    doc_layer.paste(doc_grad, (0, 0), doc_mask)
    canvas = Image.alpha_composite(canvas, doc_layer)

    # 4. Shadow cast by front flap onto back/doc
    flap_back_shadow = Image.new("RGBA", (S, S), (0, 0, 0, 0))
    fbs_draw = ImageDraw.Draw(flap_back_shadow)
    fbs_draw.rounded_rectangle([84, 275, 940, 855], radius=76, fill=(0, 0, 0, 110))
    flap_back_shadow = flap_back_shadow.filter(ImageFilter.GaussianBlur(22))
    # Clip to back_mask so shadow doesn't bleed outside
    clipped_fbs = Image.new("RGBA", (S, S), (0, 0, 0, 0))
    clipped_fbs.paste(flap_back_shadow, (0, 0), back_mask)
    canvas = Image.alpha_composite(canvas, clipped_fbs)

    # 5. Front Fluent Folder Plate
    front_mask = Image.new("L", (S, S), 0)
    fdraw = ImageDraw.Draw(front_mask)
    fdraw.rounded_rectangle([84, 292, 940, 864], radius=78, fill=255)

    front_grad = create_linear_gradient((S, S), theme["front_top"], theme["front_bot"], diagonal=True)

    # Add soft diagonal specular glow in upper-left of front plate
    glow = Image.new("RGBA", (S, S), (0, 0, 0, 0))
    gdraw = ImageDraw.Draw(glow)
    gdraw.ellipse([-100, 120, 720, 740], fill=theme["accent_glow"])
    glow = glow.filter(ImageFilter.GaussianBlur(95))
    front_combined = Image.alpha_composite(front_grad, glow)

    # Subtle bottom inner shadow for 3D thickness
    bot_rim = ImageChops.subtract(front_mask, ImageChops.offset(front_mask, 0, -14))
    bot_shade = Image.new("RGBA", (S, S), (0, 0, 0, 75))
    front_combined.paste(bot_shade, (0, 0), bot_rim)

    # Crisp top glass bevel highlight on front plate
    top_rim = ImageChops.subtract(front_mask, ImageChops.offset(front_mask, 0, 8))
    top_hi = create_linear_gradient((S, S), (255, 255, 255, 190), (255, 255, 255, 45), vertical=False)
    front_combined.paste(top_hi, (0, 0), top_rim)

    front_layer = Image.new("RGBA", (S, S), (0, 0, 0, 0))
    front_layer.paste(front_combined, (0, 0), front_mask)
    canvas = Image.alpha_composite(canvas, front_layer)

    return canvas


def paste_emblem_with_3d_effect(folder_img, emblem_mask, color_top=(255, 255, 255, 255), color_bot=(225, 238, 255, 245)):
    """Takes an L-mode emblem mask (1024x1024) and composites it with 3D drop shadow and top bevel."""
    S = 1024
    # Smooth anti-alias on mask
    emblem_mask = emblem_mask.filter(ImageFilter.GaussianBlur(1.2))

    # Drop shadow
    sh_mask = ImageChops.offset(emblem_mask, 0, 14).filter(ImageFilter.GaussianBlur(18))
    sh_layer = Image.new("RGBA", (S, S), (0, 0, 0, 0))
    sh_color = Image.new("RGBA", (S, S), (0, 8, 28, 125))
    sh_layer.paste(sh_color, (0, 0), sh_mask)

    out = Image.alpha_composite(folder_img, sh_layer)

    # Emblem body with subtle vertical gradient
    emb_grad = create_linear_gradient((S, S), color_top, color_bot, vertical=True)
    emb_layer = Image.new("RGBA", (S, S), (0, 0, 0, 0))
    emb_layer.paste(emb_grad, (0, 0), emblem_mask)

    # Top specular bevel on emblem
    bevel_mask = ImageChops.subtract(emblem_mask, ImageChops.offset(emblem_mask, 0, 5))
    bevel_hi = Image.new("RGBA", (S, S), (255, 255, 255, 210))
    emb_layer.paste(bevel_hi, (0, 0), bevel_mask)

    return Image.alpha_composite(out, emb_layer)


def draw_thick_line(draw, p1, p2, width, fill=255):
    """Draws a line with rounded caps."""
    draw.line([p1, p2], fill=fill, width=width)
    r = width // 2
    draw.ellipse([p1[0] - r, p1[1] - r, p1[0] + r, p1[1] + r], fill=fill)
    draw.ellipse([p2[0] - r, p2[1] - r, p2[0] + r, p2[1] + r], fill=fill)


def make_emblem_proyectos():
    """Code brackets </> emblem."""
    S = 1024
    m = Image.new("L", (S, S), 0)
    d = ImageDraw.Draw(m)
    cy = 582
    w = 42
    # Left chevron '<'
    draw_thick_line(d, (410, cy - 105), (305, cy), w)
    draw_thick_line(d, (305, cy), (410, cy + 105), w)
    # Right chevron '>'
    draw_thick_line(d, (614, cy - 105), (719, cy), w)
    draw_thick_line(d, (719, cy), (614, cy + 105), w)
    # Center slash '/'
    draw_thick_line(d, (548, cy - 130), (476, cy + 130), w)
    return m


def make_emblem_escuela():
    """3D Graduation Cap (Birrete) emblem."""
    S = 1024
    m = Image.new("L", (S, S), 0)
    d = ImageDraw.Draw(m)
    cx, cy = 512, 540
    # Top mortarboard diamond with rounded corners via thick lines + polygon
    pts = [(cx, cy - 115), (cx + 225, cy - 15), (cx, cy + 85), (cx - 225, cy - 15)]
    d.polygon(pts, fill=255)
    for i in range(4):
        draw_thick_line(d, pts[i], pts[(i + 1) % 4], 24, 255)

    # Cut a gap between mortarboard and skullcap for crisp separation
    cut_pts = [(cx, cy + 112), (cx + 150, cy + 45), (cx + 150, cy + 72), (cx, cy + 138), (cx - 150, cy + 72), (cx - 150, cy + 45)]

    # Skullcap base underneath
    cap = Image.new("L", (S, S), 0)
    cd = ImageDraw.Draw(cap)
    cd.polygon([(cx - 125, cy + 48), (cx, cy + 106), (cx + 125, cy + 48), (cx + 125, cy + 145), (cx, cy + 195), (cx - 125, cy + 145)], fill=255)
    cd.pieslice([cx - 125, cy + 85, cx + 125, cy + 205], start=0, end=180, fill=255)
    # Subtract a thick outline of the diamond so there's a clean negative space gap
    gap = Image.new("L", (S, S), 0)
    gd = ImageDraw.Draw(gap)
    for i in range(4):
        draw_thick_line(gd, pts[i], pts[(i + 1) % 4], 52, 255)
    gd.polygon(pts, fill=255)
    cap = ImageChops.subtract(cap, gap)

    m = ImageChops.lighter(m, cap)
    d = ImageDraw.Draw(m)

    # Tassel hanging on the right
    draw_thick_line(d, (cx + 185, cy + 4), (cx + 185, cy + 135), 22, 255)
    d.ellipse([cx + 161, cy + 118, cx + 209, cy + 168], fill=255)
    return m


def make_emblem_redes():
    """Cyber Network Shield + Connected Nodes / Lightning emblem."""
    S = 1024
    m = Image.new("L", (S, S), 0)
    d = ImageDraw.Draw(m)
    cx, cy = 512, 578

    # Central hub + 3 satellite network nodes + connecting beams (Global/Router/Network topology)
    # Outer ring / globe arc
    d.ellipse([cx - 165, cy - 165, cx + 165, cy + 165], outline=255, width=30)
    # Horizontal & vertical elliptical meridian arcs
    d.arc([cx - 88, cy - 165, cx + 88, cy + 165], start=0, end=360, fill=255, width=24)
    draw_thick_line(d, (cx - 158, cy), (cx + 158, cy), 24, 255)
    draw_thick_line(d, (cx - 135, cy - 82), (cx + 135, cy - 82), 20, 255)
    draw_thick_line(d, (cx - 135, cy + 82), (cx + 135, cy + 82), 20, 255)

    # Glowing network nodes at key intersections
    nodes = [(cx, cy - 165), (cx + 165, cy), (cx - 165, cy), (cx, cy + 165), (cx + 85, cy - 82), (cx - 85, cy + 82)]
    for nx, ny in nodes:
        d.ellipse([nx - 26, ny - 26, nx + 26, ny + 26], fill=255)
    return m


def make_emblem_juegos():
    """Modern Gamepad Controller emblem."""
    S = 1024
    m = Image.new("L", (S, S), 0)
    d = ImageDraw.Draw(m)
    cx, cy = 512, 585

    # Controller body: two angled grips + center bridge
    # Left grip
    for t in range(30):
        frac = t / 29.0
        x = (cx - 125) - frac * 52
        y = (cy - 45) + frac * 145
        r = int(78 - frac * 22)
        d.ellipse([x - r, y - r, x + r, y + r], fill=255)
    # Right grip
    for t in range(30):
        frac = t / 29.0
        x = (cx + 125) + frac * 52
        y = (cy - 45) + frac * 145
        r = int(78 - frac * 22)
        d.ellipse([x - r, y - r, x + r, y + r], fill=255)

    # Center bridge
    d.rounded_rectangle([cx - 175, cy - 120, cx + 175, cy + 52], radius=65, fill=255)
    # Top shoulder bumpers
    d.rounded_rectangle([cx - 155, cy - 138, cx - 55, cy - 95], radius=20, fill=255)
    d.rounded_rectangle([cx + 55, cy - 138, cx + 155, cy - 95], radius=20, fill=255)

    # Subtract D-pad on left and 4 action buttons on right
    cut = Image.new("L", (S, S), 0)
    cd = ImageDraw.Draw(cut)
    # D-Pad cross
    dx, dy = cx - 98, cy - 32
    cd.rounded_rectangle([dx - 16, dy - 48, dx + 16, dy + 48], radius=8, fill=255)
    cd.rounded_rectangle([dx - 48, dy - 16, dx + 48, dy + 16], radius=8, fill=255)
    # Action buttons (A, B, X, Y)
    bx, by = cx + 98, cy - 32
    btn_r = 16
    offset = 34
    for ox, oy in [(0, -offset), (0, offset), (-offset, 0), (offset, 0)]:
        cd.ellipse([bx + ox - btn_r, by + oy - btn_r, bx + ox + btn_r, by + oy + btn_r], fill=255)

    return ImageChops.subtract(m, cut)


def make_emblem_apps():
    """Windows 11 Fluent Apps Bento Grid (3 rounded squares + 1 tilted floating diamond tile)."""
    S = 1024
    m = Image.new("L", (S, S), 0)
    d = ImageDraw.Draw(m)
    cx, cy = 512, 580
    sz = 126
    gap = 22
    rad = 32

    # Bottom-left tile
    d.rounded_rectangle([cx - gap - sz, cy + gap, cx - gap, cy + gap + sz], radius=rad, fill=255)
    # Bottom-right tile
    d.rounded_rectangle([cx + gap, cy + gap, cx + gap + sz, cy + gap + sz], radius=rad, fill=255)
    # Top-left tile
    d.rounded_rectangle([cx - gap - sz, cy - gap - sz, cx - gap, cy - gap], radius=rad, fill=255)

    # Top-right tile rotated 45 degrees (Fluent signature app store / widget motif)
    tile = Image.new("L", (300, 300), 0)
    td = ImageDraw.Draw(tile)
    td.rounded_rectangle([87, 87, 87 + sz, 87 + sz], radius=rad, fill=255)
    tile_rot = tile.rotate(45, resample=Image.Resampling.BICUBIC)
    tx = int(cx + gap + sz // 2 - 150 + 8)
    ty = int(cy - gap - sz // 2 - 150 - 8)
    m.paste(tile_rot, (tx, ty), tile_rot)
    return m


THEMES = {
    "proyectos": {
        "label": "Proyectos",
        "back_top": (30, 64, 175, 255),      # Deep Royal Blue
        "back_bot": (23, 37, 84, 255),
        "front_top": (59, 130, 246, 255),    # Electric Fluent Blue
        "front_bot": (29, 78, 216, 255),
        "accent_glow": (56, 189, 248, 135),  # Cyan glow
        "emblem_fn": make_emblem_proyectos,
    },
    "escuela": {
        "label": "Escuela",
        "back_top": (6, 95, 70, 255),        # Deep Emerald
        "back_bot": (4, 47, 46, 255),
        "front_top": (16, 185, 129, 255),    # Fluent Mint / Emerald
        "front_bot": (5, 150, 105, 255),
        "accent_glow": (110, 231, 183, 135), # Mint glow
        "emblem_fn": make_emblem_escuela,
    },
    "redes": {
        "label": "Redes y Scripts",
        "back_top": (180, 83, 9, 255),       # Deep Amber
        "back_bot": (120, 53, 15, 255),
        "front_top": (245, 158, 11, 255),    # Fluent Gold / Amber
        "front_bot": (217, 119, 6, 255),
        "accent_glow": (253, 224, 71, 135),  # Warm Gold glow
        "emblem_fn": make_emblem_redes,
    },
    "juegos": {
        "label": "Juegos",
        "back_top": (91, 33, 182, 255),      # Deep Royal Purple
        "back_bot": (59, 7, 100, 255),
        "front_top": (139, 92, 246, 255),    # Fluent Violet
        "front_bot": (109, 40, 217, 255),
        "accent_glow": (216, 180, 254, 135), # Neon Lavender glow
        "emblem_fn": make_emblem_juegos,
    },
    "apps": {
        "label": "Apps y Herramientas",
        "back_top": (157, 23, 77, 255),      # Deep Rose/Crimson
        "back_bot": (80, 7, 36, 255),
        "front_top": (244, 63, 94, 255),     # Fluent Coral Rose
        "front_bot": (190, 18, 60, 255),
        "accent_glow": (253, 164, 175, 135), # Soft Rose glow
        "emblem_fn": make_emblem_apps,
    },
}

def main():
    out_dir = r"C:\Users\Admin\AppData\Local\DesktopIcons"
    os.makedirs(out_dir, exist_ok=True)
    scratch_dir = os.path.dirname(os.path.abspath(__file__))

    preview_sheet = Image.new("RGBA", (5 * 300 + 60, 340), (24, 26, 32, 255))

    for idx, (key, cfg) in enumerate(THEMES.items()):
        base = draw_fluent_folder_base(cfg)
        emb = cfg["emblem_fn"]()
        final_1024 = paste_emblem_with_3d_effect(base, emb)

        # Save 256x256 PNG and multi-size ICO
        icon_256 = final_1024.resize((256, 256), Image.Resampling.LANCZOS)
        ico_path = os.path.join(out_dir, f"{key}.ico")
        icon_256.save(
            ico_path,
            format="ICO",
            sizes=[(256, 256), (128, 128), (64, 64), (48, 48), (32, 32), (16, 16)]
        )
        print(f"Saved {ico_path}")

        # Add to preview sheet
        thumb = final_1024.resize((260, 260), Image.Resampling.LANCZOS)
        px = 30 + idx * 300
        preview_sheet.paste(thumb, (px, 40), thumb)

    preview_path = os.path.join(scratch_dir, "icons_preview.png")
    preview_sheet.save(preview_path)
    print(f"Saved preview to {preview_path}")

if __name__ == "__main__":
    main()

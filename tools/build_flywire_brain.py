#!/usr/bin/env python3
"""
build_flywire_brain.py — Offline asset builder for the Porygon FlyWire / VFB neural visualizer.

Downloads (once, cached in tools/cache/) from Virtual Fly Brain (VFB):
  * JRC2018Unisex template surface mesh (VFB_00101567)
  * 46 neuropil "painted domain" meshes registered to JRC2018Unisex
  * ~280 real FlyWire (Dorkenwald et al. 2023/2024, FAFB v783) neuron skeletons registered to JRC2018Unisex,
    selected along REAL anatomical pathways using real synapse weights from the VFB knowledge base:
        optic ingress (L1 -> Mi1/Tm3 -> T4/T5) -> looming detectors (LPLC2 / LC4) -> Giant Fiber escape (DNp01)
        olfactory signatures (ORN Or67d -> DA1 PN) -> Mushroom Body memory (KC -> MBON, PAM/PPL1 DAN)
        central complex decision (ring -> EPG / Delta7 / PFN) -> descending motor neurons (MDN, DNs)
Outputs:
  Models/flywire_brain.bin        compact little-endian binary consumed by HackerWallpaper.exe
  Models/flywire_brain_manifest.json  human-readable summary + attribution

Data licence: FlyWire data CC BY-NC 4.0 (FlyWire Consortium; Dorkenwald et al. 2024, Schlegel et al. 2024).
VFB templates/domains: CC BY 4.0 (Virtual Fly Brain; Court et al. 2023). Non-commercial personal use.
"""
import io, json, os, random, struct, sys, time
from concurrent.futures import ThreadPoolExecutor

import numpy as np
import requests
import trimesh
import fast_simplification
import flywire_synapses as FS

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
CACHE = os.path.join(ROOT, "tools", "cache")
OUT_BIN = os.path.join(ROOT, "Models", "flywire_brain.bin")
OUT_JSON = os.path.join(ROOT, "Models", "flywire_brain_manifest.json")
os.makedirs(CACHE, exist_ok=True)

PDB = "https://pdb.virtualflybrain.org/db/neo4j/tx/commit"
AUTH = ("neo4j", "vfb")
TEMPLATE = "VFB_00101567"
DS = "Dorkenwald2023"
random.seed(783)

STAGES = ["INGRESS_OPTIC", "SIGNATURE_OLF", "THREAT_LOOMING", "MEMORY_MB", "DECISION_CX", "ACTION_DN", "AUDITORY_EXFIL"]
ANNOT_URL = "https://raw.githubusercontent.com/flyconnectome/flywire_annotations/main/supplemental_files/Supplemental_file1_neuron_annotations.tsv"


def log(*a):
    print(time.strftime("[%H:%M:%S]"), *a, flush=True)


def q(cypher, params=None, retries=4):
    for attempt in range(retries):
        try:
            r = requests.post(PDB, json={"statements": [{"statement": cypher, "parameters": params or {}}]},
                              auth=AUTH, timeout=180)
            r.raise_for_status()
            j = r.json()
            if j.get("errors"):
                raise RuntimeError(j["errors"])
            res = j["results"][0]
            return [dict(zip(res["columns"], row["row"])) for row in res["data"]]
        except Exception as e:
            log("query retry", attempt + 1, str(e)[:160])
            time.sleep(2 + attempt * 3)
    raise RuntimeError("query failed: " + cypher[:120])


def fetch(url, name):
    path = os.path.join(CACHE, name)
    if os.path.exists(path) and os.path.getsize(path) > 0:
        with open(path, "rb") as f:
            return f.read()
    for attempt in range(4):
        try:
            r = requests.get(url, timeout=120)
            r.raise_for_status()
            with open(path, "wb") as f:
                f.write(r.content)
            return r.content
        except Exception as e:
            log("download retry", attempt + 1, url, str(e)[:100])
            time.sleep(2 + attempt * 2)
    return None


# ----------------------------------------------------------------------------------------------
# 1. Neuron selection along real pathways (real synapse weights from VFB KB)
# ----------------------------------------------------------------------------------------------
def cls_where(spec):
    kind, val = spec
    if kind == "eq":
        return "c.label = $cls"
    if kind == "contains":
        return "c.label CONTAINS $cls"
    return "c.label STARTS WITH $cls"


def pick_type(spec, n, exclude):
    rows = q("""MATCH (m:Neuron)-[:has_source]->({short_form:$ds}) MATCH (m)-[:INSTANCEOF]->(c:Class) WHERE %s
               RETURN DISTINCT m.short_form AS id LIMIT 400""" % cls_where(spec), {"ds": DS, "cls": spec[1]})
    ids = [r["id"] for r in rows if r["id"] not in exclude]
    random.shuffle(ids)
    return ids[:n]


def top_partners(selected, spec, direction, n, exclude):
    """direction='up': partners that synapse ONTO selected; 'down': partners selected synapse onto."""
    if not selected:
        return []
    if direction == "up":
        pat = "(m:Neuron)-[s:synapsed_to]->(t:Neuron)"
    else:
        pat = "(t:Neuron)-[s:synapsed_to]->(m:Neuron)"
    rows = q("""MATCH %s WHERE t.short_form IN $sel
               MATCH (m)-[:INSTANCEOF]->(c:Class) WHERE %s
               MATCH (m)-[:has_source]->({short_form:$ds})
               RETURN m.short_form AS id, sum(s.weight[0]) AS w ORDER BY w DESC LIMIT %d""" % (pat, cls_where(spec), n * 3 + 10),
             {"sel": list(selected), "cls": spec[1], "ds": DS})
    out = []
    for r in rows:
        if r["id"] in exclude or r["id"] in out:
            continue
        out.append(r["id"])
        if len(out) >= n:
            break
    return out


OVERSAMPLE = 1.4   # extra candidates per type; the anatomical QC step trims back to the target count
SCALE = 2.0        # 1.0 = ~500 neurons, 2.0 = ~1000 neurons (types with fewer real cells are capped automatically)
SYN_CAP = 420      # max real synapse dots stored per neuron and direction (outputs / inputs), randomly subsampled


def select_neurons():
    sel = {}  # id -> (stage, type_short)
    order = []
    targets = {}

    def add(ids, stage, tshort, target):
        target = int(round(target * SCALE))
        got = []
        for i in ids:
            if i not in sel:
                sel[i] = (stage, tshort)
                order.append(i)
                got.append(i)
        targets[tshort] = targets.get(tshort, 0) + target
        log("  +%-3d (target %-3d) %-10s %s (total %d)" % (len(got), target, tshort, STAGES[stage], len(sel)))
        return got

    def n_os(n):
        return int(round(n * OVERSAMPLE))

    def tp_or_pick(seeds, spec, direction, n):
        k = n_os(int(round(n * SCALE)))
        got = top_partners(seeds, spec, direction, k, sel) if seeds else []
        if len(got) < k:
            got += [x for x in pick_type(spec, k - len(got) + 6, set(sel) | set(got)) if x not in got][:k - len(got)]
        return got

    log("Selecting ~%d neurons along real pathways ..." % int(500 * SCALE))
    # --- ACTION seeds
    gf = add(pick_type(("eq", "giant fiber neuron"), 2, sel), 5, "GF/DNp01", 1)
    add(pick_type(("eq", "adult moonwalker descending neuron"), 4, sel), 5, "MDN", 2)
    # --- THREAT looming detectors -> GF (real top inputs)
    lplc2 = add(tp_or_pick(gf, ("eq", "lobula plate-lobula columnar neuron LPLC2"), "up", 30), 2, "LPLC2", 30)
    lc4 = add(tp_or_pick(gf, ("eq", "lobula columnar neuron LC4"), "up", 26), 2, "LC4", 26)
    add(tp_or_pick([], ("eq", "lobula columnar neuron LC6"), "up", 14), 2, "LC6", 14)
    add(tp_or_pick([], ("eq", "lobula columnar neuron LC11"), "up", 14), 2, "LC11", 14)
    # --- INGRESS optic chain feeding the looming detectors
    t4 = add(tp_or_pick(lplc2, ("eq", "T4a neuron"), "up", 22), 0, "T4a", 22)
    add(tp_or_pick(lplc2 + lc4, ("eq", "T5a neuron"), "up", 20), 0, "T5a", 20)
    mi1 = add(tp_or_pick(t4, ("eq", "medulla intrinsic neuron Mi1"), "up", 22), 0, "Mi1", 22)
    add(tp_or_pick(t4, ("eq", "transmedullary neuron Tm3"), "up", 18), 0, "Tm3", 18)
    add(tp_or_pick(mi1, ("eq", "lamina monopolar neuron L1"), "up", 18), 0, "L1", 18)
    # --- SIGNATURE olfactory chain
    pn = add(tp_or_pick([], ("eq", "adult antennal lobe projection neuron DA1 lPN"), "up", 10), 1, "PN-DA1", 10)
    pn += add(tp_or_pick([], ("eq", "adult antennal lobe projection neuron DA2 lPN"), "up", 8), 1, "PN-DA2", 8)
    pn += add(tp_or_pick([], ("eq", "adult uniglomerular antennal lobe projection neuron lPN"), "up", 10), 1, "uPN", 10)
    add(tp_or_pick(pn, ("eq", "adult olfactory receptor neuron Or67d"), "up", 24), 1, "ORN-Or67d", 24)
    # --- MEMORY mushroom body
    kc = add(tp_or_pick(pn, ("eq", "gamma main Kenyon cell"), "down", 40), 3, "KCg", 40)
    kc += add(tp_or_pick(pn, ("eq", "alpha/beta Kenyon cell"), "down", 30), 3, "KCab", 30)
    kc += add(tp_or_pick(pn, ("eq", "alpha'/beta' middle Kenyon cell"), "down", 16), 3, "KCa'b'", 16)
    mbon = add(tp_or_pick(kc, ("contains", "mushroom body output neuron"), "down", 24), 3, "MBON", 24)
    add(tp_or_pick(kc, ("contains", "dopaminergic PAM neuron"), "up", 20), 3, "PAM-DAN", 20)
    add(tp_or_pick(kc, ("contains", "PPL1"), "up", 6), 3, "PPL1-DAN", 6)
    # --- DECISION central complex
    ring = add(tp_or_pick([], ("eq", "adult ellipsoid body ring neuron R2"), "up", 12), 4, "ER2", 12)
    ring += add(tp_or_pick([], ("eq", "adult ellipsoid body ring neuron R4d"), "up", 10), 4, "ER4d", 10)
    epg = add(tp_or_pick(ring, ("eq", "adult ellipsoid body-protocerebral bridge 1 glomerulus-dorsal/ventral gall neuron"), "down", 20), 4, "EPG", 20)
    add(tp_or_pick(epg, ("eq", "adult intrinsic protocerebral bridge 18 glomeruli-delta7 neuron"), "down", 10), 4, "Delta7", 10)
    add(tp_or_pick(epg, ("contains", "protocerebral bridge 1 glomerulus-fan-shaped body-nodulus"), "down", 14), 4, "PFN", 14)
    # --- AUDITORY / mechanosensory (exfiltration): Johnston organ -> AMMC -> WED / Giant Fiber (real GF inputs)
    jo_a = add(tp_or_pick(gf, ("eq", "zone A Johnston organ neuron"), "up", 14), 6, "JO-A", 14)
    jo_b = add(tp_or_pick(gf, ("eq", "zone B Johnston organ neuron"), "up", 14), 6, "JO-B", 14)
    add(tp_or_pick(gf, ("eq", "antennal mechanosensory and motor center A1 projection neuron"), "up", 4), 6, "AMMC-A1", 4)
    add(tp_or_pick(jo_a + jo_b, ("contains", "adult wedge projection neuron"), "down", 10), 6, "WED-PN", 10)
    # --- ACTION descending neurons driven by MB/CX outputs (real downstream partners, fallback to anterior DNs)
    add(tp_or_pick(mbon + epg, ("contains", "descending neuron"), "down", 14), 5, "DN", 14)
    log("Candidates: %d (target %d)" % (len(sel), sum(targets.values())))
    return sel, order, targets


def fetch_meta(ids):
    meta = {}
    for k in range(0, len(ids), 150):
        chunk = ids[k:k + 150]
        rows = q("""MATCH (n:Neuron) WHERE n.short_form IN $ids
                   MATCH (n)<-[:depicts]-(ch)-[r:in_register_with]->(tc)-[:depicts]->({short_form:$tpl})
                   OPTIONAL MATCH (n)-[:INSTANCEOF]->(c:Class)
                   RETURN n.short_form AS id, n.label AS label, r.folder AS folder, collect(DISTINCT c.label) AS cls""",
                 {"ids": chunk, "tpl": TEMPLATE})
        for r in rows:
            folder = r["folder"][0] if isinstance(r["folder"], list) else r["folder"]
            meta[r["id"]] = {"label": r["label"], "folder": folder, "cls": r["cls"]}
    return meta


def fetch_edges(ids):
    rows = q("""MATCH (a:Neuron)-[s:synapsed_to]->(b:Neuron) WHERE a.short_form IN $ids AND b.short_form IN $ids
               RETURN a.short_form AS a, b.short_form AS b, s.weight[0] AS w""", {"ids": ids})
    return [(r["a"], r["b"], int(r["w"] or 0)) for r in rows if (r["w"] or 0) >= 2 and r["a"] != r["b"]]


def nt_of(cls_list):
    s = " ".join(cls_list).lower()
    for key, tag in (("cholinergic", "ACh"), ("gabaergic", "GABA"), ("glutamatergic", "Glu"), ("dopaminergic", "DA"),
                     ("serotonergic", "5-HT"), ("octopaminergic", "OA")):
        if key in s:
            return tag
    if "dopaminergic" in s or "pam" in s or "ppl1" in s:
        return "DA"
    return "?"


def type_of(cls_list, fallback):
    best = None
    for c in cls_list:
        cl = c.lower()
        if cl.startswith("adult cholinergic") or cl.startswith("adult gabaergic") or cl.startswith("adult glutamatergic") \
                or "hemilineage" in cl or "lineage neuron" in cl or cl.startswith("adult dopaminergic neuron"):
            continue
        if best is None or len(c) < len(best):
            best = c
    return best or fallback


# ----------------------------------------------------------------------------------------------
# 2. Geometry
# ----------------------------------------------------------------------------------------------

# ----------------------------------------------------------------------------------------------
# Anatomical QC helpers: JRC2018U brain occupancy mask + FlyWire neurotransmitter predictions
# ----------------------------------------------------------------------------------------------
def build_template_mask(v, f, pitch=3.0, tol_vox=1):
    """Solid occupancy grid of the full-resolution template surface; returns inside(points)->bool."""
    from scipy import ndimage
    m = trimesh.Trimesh(vertices=v, faces=f, process=False)
    vg = m.voxelized(pitch=pitch)
    shell = ndimage.binary_dilation(np.asarray(vg.matrix), iterations=1)

    # Robust solid fill: a voxel is inside only if the surface shell bounds it on BOTH sides along ALL 3 axes
    # (binary_fill_holes leaks through diagonal gaps of a voxelised surface; this test cannot leak).
    def scan(sh, axis):
        a = np.cumsum(sh, axis=axis) > 0
        b = np.flip(np.cumsum(np.flip(sh, axis=axis), axis=axis) > 0, axis=axis)
        return a & b
    mat = (scan(shell, 0) & scan(shell, 1) & scan(shell, 2)) | shell
    mat = ndimage.binary_dilation(mat, iterations=tol_vox)
    T = np.asarray(vg.transform)
    inv = np.linalg.inv(T)
    shape = np.array(mat.shape)
    log("  template mask: %s voxels @ %.1f um (fill %.1f%%)" % (tuple(shape), pitch, 100.0 * mat.mean()))

    def inside(pts):
        pts = np.asarray(pts, dtype=np.float64)
        h = np.c_[pts, np.ones(len(pts))] @ inv.T
        ijk = np.round(h[:, :3]).astype(int)
        ok = np.all((ijk >= 0) & (ijk < shape), axis=1)
        out = np.zeros(len(pts), dtype=bool)
        out[ok] = mat[ijk[ok, 0], ijk[ok, 1], ijk[ok, 2]]
        return out
    return inside


NT_SHORT = {"acetylcholine": "ACh", "gaba": "GABA", "glutamate": "Glu", "dopamine": "DA", "serotonin": "5-HT",
            "octopamine": "OA", "histamine": "HA"}


def load_nt_annotations():
    """FlyWire neuron annotations (Schlegel et al. 2024): known NT if available, else predicted NT (Eckstein et al. 2024)."""
    raw = fetch(ANNOT_URL, "flywire_neuron_annotations.tsv")
    out = {}
    if not raw:
        log("  ! NT annotations unavailable")
        return out
    lines = raw.decode("utf-8", "ignore").splitlines()
    hdr = lines[0].split("\t")
    ci = {h: i for i, h in enumerate(hdr)}
    for line in lines[1:]:
        p = line.split("\t")
        if len(p) < len(hdr) - 4:
            continue
        p += [""] * (len(hdr) - len(p))
        vfb = p[ci["vfb_id"]].strip()
        if not vfb:
            continue
        known = p[ci["known_nt"]].strip().lower()
        top = p[ci["top_nt"]].strip().lower()
        conf = p[ci["top_nt_conf"]].strip()
        if known:
            first = known.split(",")[0].split(";")[0].strip()
            out[vfb] = NT_SHORT.get(first, first[:6]) + " (conocido)"
        elif top:
            try:
                out[vfb] = "%s (pred %.2f)" % (NT_SHORT.get(top, top[:6]), float(conf))
            except ValueError:
                out[vfb] = NT_SHORT.get(top, top[:6]) + " (pred)"
    log("  NT annotations loaded: %d neurons" % len(out))
    return out


def load_obj(data):
    m = trimesh.load(io.BytesIO(data), file_type="obj", force="mesh", process=True)
    return np.asarray(m.vertices, dtype=np.float64), np.asarray(m.faces, dtype=np.int64)


def decimate(v, f, target_faces):
    if len(f) <= target_faces:
        return v, f
    ratio = 1.0 - float(target_faces) / float(len(f))
    v2, f2 = fast_simplification.simplify(v.astype(np.float32), f.astype(np.int32), target_reduction=ratio)
    return np.asarray(v2, dtype=np.float64), np.asarray(f2, dtype=np.int64)


def vertex_normals(v, f):
    m = trimesh.Trimesh(vertices=v, faces=f, process=False)
    return np.asarray(m.vertex_normals, dtype=np.float64)


def parse_swc(data):
    nodes = {}
    for line in data.decode("utf-8", "ignore").splitlines():
        line = line.strip()
        if not line or line.startswith("#"):
            continue
        p = line.split()
        if len(p) < 7:
            continue
        try:
            nid = int(float(p[0])); x, y, z = float(p[2]), float(p[3]), float(p[4]); par = int(float(p[6]))
        except ValueError:
            continue
        nodes[nid] = (x, y, z, par)
    return nodes


def simplify_skeleton(nodes, step=2.6):
    """Keeps root, branch points and leaves; resamples unbranched runs every `step` µm.
    Returns (pts Nx3, parent idx array, root idx)."""
    children = {}
    root = None
    for nid, (x, y, z, par) in nodes.items():
        if par < 0 or par not in nodes:
            if root is None:
                root = nid
            continue
        children.setdefault(par, []).append(nid)
    if root is None:
        return None
    keep_pts, keep_par = [], []
    idx_of = {}

    def P(n):
        return np.array(nodes[n][:3])

    idx_of[root] = 0
    keep_pts.append(P(root)); keep_par.append(-1)
    stack = [(c, 0, 0.0) for c in children.get(root, [])]
    while stack:
        n, last_kept, acc = stack.pop()
        par = nodes[n][3]
        acc += float(np.linalg.norm(P(n) - P(par)))
        ch = children.get(n, [])
        if len(ch) != 1 or acc >= step:
            idx_of[n] = len(keep_pts)
            keep_pts.append(P(n)); keep_par.append(last_kept)
            last_kept = idx_of[n]; acc = 0.0
        for c in ch:
            stack.append((c, last_kept, acc))
    return np.array(keep_pts), np.array(keep_par, dtype=np.int64), 0


def path_to_root(par, i):
    out = [i]
    while par[out[-1]] >= 0 and len(out) < 100000:
        out.append(par[out[-1]])
    return out[::-1]


def resample_path(pts, k):
    if len(pts) <= k:
        return pts
    sel = np.linspace(0, len(pts) - 1, k).round().astype(int)
    return pts[sel]


# ----------------------------------------------------------------------------------------------
# 3. Neuropil colors (Codex / Virtual Fly Brain inspired palette)
# ----------------------------------------------------------------------------------------------
NEUROPIL_GROUPS = {
    "optic": (["ME", "AME", "LO", "LOP"], (64, 156, 255)),
    "mb": (["CA", "PED", "aL", "a\\'L", "bL", "b\\'L", "gL", "a'L", "b'L"], (255, 92, 200)),
    "cx": (["FB", "EB", "PB", "NO"], (255, 196, 48)),
    "lh": (["LH"], (120, 230, 90)),
    "al": (["AL"], (64, 232, 220)),
    "vlnp": (["AVLP", "PVLP", "PLP", "WED", "AOTU"], (150, 120, 255)),
    "snp": (["SLP", "SIP", "SMP"], (255, 140, 90)),
    "inp": (["CRE", "ROB", "RUB", "SCL", "ICL", "IB", "ATL", "LAL", "BU", "GA"], (200, 170, 140)),
    "vmnp": (["VES", "EPA", "GOR", "SPS", "IPS"], (110, 200, 160)),
    "sez": (["SAD", "AMMC", "FLA", "CAN", "PRW", "GNG"], (240, 110, 110)),
}


def group_of(name):
    for g, (names, col) in NEUROPIL_GROUPS.items():
        if name in names:
            return g, col
    return "other", (160, 170, 180)


GROUP_IDS = {g: i for i, g in enumerate(list(NEUROPIL_GROUPS.keys()) + ["other"])}


# ----------------------------------------------------------------------------------------------
# 4. Binary writer
# ----------------------------------------------------------------------------------------------
class W:
    def __init__(self):
        self.b = io.BytesIO()

    def i32(self, v): self.b.write(struct.pack("<i", int(v)))
    def u8(self, v): self.b.write(struct.pack("<B", int(v) & 255))
    def f32(self, v): self.b.write(struct.pack("<f", float(v)))
    def s(self, txt):
        d = (txt or "").encode("utf-8")
        self.i32(len(d)); self.b.write(d)
    def farr(self, a):
        a = np.ascontiguousarray(a, dtype="<f4").ravel()
        self.i32(a.size); self.b.write(a.tobytes())
    def iarr(self, a):
        a = np.ascontiguousarray(a, dtype="<i4").ravel()
        self.i32(a.size); self.b.write(a.tobytes())


def main():
    t0 = time.time()
    # --- Template
    log("Downloading JRC2018Unisex template mesh ...")
    tpl_raw = fetch("https://www.virtualflybrain.org/data/VFB/i/0010/1567/VFB_00101567/volume_man.obj", "template_JRC2018U.obj")
    tv, tf = load_obj(tpl_raw)
    log("  template raw: %d verts / %d faces, bounds %s .. %s" % (len(tv), len(tf), tv.min(0).round(1), tv.max(0).round(1)))
    inside = build_template_mask(tv, tf)
    tv, tf = decimate(tv, tf, 26000)
    tn = vertex_normals(tv, tf)
    log("  template decimated: %d verts / %d faces" % (len(tv), len(tf)))
    center = (tv.min(0) + tv.max(0)) * 0.5
    extent = float((tv.max(0) - tv.min(0)).max())

    # --- Neuropils
    log("Querying neuropil domains ...")
    doms = q("""MATCH ({short_form:$tpl})<-[:depicts]-(tc)<-[r:in_register_with]-(c)-[:depicts]->(d)
               WHERE r.index IS NOT NULL RETURN d.short_form AS id, d.label AS label, r.folder AS folder ORDER BY r.index""", {"tpl": TEMPLATE})
    neuropils = []

    def dl_dom(d):
        folder = d["folder"][0] if isinstance(d["folder"], list) else d["folder"]
        name = d["label"].split(" on JRC2018Unisex")[0].strip()
        raw = fetch(folder.replace("http://", "https://") + "volume_man.obj", "np_%s.obj" % d["id"])
        return name, d["id"], raw

    with ThreadPoolExecutor(8) as ex:
        for name, vid, raw in ex.map(dl_dom, doms):
            if not raw:
                log("  ! missing neuropil", name); continue
            try:
                v, f = load_obj(raw)
            except Exception as e:
                log("  ! bad neuropil", name, e); continue
            v, f = decimate(v, f, 2600)
            n = vertex_normals(v, f)
            g, col = group_of(name.replace("\\'", "'"))
            neuropils.append({"name": name.replace("\\'", "'"), "id": vid, "group": g, "color": col, "v": v, "f": f, "n": n,
                              "centroid": v.mean(0)})
    log("  neuropils loaded: %d (%d tris)" % (len(neuropils), sum(len(p["f"]) for p in neuropils)))

    # --- Neurons
    sel, order, targets = select_neurons()
    annot = load_nt_annotations()
    meta = fetch_meta(order)
    order = [i for i in order if i in meta]
    log("Neurons with JRC2018U registration: %d" % len(order))

    def dl_swc(nid):
        return nid, fetch(meta[nid]["folder"].replace("http://", "https://") + "volume.swc", "swc_%s.swc" % nid)

    neurons = []
    qc_rejected = 0
    clipped_total = 0
    kept_per_type = {}
    with ThreadPoolExecutor(12) as ex:
        results = dict(ex.map(dl_swc, order))
    for nid in order:
        raw = results.get(nid)
        if not raw:
            log("  ! missing swc", nid); continue
        nodes = parse_swc(raw)
        sk = simplify_skeleton(nodes)
        if sk is None or len(sk[0]) < 4:
            continue
        pts, par, root = sk
        stage, tshort = sel[nid]
        # --- Anatomical QC: keep only arbor inside the JRC2018U brain (+6 um); drop badly registered neurons
        ins = inside(pts)
        frac = float(ins.mean())
        min_frac = 0.20 if tshort in ("ORN-Or67d", "JO-A", "JO-B", "L1") else (0.35 if stage == 5 else 0.60)
        if frac < min_frac:
            qc_rejected += 1
            continue
        if kept_per_type.get(tshort, 0) >= targets.get(tshort, 9999):
            continue
        # main path: root -> farthest (path length) node, restricted to the in-brain part
        depth = np.zeros(len(pts))
        for i in range(1, len(pts)):
            depth[i] = depth[par[i]] + np.linalg.norm(pts[i] - pts[par[i]])
        far = int(np.argmax(np.where(ins, depth, -1.0)))
        mp_idx = [k for k in path_to_root(par, far) if ins[k]]
        if len(mp_idx) < 2:
            mp_idx = [k for k in range(len(pts)) if ins[k]][:2] or [0, 0]
        main_path = resample_path(pts[mp_idx], 40)
        segs = np.array([[pts[par[i]], pts[i]] for i in range(1, len(pts)) if par[i] >= 0 and ins[i] and ins[par[i]]])
        if len(segs) < 3:
            qc_rejected += 1
            continue
        clipped_total += int(len(pts) - 1 - len(segs))
        root_pt = pts[0] if ins[0] else pts[mp_idx[0]]
        m = meta[nid]
        fw = m["label"].split("FlyWire:")[-1].rstrip(")") if "FlyWire:" in m["label"] else ""
        nt = annot.get(nid.replace("VFB_", ""), None) or nt_of(m["cls"])
        kept_per_type[tshort] = kept_per_type.get(tshort, 0) + 1
        neurons.append({"id": nid, "fw": fw, "label": m["label"], "type": type_of(m["cls"], tshort), "short": tshort,
                        "nt": nt, "stage": stage, "pts": pts, "par": par, "ins": ins, "segs": segs, "main": main_path,
                        "root": root_pt, "centroid": pts[ins].mean(0), "side": 1 if root_pt[0] > center[0] else 0})
    log("  neurons kept: %d (QC rejected %d, clipped %d out-of-brain segments), total segments %d" %
        (len(neurons), qc_rejected, clipped_total, sum(len(n["segs"]) for n in neurons)))
    log("  per type: %s" % json.dumps(kept_per_type))

    # --- Edges: path along the PRE arbor from its root to the closest postsynaptic skeleton contact
    idx = {n["id"]: i for i, n in enumerate(neurons)}
    edges_raw = [(a, b, w) for (a, b, w) in fetch_edges([n["id"] for n in neurons]) if a in idx and b in idx]
    edges_raw.sort(key=lambda e: -e[2])
    edges_raw = edges_raw[:int(3200 * SCALE)]
    edges = []
    from scipy.spatial import cKDTree
    trees = {}
    for a, b, w in edges_raw:
        A, B = neurons[idx[a]], neurons[idx[b]]
        ia_all = np.nonzero(A["ins"])[0]
        ib_all = np.nonzero(B["ins"])[0]
        if len(ia_all) == 0 or len(ib_all) == 0:
            continue
        if b not in trees:
            trees[b] = cKDTree(B["pts"][ib_all])
        dist, j = trees[b].query(A["pts"][ia_all], k=1)
        kbest = int(np.argmin(dist))
        ia = int(ia_all[kbest]); ib = int(ib_all[int(j[kbest])])
        contact = (A["pts"][ia] + B["pts"][ib]) * 0.5
        pidx = [k for k in path_to_root(A["par"], ia) if A["ins"][k]]
        if len(pidx) < 2:
            pidx = [ia, ia]
        path = resample_path(A["pts"][pidx], 18)
        path = np.vstack([path, contact[None, :]])
        edges.append({"pre": idx[a], "post": idx[b], "w": w, "path": path})
    log("  edges kept: %d" % len(edges))

    # --- Write binary (all coordinates centred on template centre, µm, Y up = -JRC Y, Z toward viewer = -JRC Z)
    def tx(a):
        a = np.asarray(a, dtype=np.float64) - center
        return np.stack([a[..., 0], -a[..., 1], -a[..., 2]], axis=-1)

    def txn(nrm):
        nrm = np.asarray(nrm, dtype=np.float64)
        return np.stack([nrm[..., 0], -nrm[..., 1], -nrm[..., 2]], axis=-1)

    w = W()
    w.b.write(b"FWB1"); w.i32(1)
    w.s("Data: FlyWire Consortium (Dorkenwald et al. 2024, Schlegel et al. 2024) CC BY-NC 4.0 / Virtual Fly Brain JRC2018Unisex VFB_00101567")
    w.f32(extent)
    # template
    w.farr(tx(tv)); w.farr(txn(tn)); w.iarr(tf)
    # neuropils
    w.i32(len(neuropils))
    for p in neuropils:
        w.s(p["name"]); w.s(p["id"]); w.u8(GROUP_IDS[p["group"]])
        w.u8(p["color"][0]); w.u8(p["color"][1]); w.u8(p["color"][2])
        w.farr(tx(p["centroid"])); w.farr(tx(p["v"])); w.farr(txn(p["n"])); w.iarr(p["f"])
    # neurons
    w.i32(len(neurons))
    for n in neurons:
        w.s(n["id"]); w.s(n["fw"]); w.s(n["type"]); w.s(n["short"]); w.s(n["nt"])
        w.u8(n["stage"]); w.u8(n["side"])
        w.farr(tx(n["root"])); w.farr(tx(n["centroid"]))
        w.farr(tx(n["segs"].reshape(-1, 3))); w.farr(tx(n["main"]))
    # edges
    w.i32(len(edges))
    for e in edges:
        w.i32(e["pre"]); w.i32(e["post"]); w.i32(e["w"]); w.farr(tx(e["path"]))
    w.b.write(b"END1")
    with open(OUT_BIN, "wb") as f:
        f.write(w.b.getvalue())

    manifest = {
        "template": TEMPLATE, "dataset": DS, "extent_um": extent,
        "template_tris": int(len(tf)), "neuropils": [p["name"] for p in neuropils],
        "neuropil_tris": int(sum(len(p["f"]) for p in neuropils)),
        "neurons": len(neurons), "segments": int(sum(len(n["segs"]) for n in neurons)), "edges": len(edges),
        "real_synapse_dots": 0, "real_synapses_total": 0,
        "per_type": {}, "per_stage": {},
        "attribution": "FlyWire Consortium (Dorkenwald et al. 2024 Nature; Schlegel et al. 2024 Nature), CC BY-NC 4.0; Virtual Fly Brain (Court et al. 2023), JRC2018Unisex (Bogovic et al. 2020).",
    }
    for n in neurons:
        manifest["per_type"][n["short"]] = manifest["per_type"].get(n["short"], 0) + 1
        manifest["per_stage"][STAGES[n["stage"]]] = manifest["per_stage"].get(STAGES[n["stage"]], 0) + 1
    with open(OUT_JSON, "w", encoding="utf-8") as f:
        json.dump(manifest, f, indent=2, ensure_ascii=False)
    FS.patch_flywire_bin(OUT_BIN, OUT_JSON, CACHE, log)
    log("Wrote %s (%.2f MB) in %.0fs" % (OUT_BIN, os.path.getsize(OUT_BIN) / 1e6, time.time() - t0))
    log(json.dumps({k: manifest[k] for k in ("template_tris", "neuropil_tris", "neurons", "segments", "edges", "per_stage")}))


if __name__ == "__main__":
    main()

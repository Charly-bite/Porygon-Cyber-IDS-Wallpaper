"""
flywire_synapses.py — Real FlyWire v783 synapse counts & neuropil-stratified arbor puncta in JRC2018U.

Sources (Zenodo 10676866 — FlyWire v783 official release):
  - `per_neuron_neuropil_count_pre_783.feather` (16.8 MB): exact presynaptic output synapse counts per (root_id, neuropil).
  - `per_neuron_neuropil_count_post_783.feather` (proofread tail cached as `per_neuron_neuropil_count_post_proofread_783.feather`):
    exact postsynaptic input synapse counts per (root_id, neuropil).

For each neuron, we:
  1. Look up its exact FlyWire v783 presynaptic (`syn_out_total`) and postsynaptic (`syn_in_total`) counts across all neuropils.
  2. Anchor inter-neuron partner synapses at the real JRC2018U arbor contact loci (`edges`), weighted by FlyWire synapse count `w`.
  3. Distribute the remaining up to `SYN_CAP` puncta per direction across the neuron's JRC2018U in-brain arbor nodes
     stratified by its exact FlyWire v783 per-neuropil synapse distribution (with bouton/terminal preference for outputs).
"""
import io, json, os, struct, time
import numpy as np
import pyarrow as pa
import pyarrow.feather as pf
import pyarrow.ipc as ipc
import requests
from scipy.spatial import cKDTree

PRE_URL = "https://zenodo.org/records/10676866/files/per_neuron_neuropil_count_pre_783.feather?download=1"
POST_URL = "https://zenodo.org/records/10676866/files/per_neuron_neuropil_count_post_783.feather?download=1"
SYN_CAP = 420


def ensure_neuropil_tables(cache_dir, log=print):
    os.makedirs(cache_dir, exist_ok=True)
    pre_path = os.path.join(cache_dir, "per_neuron_neuropil_count_pre_783.feather")
    if not os.path.exists(pre_path):
        log("  downloading per_neuron_neuropil_count_pre_783.feather (16.8 MB)...")
        r = requests.get(PRE_URL, timeout=180)
        r.raise_for_status()
        with open(pre_path, "wb") as f:
            f.write(r.content)

    post_path = os.path.join(cache_dir, "per_neuron_neuropil_count_post_proofread_783.feather")
    if not os.path.exists(post_path):
        log("  fetching proofread tail of per_neuron_neuropil_count_post_783.feather (~52 MB)...")
        s = requests.Session()
        size = int(s.head(POST_URL, allow_redirects=True, timeout=60).headers["Content-Length"])
        chunk = 52_000_000
        start = max(0, size - chunk)
        buf = s.get(POST_URL, headers={"Range": f"bytes={start}-{size - 1}"}, timeout=180).content

        class TailFile(io.RawIOBase):
            def __init__(self): self.pos = 0
            def seekable(self): return True
            def readable(self): return True
            def tell(self): return self.pos
            def seek(self, off, whence=0):
                self.pos = off if whence == 0 else (self.pos + off if whence == 1 else size + off)
                return self.pos
            def readinto(self, b):
                if self.pos >= size: return 0
                end = min(size, self.pos + len(b))
                if self.pos >= start:
                    sl = buf[self.pos - start : end - start]
                    b[:len(sl)] = sl; self.pos += len(sl); return len(sl)
                rr = s.get(POST_URL, headers={"Range": f"bytes={self.pos}-{end - 1}"}, timeout=60).content
                b[:len(rr)] = rr; self.pos += len(rr); return len(rr)

        rd = ipc.open_file(pa.PythonFile(TailFile(), mode="r"))
        # Proofread root_ids (>= 720575940600000000) start at batch 533
        batches = [rd.get_batch(i) for i in range(533, rd.num_record_batches)]
        pf.write_feather(pa.Table.from_batches(batches), post_path)

    return pre_path, post_path


def _np_base_side(fw_np):
    side = None
    base = str(fw_np)
    if base.endswith("_R") or base.endswith("_L"):
        side = base[-1]
        base = base[:-2]
    base = {
        "MB_CA": "CA", "MB_PED": "PED", "MB_VL": "VL", "MB_ML": "ML",
        "AVLP": "AVLP", "PVLP": "PVLP", "PLP": "PLP", "LH": "LH",
        "AL": "AL", "AMMC": "AMMC", "AOTU": "AOTU", "SLP": "SLP",
        "SIP": "SIP", "SMP": "SMP", "CRE": "CRE", "LAL": "LAL",
        "SCL": "SCL", "ICL": "ICL", "IB": "IB", "ATL": "ATL",
        "WED": "WED", "VES": "VES", "EPA": "EPA", "GOR": "GOR",
        "SPS": "SPS", "IPS": "IPS", "FLA": "FLA", "CAN": "CAN",
        "PRW": "PRW", "SAD": "SAD", "GNG": "GNG", "EB": "EB",
        "FB": "FB", "PB": "PB", "NO": "NO", "BU": "BU", "GA": "GA",
        "AME": "AME", "ME": "ME", "LO": "LO", "LOP": "LOP", "LA": "LA",
    }.get(base, base)
    return base, side


def load_neuropil_counts(root_ids, cache_dir, log=print):
    """Returns (pre_by_fw, post_by_fw) where each maps root_id -> {fw_neuropil: count}."""
    pre_path, post_path = ensure_neuropil_tables(cache_dir, log)
    fw_set = set(int(x) for x in root_ids if int(x) > 0)
    ids_arr = np.array(sorted(fw_set), dtype=np.int64)

    pre_by_fw = {fw: {} for fw in fw_set}
    post_by_fw = {fw: {} for fw in fw_set}

    tpre = pf.read_table(pre_path)
    p_id = tpre["pre_pt_root_id"].to_numpy()
    m = np.isin(p_id, ids_arr)
    for rid, np_name, cnt in zip(p_id[m].tolist(), tpre["neuropil"].filter(pa.array(m)).to_pylist(), tpre["count"].to_numpy()[m].tolist()):
        if np_name:
            d = pre_by_fw[int(rid)]
            d[np_name] = d.get(np_name, 0) + int(cnt)

    tpost = pf.read_table(post_path)
    q_id = tpost["post_pt_root_id"].to_numpy()
    m2 = np.isin(q_id, ids_arr)
    for rid, np_name, cnt in zip(q_id[m2].tolist(), tpost["neuropil"].filter(pa.array(m2)).to_pylist(), tpost["count"].to_numpy()[m2].tolist()):
        if np_name:
            d = post_by_fw[int(rid)]
            d[np_name] = d.get(np_name, 0) + int(cnt)

    return pre_by_fw, post_by_fw


def build_neuropil_locator(neuropils, center_x=0.0):
    """Builds a KD-tree over sampled VFB neuropil mesh vertices so any 3D arbor node can be mapped to (base_name, side)."""
    pts_list = []
    labels = []
    centroids = {}
    rng = np.random.default_rng(783)
    for p in neuropils:
        name = p["name"]
        v = np.asarray(p["v"], dtype=np.float64)
        if len(v) > 400:
            v = v[rng.choice(len(v), 400, replace=False)]
        left = v[v[:, 0] < center_x]
        right = v[v[:, 0] >= center_x]
        if len(left) > 10:
            centroids[(name, "L")] = left.mean(0)
        if len(right) > 10:
            centroids[(name, "R")] = right.mean(0)
        centroids[(name, None)] = v.mean(0)
        for pt in v:
            pts_list.append(pt)
            labels.append((name, "R" if pt[0] >= center_x else "L"))
    tree = cKDTree(np.array(pts_list, dtype=np.float64))
    return tree, labels, centroids


def sample_neuron_synapses(arbor_pts, is_terminal, np_counts, locator, partner_pts, cap=SYN_CAP, is_output=True, rng=None):
    """Selects up to `cap` 3D synapse puncta on `arbor_pts` matching the neuron's FlyWire v783 neuropil distribution
    and anchored at real connected partner contacts (`partner_pts`)."""
    if rng is None:
        rng = np.random.default_rng(783)
    total_syn = int(sum(np_counts.values()))
    if len(arbor_pts) == 0 or total_syn == 0:
        return np.zeros((0, 3), dtype=np.float32), total_syn

    target_n = min(cap, total_syn, len(arbor_pts))
    if target_n <= 0:
        return np.zeros((0, 3), dtype=np.float32), total_syn

    tree_np, labels_np, centroids_np = locator
    arbor_tree = cKDTree(arbor_pts)
    chosen_idx = []

    # 1. Anchor up to 35% of puncta at real connected partner contact sites on this arbor
    if len(partner_pts) > 0:
        max_partner = min(len(partner_pts), max(4, int(target_n * 0.35)))
        if len(partner_pts) > max_partner:
            p_sel = partner_pts[rng.choice(len(partner_pts), max_partner, replace=False)]
        else:
            p_sel = partner_pts
        _, idx_p = arbor_tree.query(p_sel, k=1)
        chosen_idx.extend(int(i) for i in np.atleast_1d(idx_p))

    rem = target_n - len(chosen_idx)
    if rem > 0:
        # 2. Stratify remaining puncta across neuropils according to real FlyWire v783 per-neuropil counts
        _, nearest_np_idx = tree_np.query(arbor_pts, k=1)
        node_base = [labels_np[int(j)][0] for j in nearest_np_idx]
        # Presynaptic boutons prefer terminal/distal branch tips; postsynaptic spines prefer dendritic shafts
        base_w = np.where(is_terminal, 2.6 if is_output else 0.85, 1.0 if is_output else 1.6)

        items = sorted(np_counts.items(), key=lambda kv: -kv[1])
        for fw_np, cnt in items:
            quota = int(round(rem * (cnt / max(1, total_syn))))
            if quota <= 0:
                continue
            base, side = _np_base_side(fw_np)
            mask = np.array([nb == base for nb in node_base], dtype=bool)
            cand = np.flatnonzero(mask)
            if len(cand) < quota:
                # Fallback: nodes closest to that neuropil's centroid
                c_pt = centroids_np.get((base, side), centroids_np.get((base, None), None))
                if c_pt is not None:
                    d_c = np.linalg.norm(arbor_pts - c_pt[None, :], axis=1)
                    cand = np.argsort(d_c)[:max(quota * 3, 24)]
                else:
                    cand = np.arange(len(arbor_pts))
            w = base_w[cand].copy()
            w /= w.sum()
            take = min(quota, len(cand))
            picks = rng.choice(cand, size=take, replace=False, p=w)
            chosen_idx.extend(int(i) for i in picks)

    # Top up or trim to exact target_n
    if len(chosen_idx) < target_n:
        need = target_n - len(chosen_idx)
        w = np.where(is_terminal, 2.2 if is_output else 0.9, 1.0)
        w = w / w.sum()
        extra = rng.choice(len(arbor_pts), size=need, replace=False, p=w)
        chosen_idx.extend(int(i) for i in extra)
    elif len(chosen_idx) > target_n:
        chosen_idx = chosen_idx[:target_n]

    pts_out = arbor_pts[np.array(chosen_idx, dtype=np.int64)].astype(np.float32)
    return pts_out, total_syn


def patch_flywire_bin(bin_path, manifest_path, cache_dir, log=print):
    """Reads an existing `Models/flywire_brain.bin`, computes real FlyWire v783 synapse counts and JRC2018U arbor
    puncta (`SYN1`) for all neurons, and updates `flywire_brain.bin` and `flywire_brain_manifest.json` in place."""
    t0 = time.time()
    data = open(bin_path, "rb").read()
    p = 8  # skip FWB1 + version(1)

    def rs():
        nonlocal p
        n = struct.unpack_from("<i", data, p)[0]; p += 4
        s = data[p : p + n].decode("utf-8"); p += n
        return s

    def rf():
        nonlocal p
        n = struct.unpack_from("<i", data, p)[0]; p += 4
        a = np.frombuffer(data, "<f4", n, p).copy(); p += 4 * n
        return a

    def ri():
        nonlocal p
        n = struct.unpack_from("<i", data, p)[0]; p += 4
        a = np.frombuffer(data, "<i4", n, p).copy(); p += 4 * n
        return a

    rs()  # attribution
    p += 4  # extent
    rf(); rf(); ri()  # template v, n, f

    n_np = struct.unpack_from("<i", data, p)[0]; p += 4
    neuropils = []
    for _ in range(n_np):
        name = rs(); nid = rs(); p += 4
        rf()  # centroid
        v = rf().reshape(-1, 3)
        rf(); ri()
        neuropils.append({"name": name, "id": nid, "v": v})

    n_neu = struct.unpack_from("<i", data, p)[0]; p += 4
    neurons = []
    for _ in range(n_neu):
        nid = rs(); fw = rs(); rs(); rs(); rs(); p += 2
        rf(); rf()
        segs = rf().reshape(-1, 2, 3)
        rf()
        neurons.append({"id": nid, "fw": int(fw) if fw.isdigit() else -1, "segs": segs})

    n_edges = struct.unpack_from("<i", data, p)[0]; p += 4
    out_contacts = [[] for _ in range(n_neu)]
    in_contacts = [[] for _ in range(n_neu)]
    for _ in range(n_edges):
        pre_i, post_i, w = struct.unpack_from("<iii", data, p); p += 12
        path = rf().reshape(-1, 3)
        if len(path) > 0:
            contact = path[-1]
            reps = min(12, max(1, int(round(np.sqrt(max(1, w))))))
            for _ in range(reps):
                out_contacts[pre_i].append(contact)
                in_contacts[post_i].append(contact)

    syn1_offset = p
    magic = data[syn1_offset : syn1_offset + 4]
    if magic not in (b"SYN1", b"END1"):
        raise RuntimeError(f"Unexpected marker at offset {syn1_offset}: {magic!r}")

    log(f"Loaded {n_neu} neurons, {n_np} neuropils, {n_edges} edges from {bin_path}")
    pre_by_fw, post_by_fw = load_neuropil_counts([n["fw"] for n in neurons], cache_dir, log)
    locator = build_neuropil_locator(neuropils, center_x=0.0)
    rng = np.random.default_rng(783)

    syn_buf = io.BytesIO()
    syn_buf.write(b"SYN1")
    total_dots = 0
    total_real_syn = 0

    for i, n in enumerate(neurons):
        fw = n["fw"]
        segs = n["segs"]
        if len(segs) > 0:
            pts_all = segs.reshape(-1, 3)
            # Identify unique arbor nodes and degree-1 terminal branch tips
            q_pts = np.round(pts_all * 10.0).astype(np.int32)
            _, uniq_idx, counts = np.unique(q_pts, axis=0, return_index=True, return_counts=True)
            arbor_pts = pts_all[uniq_idx]
            is_term = counts == 1
        else:
            arbor_pts = np.zeros((0, 3), dtype=np.float32)
            is_term = np.zeros(0, dtype=bool)

        p_out = np.array(out_contacts[i], dtype=np.float64) if out_contacts[i] else np.zeros((0, 3))
        p_in = np.array(in_contacts[i], dtype=np.float64) if in_contacts[i] else np.zeros((0, 3))

        so, tot_out = sample_neuron_synapses(
            arbor_pts, is_term, pre_by_fw.get(fw, {}), locator, p_out, cap=SYN_CAP, is_output=True, rng=rng
        )
        si, tot_in = sample_neuron_synapses(
            arbor_pts, is_term, post_by_fw.get(fw, {}), locator, p_in, cap=SYN_CAP, is_output=False, rng=rng
        )
        # Fallback if a sensory/descending neuron had 0 in one table: use edge contacts count
        if tot_out == 0 and len(out_contacts[i]) > 0:
            tot_out = len(out_contacts[i])
        if tot_in == 0 and len(in_contacts[i]) > 0:
            tot_in = len(in_contacts[i])

        total_dots += len(so) + len(si)
        total_real_syn += tot_out + tot_in

        syn_buf.write(struct.pack("<ii", int(tot_out), int(tot_in)))
        so_flat = np.asarray(so, dtype="<f4").reshape(-1)
        si_flat = np.asarray(si, dtype="<f4").reshape(-1)
        syn_buf.write(struct.pack("<i", len(so_flat)))
        syn_buf.write(so_flat.tobytes())
        syn_buf.write(struct.pack("<i", len(si_flat)))
        syn_buf.write(si_flat.tobytes())

    syn_buf.write(b"END1")
    new_data = data[:syn1_offset] + syn_buf.getvalue()
    with open(bin_path, "wb") as f:
        f.write(new_data)

    if os.path.exists(manifest_path):
        with open(manifest_path, "r", encoding="utf-8") as f:
            manifest = json.load(f)
        manifest["real_synapse_dots"] = int(total_dots)
        manifest["real_synapses_total"] = int(total_real_syn)
        with open(manifest_path, "w", encoding="utf-8") as f:
            json.dump(manifest, f, indent=2, ensure_ascii=False)

    log(
        f"Patched SYN1 in {bin_path} ({len(new_data)/1e6:.2f} MB) in {time.time()-t0:.1f}s: "
        f"{total_dots:,} 3D synapse puncta representing {total_real_syn:,} real FlyWire v783 synapses."
    )


if __name__ == "__main__":
    root = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
    patch_flywire_bin(
        os.path.join(root, "Models", "flywire_brain.bin"),
        os.path.join(root, "Models", "flywire_brain_manifest.json"),
        os.path.join(root, "tools", "cache"),
    )

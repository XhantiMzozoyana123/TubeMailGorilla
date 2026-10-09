"""End-to-end test of the EXACT img2img contract ComfyUiImageGenerationService uses:
upload image -> POST /prompt (same graph) -> poll /history/{id} -> GET /view.
Uses only stdlib + PIL (bundled with ComfyUI). Writes result to e2e_result.txt.
"""
import io, json, os, sys, time, urllib.request, urllib.error, uuid

BASE = "http://127.0.0.1:8188"
OUT = r"C:\Users\Shadow\Documents\TubeMailGorilla\e2e_result_18.txt"

def log(msg):
    with open(OUT, "a", encoding="utf-8") as f:
        f.write(str(msg) + "\n")

def url(path):
    return BASE + path

def get(path, timeout=30):
    with urllib.request.urlopen(url(path), timeout=timeout) as r:
        return r.read()

def post(path, data, timeout=60):
    req = urllib.request.Request(url(path), data=data, method="POST")
    with urllib.request.urlopen(req, timeout=timeout) as r:
        return r.read()

def main():
    if os.path.exists(OUT):
        os.remove(OUT)
    log("E2E TEST START")

    # --- make a source image (512x768 portrait, red-ish scene) ---
    from PIL import Image, ImageDraw
    img = Image.new("RGB", (512, 768), (120, 140, 170))
    d = ImageDraw.Draw(img)
    d.rectangle([100, 200, 410, 600], fill=(180, 60, 60))
    d.ellipse([180, 80, 330, 230], fill=(240, 200, 160))
    buf = io.BytesIO()
    img.save(buf, format="PNG")
    png = buf.getvalue()
    log("source image built bytes=%d" % len(png))

    # --- 1) multipart upload (mirrors MultipartFileUploadAsync) ---
    boundary = uuid.uuid4().hex
    fname = "e2e_src_%s.png" % uuid.uuid4().hex[:8]
    body = b"--" + boundary.encode() + b"\r\n"
    body += b'Content-Disposition: form-data; name="image"; filename="' + fname.encode() + b'"\r\n'
    body += b"Content-Type: image/png\r\n\r\n"
    body += png + b"\r\n"
    body += b"--" + boundary.encode() + b"\r\n"
    body += b'Content-Disposition: form-data; name="overwrite"\r\n\r\n'
    body += b"true\r\n"
    body += b"--" + boundary.encode() + b"--\r\n"

    req = urllib.request.Request(
        url("/upload/image"),
        data=body,
        method="POST",
        headers={"Content-Type": "multipart/form-data; boundary=" + boundary},
    )
    with urllib.request.urlopen(req, timeout=60) as r:
        up = json.loads(r.read().decode("utf-8"))
    uploaded = up.get("name") or fname
    log("upload ok name=%s subfolder=%s" % (up.get("name"), up.get("subfolder")))

    # --- 2) build the SAME graph the C# service builds ---
    ckpt = "v1-5-pruned-emaonly.safetensors"
    prompt = {
        "1": {"class_type": "LoadImage",
              "inputs": {"image": uploaded, "upload": "image"}},
        "2": {"class_type": "CheckpointLoaderSimple", "inputs": {"ckpt_name": ckpt}},
        "3": {"class_type": "VAEEncode",
              "inputs": {"pixels": ["1", 0], "vae": ["2", 2]}},
        "4": {"class_type": "LoraZero",
              "inputs": {"model": ["2", 0], "clip": ["2", 1],
                         "strength_model": 0, "strength_clip": 0}},
        "5": {"class_type": "KSampler",
              "inputs": {"seed": 42, "steps": 4, "cfg": 7.0,
                         "sampler_name": "euler", "scheduler": "normal",
                         "denoise": 0.55, "model": ["4", 0],
                         "positive": ["4", 1], "negative": ["4", 1],
                         "latent_image": ["3", 0]}},
        "6": {"class_type": "VAEDecode",
              "inputs": {"samples": ["5", 0], "vae": ["2", 2]}},
        "7": {"class_type": "SaveImage",
              "inputs": {"filename_prefix": "e2e_test", "images": ["6", 0]}},
    }
    payload = json.dumps({"prompt": prompt, "client_id": uuid.uuid4().hex}).encode("utf-8")

    # --- 3) queue ---
    try:
        resp = post("/prompt", payload, timeout=60)
        q = json.loads(resp.decode("utf-8"))
    except urllib.error.HTTPError as e:
        detail = e.read().decode("utf-8", "replace")[:1500]
        log("QUEUE FAILED http=%s" % e.code)
        log(detail)
        return
    pid = q.get("prompt_id")
    log("queued prompt_id=%s" % pid)
    if not pid:
        log("NO PROMPT_ID: %s" % json.dumps(q)[:1500])
        return

    # --- 4) poll history (mirrors WaitForOutputAsync) ---
    deadline = time.time() + 300
    image = None
    while time.time() < deadline:
        try:
            raw = get("/history/%s" % pid, timeout=30)
            hist = json.loads(raw.decode("utf-8"))
        except Exception as ex:
            log("history err: %s" % ex)
            time.sleep(1.0)
            continue
        image = None
        for entry in hist.values():
            for node in (entry.get("outputs") or {}).values():
                for im in (node.get("images") or []):
                    if im.get("filename"):
                        image = im
                        break
                if image:
                    break
            if image:
                break
        if image:
            break
        time.sleep(0.75)

    if not image:
        log("TIMEOUT waiting for output image")
        return
    log("output image: %s" % json.dumps(image))

    # --- 5) fetch the produced image (mirrors ComfyUI_Image_URL) ---
    from urllib.parse import urlencode
    qs = urlencode({"filename": image["filename"],
                    "subfolder": image.get("subfolder", ""),
                    "type": image.get("type", "")})
    data = get("/view?" + qs, timeout=60)
    log("fetched image bytes=%d" % len(data))
    log("RESULT: " + ("PASS" if len(data) > 1000 else "FAIL (too small)"))
    log("E2E TEST END")

if __name__ == "__main__":
    try:
        main()
    except Exception:
        import traceback
        log("UNCAUGHT:\n" + traceback.format_exc())
        log("E2E TEST END")

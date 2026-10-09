"""E2E test mirroring ComfyUiImageGenerationService EXACTLY as it exists on disk:
  upload: POST /upload/image, multipart field 'image', filename 'snapshot.jpg',
          content-type image/jpeg, NO overwrite field -> use response .name
  queue:  POST /prompt with {"prompt": graph} (no client_id)
  graph:  1 CheckpointLoader -> 2/3 CLIPTextEncode -> 4 LoadImage -> 9 ImageScale
          -> 5 VAEEncode -> 6 KSampler -> 7 VAEDecode -> 8 SaveImage
  wait:   poll /history/{prompt_id} -> GET /view
Writes progress to e2e2_result_23.txt.
"""
import io, json, os, random, time, uuid
import urllib.request, urllib.error, urllib.parse

BASE = "http://127.0.0.1:8188"
OUT = r"C:\Users\Shadow\Documents\TubeMailGorilla\e2e2_result_23.txt"
CKPT = "v1-5-pruned-emaonly.safetensors"
EDIT_PROMPT = "warm golden hour color grade, soft rim light"
WIDTH, HEIGHT = 600, 400


def log(msg):
    with open(OUT, "a", encoding="utf-8") as f:
        f.write(str(msg) + "\n")


def http(url, data=None, headers=None, timeout=60):
    req = urllib.request.Request(url, data=data, headers=headers or {})
    with urllib.request.urlopen(req, timeout=timeout) as r:
        return r.status, r.read()


def main():
    if os.path.exists(OUT):
        os.remove(OUT)
    log("E2E2 TEST START")

    from PIL import Image, ImageDraw
    img = Image.new("RGB", (WIDTH, HEIGHT), (90, 120, 160))
    d = ImageDraw.Draw(img)
    d.rectangle([80, 120, 520, 360], fill=(170, 70, 60))
    d.ellipse([240, 30, 360, 150], fill=(235, 205, 170))
    buf = io.BytesIO()
    img.save(buf, format="JPEG", quality=92)
    jpg = buf.getvalue()
    log("source jpeg bytes=%d" % len(jpg))

    boundary = uuid.uuid4().hex
    body = b"--" + boundary.encode() + b"\r\n"
    body += b'Content-Disposition: form-data; name="image"; filename="snapshot.jpg"\r\n'
    body += b"Content-Type: image/jpeg\r\n\r\n"
    body += jpg + b"\r\n"
    body += b"--" + boundary.encode() + b"--\r\n"
    try:
        st, resp = http(BASE + "/upload/image", data=body,
                        headers={"Content-Type": "multipart/form-data; boundary=" + boundary})
    except urllib.error.HTTPError as e:
        log("UPLOAD FAILED http=%s %s" % (e.code, e.read().decode("utf-8", "replace")[:800]))
        return
    up = json.loads(resp.decode("utf-8"))
    image_name = up.get("name")
    log("upload http=%s name=%s type=%s" % (st, image_name, up.get("type")))
    if not image_name:
        log("no name in upload response: %s" % resp[:400])
        return
    run_queue(image_name)


def run_queue(image_name):
    w = (WIDTH // 8) * 8
    h = (HEIGHT // 8) * 8
    seed = random.randint(1, 2**31 - 1)
    graph = {
        "1": {"class_type": "CheckpointLoaderSimple",
              "inputs": {"ckpt_name": CKPT}},
        "2": {"class_type": "CLIPTextEncode",
              "inputs": {"text": "cinematic still, " + EDIT_PROMPT, "clip": ["1", 1]}},
        "3": {"class_type": "CLIPTextEncode",
              "inputs": {"text": "blurry, watermark, text, logo, deformed, low quality",
                         "clip": ["1", 1]}},
        "4": {"class_type": "LoadImage", "inputs": {"image": image_name}},
        "9": {"class_type": "ImageScale",
              "inputs": {"image": ["4", 0], "width": w, "height": h,
                         "upscale_method": "bilinear", "crop": "disabled"}},
        "5": {"class_type": "VAEEncode",
              "inputs": {"pixels": ["9", 0], "vae": ["1", 2]}},
        "6": {"class_type": "KSampler",
              "inputs": {"model": ["1", 0], "positive": ["2", 0],
                         "negative": ["3", 0], "latent_image": ["5", 0],
                         "seed": seed, "steps": 25, "cfg": 8.0,
                         "sampler_name": "euler_ancestral", "scheduler": "normal",
                         "denoise": 0.55}},
        "7": {"class_type": "VAEDecode",
              "inputs": {"samples": ["6", 0], "vae": ["1", 2]}},
        "8": {"class_type": "SaveImage",
              "inputs": {"images": ["7", 0], "filename_prefix": "tmg_snapshot_ai"}},
    }
    payload = json.dumps({"prompt": graph}).encode("utf-8")
    try:
        st, resp = http(BASE + "/prompt", data=payload,
                        headers={"Content-Type": "application/json; charset=utf-8"})
    except urllib.error.HTTPError as e:
        log("QUEUE FAILED http=%s" % e.code)
        log(e.read().decode("utf-8", "replace")[:1500])
        return
    q = json.loads(resp.decode("utf-8"))
    pid = q.get("prompt_id")
    log("queue http=%s prompt_id=%s" % (st, pid))
    if not pid:
        log("NO PROMPT_ID: %s" % resp[:800])
        return
    wait_and_fetch(pid)


def wait_and_fetch(pid):
    deadline = time.time() + 300
    image = None
    while time.time() < deadline:
        try:
            _, raw = http(BASE + "/history/" + pid, timeout=30)
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
        log("TIMEOUT waiting for output image (300s)")
        return
    log("output: %s" % json.dumps(image))

    qs = urllib.parse.urlencode({"filename": image["filename"],
                                 "subfolder": image.get("subfolder", ""),
                                 "type": image.get("type", "")})
    _, data = http(BASE + "/view?" + qs, timeout=60)
    log("view bytes=%d" % len(data))
    from PIL import Image as Im
    im2 = Im.open(io.BytesIO(data))
    log("decoded size=%s mode=%s" % (im2.size, im2.mode))
    ok = len(data) > 1000 and im2.size[0] > 0
    log("RESULT: " + ("PASS" if ok else "FAIL"))
    log("E2E2 TEST END")


if __name__ == "__main__":
    try:
        main()
    except Exception:
        import traceback
        log("UNCAUGHT:\n" + traceback.format_exc())
        log("E2E2 TEST END")


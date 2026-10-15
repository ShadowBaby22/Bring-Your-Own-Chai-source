"""Tiny local receiver for screenshots/video recorded from the WebGL canvas: POST /save?name=file.png saves to Builds/media/."""
import os, http.server, urllib.parse
OUT = os.path.join(os.path.dirname(__file__), "..", "Builds", "media")
class H(http.server.BaseHTTPRequestHandler):
    def cors(self):
        self.send_header("Access-Control-Allow-Origin", "*")
        self.send_header("Access-Control-Allow-Methods", "POST, GET, OPTIONS")
        self.send_header("Access-Control-Allow-Headers", "*")
    def do_OPTIONS(self):
        self.send_response(204); self.cors(); self.end_headers()
    def do_GET(self):
        self.send_response(200); self.cors(); self.end_headers(); self.wfile.write(b"capture receiver ok")
    def do_POST(self):
        name = os.path.basename(urllib.parse.parse_qs(urllib.parse.urlparse(self.path).query)["name"][0])
        data = self.rfile.read(int(self.headers["Content-Length"]))
        os.makedirs(OUT, exist_ok=True)
        open(os.path.join(OUT, name), "wb").write(data)
        self.send_response(200); self.cors(); self.end_headers(); self.wfile.write(f"saved {name} {len(data)}".encode())
http.server.ThreadingHTTPServer(("127.0.0.1", 8766), H).serve_forever()

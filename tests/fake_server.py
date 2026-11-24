import json, sys
from http.server import BaseHTTPRequestHandler, HTTPServer
mode = sys.argv[2]
class H(BaseHTTPRequestHandler):
    def log_message(self, *a): pass
    def send(self, o, code=200):
        b = json.dumps(o).encode()
        self.send_response(code); self.send_header("Content-Type", "application/json")
        self.send_header("Content-Length", str(len(b))); self.end_headers(); self.wfile.write(b)
    def do_GET(self):
        self.send({"object": "list", "data": [] if mode == "empty" else [{"id": "fake", "object": "model"}]})
    def do_POST(self):
        req = json.loads(self.rfile.read(int(self.headers["Content-Length"])))
        if mode == "error": return self.send({"error": {"message": "model failed"}}, 400)
        tool = [m for m in req["messages"] if m["role"] == "tool"]
        if mode == "tools" and not tool:
            msg = {"role": "assistant", "content": None, "tool_calls": [{"id": "c1", "type": "function",
                   "function": {"name": req["tools"][0]["function"]["name"], "arguments": "{}"}}]}
            fin = "tool_calls"
        else:
            msg = {"role": "assistant", "content": str(tool[-1]["content"]) if tool else "LOCAL_READY, Windows, .NET 11"}
            fin = "stop"
        self.send({"id": "x", "object": "chat.completion", "created": 1, "model": "fake",
                   "choices": [{"index": 0, "message": msg, "finish_reason": fin}],
                   "usage": {"prompt_tokens": 1, "completion_tokens": 1, "total_tokens": 2}})
HTTPServer(("127.0.0.1", int(sys.argv[1])), H).serve_forever()

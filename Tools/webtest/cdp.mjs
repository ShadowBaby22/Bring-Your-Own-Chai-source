// Headless Chrome driver for the WebGL build. No dependencies (Node 22+ has WebSocket).
// node cdp.mjs <url> <steps.txt> <outdir>
// steps: "wait <sec>" | "click <x> <y>" (1080x1920 game coords) | "shot <name>" | "eval <js>"
import { spawn } from 'node:child_process';
import { readFileSync, writeFileSync, mkdirSync } from 'node:fs';

const [url, stepsFile, outDir] = process.argv.slice(2);
mkdirSync(outDir, { recursive: true });
const W = 540, H = 960, K = W / 1080;
const port = 9300 + Math.floor(Math.random() * 500);
const chrome = spawn('/Applications/Google Chrome.app/Contents/MacOS/Google Chrome', [
  '--headless=new', `--remote-debugging-port=${port}`, `--window-size=${W},${H}`, '--use-angle=swiftshader',
  '--enable-unsafe-swiftshader', '--autoplay-policy=no-user-gesture-required', '--no-first-run', '--no-default-browser-check',
  `--user-data-dir=/tmp/kc_cdp_${port}`, 'about:blank'], { stdio: 'ignore' });
const sleep = ms => new Promise(r => setTimeout(r, ms));
let ws, id = 0; const pending = new Map(); const logs = [];
async function connect() {
  for (let i = 0; i < 50; i++) {
    try {
      const list = await (await fetch(`http://127.0.0.1:${port}/json/list`)).json();
      const page = list.find(t => t.type === 'page');
      if (page) { ws = new WebSocket(page.webSocketDebuggerUrl); break; }
    } catch { }
    await sleep(200);
  }
  await new Promise(r => ws.onopen = r);
  ws.onmessage = e => {
    const m = JSON.parse(e.data);
    if (m.id && pending.has(m.id)) { pending.get(m.id)(m); pending.delete(m.id); }
    if (m.method === 'Runtime.consoleAPICalled') logs.push(m.params.args.map(a => a.value ?? a.description).join(' '));
    if (m.method === 'Runtime.exceptionThrown') logs.push('EXCEPTION ' + JSON.stringify(m.params.exceptionDetails.exception?.description ?? m.params.exceptionDetails.text));
  };
}
const send = (method, params = {}) => new Promise(r => { const i = ++id; pending.set(i, r); ws.send(JSON.stringify({ id: i, method, params })); });

await connect();
await send('Runtime.enable');
await send('Page.enable');
await send('Emulation.setDeviceMetricsOverride', { width: W, height: H, deviceScaleFactor: 1, mobile: false });
await send('Page.navigate', { url });
const t0 = Date.now();
for (const raw of readFileSync(stepsFile, 'utf8').split('\n')) {
  const [cmd, ...a] = raw.trim().split(' ');
  if (!cmd || cmd.startsWith('#')) continue;
  if (cmd === 'wait') await sleep(parseFloat(a[0]) * 1000);
  if (cmd === 'click') {
    const x = parseFloat(a[0]) * K, y = parseFloat(a[1]) * K;
    for (const type of ['mouseMoved', 'mousePressed', 'mouseReleased'])
      await send('Input.dispatchMouseEvent', { type, x, y, button: 'left', clickCount: 1 });
    logs.push(`[${((Date.now() - t0) / 1000).toFixed(1)}s] click ${a[0]},${a[1]}`);
  }
  if (cmd === 'shot') {
    const r = await send('Page.captureScreenshot', { format: 'png' });
    writeFileSync(`${outDir}/${a[0]}.png`, Buffer.from(r.result.data, 'base64'));
    logs.push(`[${((Date.now() - t0) / 1000).toFixed(1)}s] shot ${a[0]}`);
  }
  if (cmd === 'eval') {
    const r = await send('Runtime.evaluate', { expression: a.join(' '), returnByValue: true });
    logs.push('eval: ' + JSON.stringify(r.result?.result?.value));
  }
}
writeFileSync(`${outDir}/log.txt`, logs.join('\n'));
ws.close(); chrome.kill();
process.exit(0);

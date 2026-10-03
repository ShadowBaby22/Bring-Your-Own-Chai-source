#!/bin/bash
# render.sh <svgdir> <pngdir> <w> <h>   (renders taller, then crops: headless viewport loses ~90px)
mkdir -p "$2"
for f in "$1"/*.svg; do
  n=$(basename "$f" .svg)
  "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome" --headless --disable-gpu --hide-scrollbars \
    --default-background-color=00000000 --window-size=$3,$(( $4 + 200 )) --screenshot="$2/$n.png" "file://$f" >/dev/null 2>&1
  python3 -c "from PIL import Image; im=Image.open('$2/$n.png'); im.crop((0,0,$3,$4)).save('$2/$n.png')"
done

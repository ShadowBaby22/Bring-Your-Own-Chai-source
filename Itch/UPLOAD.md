# Share a playable link (itch.io secret draft)

File to upload: `Builds/GRAP-Stage-4-web-<day-time>.zip` (the newest one).

## First time (~3 minutes)
1. Go to https://itch.io, log in, then open **Dashboard** and click **Create new project**.
2. **Title:** `KADAK Chai`. The project URL fills itself in.
3. **Kind of project:** **HTML**.
4. **Uploads:** click **Upload files** and choose the zip. Tick **"This file will be played in the browser"**.
5. **Embed options:**
   - Viewport dimensions: **540 × 960**
   - Tick **Mobile friendly** and set Orientation to **Portrait**
   - Tick **Fullscreen button**
   - Leave **Automatically start on page load** unticked, so the 18 MB only downloads when someone taps Run
   - Leave **SharedArrayBuffer support** unticked (not needed)
6. **Visibility & access:** **Draft**, then **Save**.
7. Open the project page and copy its **secret URL**. Itch shows it for drafts as "share this page with a secret URL". Anyone with that link can play on phone or desktop without an itch account.
   - If you don't see a secret URL, set visibility to **Restricted** and add a password instead.

## Every new build
Ask Claude for "a new web build", then on the project's **Edit** page delete the old zip, upload the new one, and tick "played in the browser" again. **The link stays the same**, so teammates just refresh.

## For playtesters (paste this with the link)
> KADAK Chai: jam build. Phone or desktop. Turn sound on and hold the phone upright.
> Tap **Run game**, then **Sign the Agreement**. A run is 11 in-game days, about 15 minutes.
> Please note: anything that made you laugh, anything slow or confusing, and the "dev build" text at the bottom of the title screen.

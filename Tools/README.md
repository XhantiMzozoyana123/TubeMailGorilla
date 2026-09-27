# Tools

Development-only helpers. The binaries in here are **not** committed (see
`.gitignore`) — download them yourself.

## yt-dlp

Required for YouTube search, transcripts and video metadata. The app shells
out to this binary; without it the Extract page shows a red banner and
extraction returns nothing.

**Docker / VPS:** nothing to do. The image installs yt-dlp and ffmpeg itself
(see `TubeMailGorilla.Mvc/Dockerfile`), and the build fails if the download
does not work — so a broken binary never ships.

**Local development (`dotnet run` from Visual Studio):** download it into
this folder:

```powershell
# Windows
Invoke-WebRequest https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp.exe -OutFile Tools\yt-dlp.exe

# macOS / Linux
curl -L https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp -o Tools/yt-dlp
chmod +x Tools/yt-dlp
```

Restart the app afterwards — the binary is located once at startup.

### Where the app looks, in order

1. `YtDlp:Path` in `appsettings.json` (absolute path, or a bare name to look
   up on `PATH`)
2. `TubeMailGorilla.Mvc/Tools/yt-dlp` — the exact path is printed in the
   Extract page's error banner
3. `Tools/yt-dlp` — this folder, at the repo root
4. `yt-dlp` on the system `PATH`

Installing it system-wide (e.g. `winget install yt-dlp.yt-dlp`,
`brew install yt-dlp`) satisfies step 4 and needs nothing here.

## ffmpeg

Used to capture a still frame every 10 seconds of each lead's video
(`VideoSnapshotService`). Optional: extraction still works without it, the
leads just arrive with no images.

```powershell
winget install Gyan.FFmpeg
```

Installed in the Docker image alongside yt-dlp.

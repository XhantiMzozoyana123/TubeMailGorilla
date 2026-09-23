# TubeMailGorilla - Unlocked edition

This is a **separate copy of the MAUI desktop app** (`TubeMailGorilla.Maui.Unlocked`)
that keeps every feature but removes the account/paywall machinery:

| | Subscription edition (`TubeMailGorilla.Maui`) | **Unlocked edition (this project)** |
|---|---|---|
| Sign-in | Required (JWT from the API) | **None** - opens straight into the app |
| Subscription / PayPal | Yes (Pro plan, upgrade, cancel) | **None** - no checkout code at all |
| Server validation | Every action gated by `POST /api/validation/check` | **None** - `ValidationService` approves locally, unlimited |
| Limits | Free-plan caps (1 extraction/month, capped contacts/campaigns) | **Unlimited** everything |
| Feature gates | Templates, blocklist and icebreakers are Pro-only | **All unlocked** - no gate UI exists |
| Data | `%LOCALAPPDATA%\TubeMailGorilla` | `%LOCALAPPDATA%\TubeMailGorillaUnlocked` |
| App identity | `com.tubemailgorilla.maui`, installs to `TubeMailGorilla` | `com.tubemailgorilla.maui.unlocked`, installs to `TubeMailGorilla Unlocked` |

Both editions can be installed side by side and neither can see the other's
database. The AI model folder is shared, so the ~1.9 GB model needs to be
downloaded only once per machine.

## How the "no locks" part works

- `Services/ValidationService.cs` - the only gatekeeper the pages ask. In this
  copy it returns `Approved = true` with `Limit = -1` (unlimited) locally, so
  every existing call site keeps working and nothing can ever be denied.
- `Services/PaymentService.cs` - returns a hard-coded "Unlocked / unlimited"
  entitlement set (`GetEntitlementsAsync`, `IsSubscribedAsync`,
  `IsBlocklistAllowedAsync`), so the pages that read limits see no caps. No
  HTTP calls at all.
- `Services/Subscriptions.cs` - a single `Unlocked` package: nothing to buy
  and nothing to cancel.
- `App.xaml.cs` - always starts `AppShell`; there is no token check and no
  `AuthView` left in the project.
- Removed files: `Views/AuthView.*`, `Views/PayPalApprovalPage.*`,
  `ViewModels/AuthViewModel.cs`, `Services/AuthService.cs`, plus the Pro-gate
  UI inside `BlockedPage`, `EmailTemplatesPage` and `ContactsPage`.
- Data isolation: `Services/DatabaseService.cs` uses its own data sub-folder.

Nothing else changed, so extraction, icebreakers, templates, contacts, sends,
blocking and settings behave exactly as in the subscription edition - just
without a login or a limit.

## Build & run

```powershell
# Debug build (Windows)
dotnet build TubeMailGorilla.Maui.Unlocked.csproj -c Debug -f net10.0-windows10.0.19041.0

# Run it
.\bin\Debug\net10.0-windows10.0.19041.0\win-x64\TubeMailGorilla.Maui.Unlocked.exe
```

## Build the installer

```powershell
powershell -ExecutionPolicy Bypass -File Installer\build-installer.ps1
```

Outputs:

- `Installer\TubeMailGorillaUnlocked-setup.exe` - the installer
- `Installer\TubeMailGorilla-Unlocked-v1.1.0-Windows.zip` - installer + install guide

Add `-CopyToWebsite` to also drop the bundle into
`TubeMailGorilla.Web\public\downloads` (off by default - the website hands out
the subscription edition).

The setup wizard downloads the on-device AI model (~1.9 GB) during
installation, exactly like the subscription edition's installer, and the app
also self-heals a missing model (or reuses a shared copy in
`%LOCALAPPDATA%\TubeMailGorilla\Models`) on first use.

---

The rest of this file describes the shared MAUI app (identical UI in both
editions).


# TubeMailGorilla.Maui

A standalone native .NET MAUI desktop application for YouTube lead extraction and email outreach, compatible with **Windows** and **macOS**.

## Architecture

This is a native .NET MAUI app (XAML UI) that replaces the Electron + Angular + Express architecture with:

- **UI**: .NET MAUI native cross-platform UI (XAML)
- **Database**: SQLite via `sqlite-net-pcl`
- **Platform**: Windows 10+ and macOS 13+ (Mac Catalyst)
- **Packaging**: Self-contained, standalone executables

## Project Structure

```
TubeMailGorilla.Maui/
├── Models/
│   ├── AppSettings.cs
│   ├── Blocker.cs
│   ├── EmailContact.cs
│   ├── Emailer.cs
│   ├── EmailTemplate.cs
│   ├── Inboxer.cs
│   ├── MessageParameter.cs
│   ├── Opener.cs
│   ├── Sender.cs
│   └── SubscriptionPackage.cs
├── Services/
│   ├── AIService.cs
│   ├── CaptionService.cs
│   ├── DatabaseService.cs
│   ├── EmailService.cs
│   ├── ExtractService.cs
│   ├── LLMService.cs
│   ├── SendSettings.cs
│   ├── ServiceHelper.cs
│   ├── Subscriptions.cs
│   ├── YouTubeSearchService.cs
│   └── YouTubeTranscriptService.cs
├── Views/
│   ├── BlockedPage.xaml(.cs)
│   ├── ExtractPage.xaml(.cs)
│   ├── ContactsPage.xaml(.cs)
│   ├── SendEmailsPage.xaml(.cs)
│   └── SettingsPage.xaml(.cs)
├── Platforms/
│   ├── MacCatalyst/
│   │   ├── AppDelegate.cs
│   │   ├── Entitlements.plist
│   │   ├── Info.plist
│   │   └── Program.cs
│   └── Windows/
│       ├── App.xaml
│       ├── App.xaml.cs
│       ├── Package.appxmanifest
│       └── app.manifest
├── Resources/
│   ├── AppIcon/
│   ├── Fonts/
│   ├── Images/
│   ├── Raw/
│   └── Splash/
├── App.xaml
├── App.xaml.cs
├── AppShell.xaml
├── AppShell.xaml.cs
├── MauiProgram.cs
├── TubeMailGorilla.Maui.csproj
└── TubeMailGorilla.Maui.sln
```

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [.NET MAUI workload](https://learn.microsoft.com/dotnet/maui/get-started/installation)
- Visual Studio 2022 with **.NET Multi-platform App UI development** workload

## Build & Run

### Windows

```bash
cd TubeMailGorilla.Maui
dotnet build -f net10.0-windows10.0.19041.0
dotnet run -f net10.0-windows10.0.19041.0
```

### macOS

```bash
cd TubeMailGorilla.Maui
dotnet build -f net10.0-maccatalyst
dotnet run -f net10.0-maccatalyst
```

## Publish (Self-Contained)

### Windows

```bash
dotnet publish -f net10.0-windows10.0.19041.0 -r win-x64 --self-contained true -c Release
```

Output: `bin/Release/net10.0-windows10.0.19041.0/win-x64/publish/`

### macOS (Apple Silicon)

```bash
dotnet publish -f net10.0-maccatalyst -r osx-arm64 --self-contained true -c Release
```

Output: `bin/Release/net10.0-maccatalyst/osx-arm64/publish/`

### macOS (Intel)

```bash
dotnet publish -f net10.0-maccatalyst -r osx-x64 --self-contained true -c Release
```

## Features

- **Contacts**: View all extracted email contacts
- **Extract**: Extract emails from YouTube videos
- **Send Emails**: Send bulk emails to all extracted leads
- **Blockers**: View and manage blocked emails
- **Settings**: Configure API keys, SMTP settings, and preferences
- **Local SQLite database**: All data stored locally
- **Standalone**: No external runtime dependencies

## Local LLM (LLamaSharp)

The app runs AI data extraction (names, companies, job titles, locations, industries, and
email icebreakers) **fully on-device** via [LLamaSharp](https://github.com/SciSharp/LLamaSharp)
(LLaMA.cpp for .NET) — no remote API, no API keys, and no data ever leaves the machine.

- **Model**: Llama 3.2 3B Instruct in GGUF format (`Q4_K_M`, ~1.9 GB) configured in
  `appsettings.json` under `LlmSettings`.
- **Bundled with the app**: the GGUF is shipped inside the published package
  (`Resources\Models\Llama-3.2-3B-Instruct-Q4_K_M.gguf` in this project, copied next to the
  executable at build/publish time). Users get a fully offline app — **no runtime download
  and no internet required**.
- **Fallback for dev**: if the model file is absent from `Resources\Models` (e.g. a fresh
  clone before the file is added), the app automatically downloads it on first use and caches
  it under the local application data folder:
  - Windows: `%LOCALAPPDATA%\TubeMailGorilla\Models`
  - macOS: `~/Library/Application Support/TubeMailGorilla/Models`
- **Obtaining the bundled file**: the model is large and not fetched automatically during a
  build. Run the helper script to place it at `Resources\Models` (then publish to bundle it):
  `powershell -ExecutionPolicy Bypass -File ..\Tools\download-model.ps1`
- **CPU inference by default**: `GpuLayerCount` defaults to `0`; on a CUDA/Metal capable
  machine raise it in `LlmSettings` for faster inference.
- **Override the model**: point `LlmSettings.ModelUrl` / `LlmSettings.ModelFileName` at any
  compatible Llama 3 GGUF (e.g. a larger or smaller quant) in `appsettings.json`, place the
  file in `Resources\Models`, and delete any cached copy to force a refresh.

The relevant NuGet packages are `LLamaSharp` and `LLamaSharp.Backend.Cpu` (the CPU native
backend), both at `0.27.0`.

## Smoke-testing the packaged model

A standalone console harness (`Tools\LlmSmokeTest`) loads the **same bundled `.gguf`** with
the **same LLamaSharp 0.27.0 settings** the MAUI app uses (`StatelessExecutor`, strict
extraction system message, temperature, `MaxTokens`, Llama 3 end-of-turn anti-prompt) and
runs a few real extraction prompts.

```bash
# from the repo root (auto-finds the bundled model)
dotnet run --project Tools\LlmSmokeTest -c Release

# or point it at any model explicitly
dotnet run --project Tools\LlmSmokeTest -c Release -- --model <path-to-.gguf>
```

Success looks like: model loads (~30-90s first time), then short, correct extractions such as
`Sarah Mitchell` and `CraftCo` for the sample prompts, and a final `SMOKE TEST COMPLETE`.
A non-zero exit code or a thrown `LlamaException`/DllNotFoundException means the model file
or native backend is broken.

## Migration Notes

This MAUI app is a ground-up rewrite of the original Electron + Angular + Express app. Key changes:

| Original | MAUI |
|----------|------|
| Electron shell | Native MAUI window (AppShell) |
| Angular + Ionic | .NET MAUI XAML pages |
| Express.js API | In-process services |
| TypeScript | C# |
| node_modules | NuGet packages |
| SQLite (TypeORM) | SQLite-net-pcl |

## Open in Visual Studio

1. Open `TubeMailGorilla.Maui.sln`
2. Select target platform (Windows or macOS)
3. Press **F5** to debug

## License

Same as parent project.
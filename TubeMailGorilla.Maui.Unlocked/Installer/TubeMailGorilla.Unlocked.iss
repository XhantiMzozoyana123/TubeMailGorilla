; =====================================================================
; TubeMailGorilla Unlocked - Simple Windows Installer (Inno Setup)
;
; Build it with:  Installer\build-installer.ps1
;
; This is the UNLOCKED edition: no account, no sign-in, no subscription and
; no server checks - every feature is available from the first launch.
; The installer also needs no account: it installs the app and downloads the
; on-device AI model (Qwen2.5-VL 3B GGUF ~1.9 GB + its vision projector
; ~845 MB) into the app's models folder, so end users never have to do
; anything technical.
;
; Installs side by side with the subscription edition (different AppId and
; install folder).
; =====================================================================

#define MyAppName "TubeMailGorilla Unlocked"
; Allow the version to be supplied by build-installer.ps1 (-Version ...)
#ifndef MyAppVersion
  #define MyAppVersion "1.1.0"
#endif
#define MyAppPublisher "TubeMailGorilla"
#define MyAppExeName "TubeMailGorilla.Maui.Unlocked.exe"

; Both halves of the AI model, and they MUST come from the same repo: llama.cpp
; feeds projector output into the chat model's embedding table, so a projector
; trained against a different base architecture can never load. These names and
; URLs must stay in sync with LlmSettings.ModelFileName / VisionModelFileName.
#define ChatModelFileName "Qwen2.5-VL-3B-Instruct-Q4_K_M.gguf"
#define ChatModelUrl "https://huggingface.co/ggml-org/Qwen2.5-VL-3B-Instruct-GGUF/resolve/main/Qwen2.5-VL-3B-Instruct-Q4_K_M.gguf"
#define VisionModelFileName "mmproj-Qwen2.5-VL-3B-Instruct-Q8_0.gguf"
#define VisionModelUrl "https://huggingface.co/ggml-org/Qwen2.5-VL-3B-Instruct-GGUF/resolve/main/mmproj-Qwen2.5-VL-3B-Instruct-Q8_0.gguf"

; Path to the dotnet publish output (created by build-installer.ps1)
#define PublishDir "..\bin\Release\net10.0-windows10.0.19041.0\win-x64\publish"

[Setup]
AppId={{C7D1E4A2-3B58-4F90-8E61-2D5B9F7C3A64}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
VersionInfoVersion={#MyAppVersion}
VersionInfoProductName={#MyAppName}
VersionInfoProductVersion={#MyAppVersion}
VersionInfoCompany={#MyAppPublisher}
VersionInfoDescription={#MyAppName} Setup
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
PrivilegesRequired=admin
OutputBaseFilename=TubeMailGorillaUnlocked-setup
OutputDir=.
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
ArchitecturesInstallIn64BitMode=x64compatible
UninstallDisplayIcon={app}\appicon.ico
SetupIconFile={#PublishDir}\appicon.ico
MinVersion=10.0

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; The entire self-contained publish output, EXCLUDING the .gguf model -
; the wizard downloads the model during installation instead (see [Code]).
Source: "{#PublishDir}\*"; Excludes: "*.gguf"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\Uninstall {#MyAppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#MyAppName}}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
; Remove the downloaded AI model when uninstalling. It lives in the per-user
; models folder (not {app}), so it is listed by its full path.
Type: files; Name: "{localappdata}\TubeMailGorillaUnlocked\models\{#ChatModelFileName}"
Type: files; Name: "{localappdata}\TubeMailGorillaUnlocked\models\{#VisionModelFileName}"

[Code]
const
  ChatModelUrl = '{#ChatModelUrl}';
  ChatModelFileName = '{#ChatModelFileName}';
  VisionModelUrl = '{#VisionModelUrl}';
  VisionModelFileName = '{#VisionModelFileName}';

  // Must match LLMService.ModelDirectory, which is where the app looks by
  // default. Writing the model anywhere else is exactly what made every
  // installer build report "No model found" on first extraction.
  ModelsDir = '{localappdata}\TubeMailGorillaUnlocked\models';

  // Real sizes are ~1.9 GB and ~845 MB; these floors only reject truncated or
  // HTML-error downloads saved under the .gguf name.
  ChatModelMinBytes = 1073741824;    // 1 GB
  VisionModelMinBytes = 268435456;   // 256 MB

var
  ModelPage: TDownloadWizardPage;
  DownloadModel: Boolean;

// Size in bytes of an existing file, or 0 when it does not exist.
function FileSizeOf(const Path: String): Int64;
var
  F: TFindRec;
begin
  Result := 0;
  if FileExists(Path) and FindFirst(Path, F) then
  try
    Result := Int64(F.SizeHigh) * 4294967296 + Int64(F.SizeLow);
  finally
    FindClose(F);
  end;
end;

// True when BOTH halves are already present and plausibly sized (upgrade case).
// Only the chat model is required for extraction; a missing projector just means
// no image features, so it must not force a pointless ~845 MB re-download.
function ChatModelAlreadyInstalled(): Boolean;
begin
  Result := FileSizeOf(ModelsDir + '\' + ChatModelFileName) > ChatModelMinBytes;
end;

function VisionModelAlreadyInstalled(): Boolean;
begin
  Result := FileSizeOf(ModelsDir + '\' + VisionModelFileName) > VisionModelMinBytes;
end;

// Called before the wizard opens. Creates the dedicated
// "AI Model" download page used between Ready and Installing.
procedure InitializeWizard();
begin
  DownloadModel := True;

  ModelPage := CreateDownloadPage(
    'Downloading AI Model',
    '{#MyAppName} Setup is downloading the on-device AI model.',
    nil);
  ModelPage.Description :=
    'Your AI model (Qwen2.5-VL 3B, approx. 2.8 GB) is being downloaded. ' +
    'This runs completely on your device after installation and may take several minutes ' +
    'depending on your internet speed. No data ever leaves your machine.';
end;

// On the Ready page, if the model must be downloaded, do it here so the
// user sees a proper progress page BEFORE files are installed.
function NextButtonClick(CurPageID: Integer): Boolean;
var
  TmpChat, TmpVision, DestChat, DestVision: String;
  NeedChat, NeedVision: Boolean;
begin
  Result := True;

  if CurPageID <> wpReady then
    Exit;

  NeedChat := not ChatModelAlreadyInstalled();
  NeedVision := (not VisionModelAlreadyInstalled()) and NeedChat;

  if not NeedChat then
  begin
    Log('AI model already present - skipping download.');
    Exit;
  end;

  if not DownloadModel then
  begin
    Log('User opted out of model download - extraction will run without AI fields.');
    Exit;
  end;

  CreateDir(ModelsDir);

  ModelPage.Show;
  try
    try
      ModelPage.Add(ChatModelUrl, ChatModelFileName, '');
      if NeedVision then
        ModelPage.Add(VisionModelUrl, VisionModelFileName, '');
      ModelPage.Download;   // downloads into {tmp}

      TmpChat := ExpandConstant('{tmp}\') + ChatModelFileName;
      TmpVision := ExpandConstant('{tmp}\') + VisionModelFileName;
      DestChat := ModelsDir + '\' + ChatModelFileName;
      DestVision := ModelsDir + '\' + VisionModelFileName;

      if (not FileExists(TmpChat)) or (not CopyFile(TmpChat, DestChat, False)) then
      begin
        // The download failed - ask the user, but never block installation.
        if MsgBox('The AI model could not be downloaded or saved to:'#13#10#13#10 +
                  ModelsDir + #13#10#13#10 +
                  'The app still works, but lead extraction will have no AI fields ' +
                  '(name, company, job title, icebreakers). You can add the model later by ' +
                  'running Tools\download-vision-model.ps1.'#13#10#13#10 +
                  'Continue with the installation anyway?',
                  mbError, MB_YESNO) = IDNO then
          Result := False;
      end
      else
      begin
        Log('AI model downloaded to ' + DestChat);
        // The projector is optional - the app works from the transcript alone.
        if NeedVision and (FileExists(TmpVision)) then
        begin
          if CopyFile(TmpVision, DestVision, False) then
            Log('Vision projector downloaded to ' + DestVision)
          else
            Log('Vision projector could not be saved - continuing without vision.');
        end;
      end;
    except
      // The download failed - ask the user, but never block installation.
      if MsgBox('The AI model could not be downloaded:'#13#10 +
                GetExceptionMessage + #13#10#13#10 +
                'The app still works, but lead extraction will have no AI fields ' +
                '(name, company, job title, icebreakers). You can add the model later by ' +
                'running Tools\download-vision-model.ps1.'#13#10#13#10 +
                'Continue with the installation anyway?',
                mbCriticalError, MB_YESNO) = IDNO then
        Result := False;
    end;
  finally
    ModelPage.Hide;
  end;
end;

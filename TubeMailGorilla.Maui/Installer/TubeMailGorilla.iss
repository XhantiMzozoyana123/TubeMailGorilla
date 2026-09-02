; =====================================================================
; TubeMailGorilla - Simple Windows Installer (Inno Setup - no WiX)
;
; Build it with:  Installer\build-installer.ps1
;
; The installation wizard installs the app AND downloads the on-device
; AI model (Llama 3.2 3B Instruct GGUF, ~1.9 GB) into the install
; folder during setup, so users never have to do anything technical.
; =====================================================================

#define MyAppName "TubeMailGorilla"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "TubeMailGorilla"
#define MyAppExeName "TubeMailGorilla.Maui.exe"
#define ModelFileName "Llama-3.2-3B-Instruct-Q4_K_M.gguf"
#define ModelUrl "https://huggingface.co/bartowski/Llama-3.2-3B-Instruct-GGUF/resolve/main/Llama-3.2-3B-Instruct-Q4_K_M.gguf"
; Path to the dotnet publish output (created by build-installer.ps1)
#define PublishDir "..\bin\Release\net10.0-windows10.0.19041.0\win-x64\publish"

[Setup]
AppId={{8F3A6C21-9B47-4E02-9D51-6A8C0B7E4F12}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
PrivilegesRequired=admin
OutputBaseFilename=TubeMailGorilla-setup
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
; Remove the downloaded AI model when uninstalling
Type: files; Name: "{app}\{#ModelFileName}"

[Code]
const
  ModelUrl = '{#ModelUrl}';
  ModelFileName = '{#ModelFileName}';

var
  ModelPage: TDownloadWizardPage;
  DownloadModel: Boolean;

// Called before the wizard opens. Creates the dedicated
// "AI Model" download page used between Ready and Installing.
procedure InitializeWizard();
begin
  DownloadModel := True;

  ModelPage := CreateDownloadPage(
    'Downloading AI Model',
    'TubeMailGorilla Setup is downloading the on-device AI model.',
    nil);
  ModelPage.Description :=
    'Your AI model (Llama 3.2 3B, approx. 1.9 GB) is being downloaded. ' +
    'This runs completely on your device after installation and may take several minutes ' +
    'depending on your internet speed. No data ever leaves your machine.';
end;

// Returns True when a valid copy of the model is already installed
// (upgrade scenario) - a truncated/corrupt file is re-downloaded.
function ModelAlreadyInstalled(): Boolean;
var
  F: TFindRec;
begin
  Result := False;
  if not DirExists(WizardDirValue) then
    Exit;
  if FindFirst(AddBackslash(WizardDirValue) + ModelFileName, F) then
  try
    Result := ((F.Attributes and FILE_ATTRIBUTE_DIRECTORY) = 0) and
              ((Int64(F.SizeHigh) * 4294967296 + Int64(F.SizeLow)) > 1073741824);   // real model is ~1.9 GB
  finally
    FindClose(F);
  end;
end;

// On the Ready page, if the model must be downloaded, do it here so the
// user sees a proper progress page BEFORE files are installed.
function NextButtonClick(CurPageID: Integer): Boolean;
var
  TmpFile, DestFile: String;
begin
  Result := True;

  if CurPageID <> wpReady then
    Exit;

  if ModelAlreadyInstalled() then
  begin
    Log('AI model already present - skipping download.');
    Exit;
  end;

  if not DownloadModel then
  begin
    Log('User opted out of model download - the app will download it on first use.');
    Exit;
  end;

  ModelPage.Show;
  try
    try
      ModelPage.Add(ModelUrl, ModelFileName, '');
      ModelPage.Download;   // downloads into {tmp}

      TmpFile := ExpandConstant('{tmp}\') + ModelFileName;
      DestFile := AddBackslash(WizardDirValue) + ModelFileName;
      if not CopyFile(TmpFile, DestFile, False) then
      begin
        // The download failed - ask the user, but never block installation.
        if MsgBox('The AI model could not be moved into the install folder.'#13#10#13#10 +
                  'You can continue - TubeMailGorilla will download the model automatically ' +
                  'the first time it needs it. Continue with the installation anyway?',
                  mbError, MB_YESNO) = IDNO then
          Result := False;
      end
      else
        Log('AI model downloaded to ' + DestFile);
    except
      // The download failed - ask the user, but never block installation.
      if MsgBox('The AI model could not be downloaded:'#13#10 +
                GetExceptionMessage + #13#10#13#10 +
                'You can continue - TubeMailGorilla will download the model automatically ' +
                'the first time it needs it. Continue with the installation anyway?',
                mbCriticalError, MB_YESNO) = IDNO then
        Result := False;
    end;
  finally
    ModelPage.Hide;
  end;
end;

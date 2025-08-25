[Setup]
AppName=AL&SA PVB
AppVersion=1.0
DefaultDirName={autopf}\AL&SA PVB
DefaultGroupName=AL&SA PVB
OutputDir=Output
OutputBaseFilename=AL&SA PVB Installer
Compression=lzma
SolidCompression=yes
UninstallFilesDir={app}\Uninstall
SetupIconFile=..\PvPlantPlanner.UI\Logo\green-energy.ico
UninstallDisplayName=AL&SA PVB

[Files]
Source: "..\bin\Release\net8.0-windows\*"; DestDir: "{app}"; Flags: recursesubdirs ignoreversion comparetimestamp createallsubdirs
Source: "..\PvPlantPlanner.UI\Logo\green-energy.ico"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net8.0-windows\AL&SA_LicenseHashGenerator.exe"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\Uninstall AL&SA PVB"; Filename: "{uninstallexe}"

[Code]
var
  LicensePage: TWizardPage;
  LicenseEdit: TEdit;

function IsValidLicense(Lic: string): Boolean;
begin
  Result := Trim(Lic) = '123XYZ';
end;

procedure CurPageChanged(CurPageID: Integer);
begin
  if CurPageID = LicensePage.ID then
    WizardForm.NextButton.Enabled := False;
end;

procedure LicenseEditChange(Sender: TObject);
begin
  WizardForm.NextButton.Enabled := IsValidLicense(LicenseEdit.Text);
end;

procedure SaveLicense(Lic: string);
var
  LicenseFile: string;
begin
  LicenseFile := ExpandConstant('{app}\license.txt');
  try
    SaveStringToFile(LicenseFile, Lic, False);
  except
    MsgBox('Ne mogu da sačuvam licencu!', mbError, MB_OK);
  end;
end;

procedure CreateDesktopShortcut();
var
  LinkPath, TargetPath: string;
  WshShell, Shortcut: Variant;
begin
  LinkPath := ExpandConstant('{userdesktop}\AL&SA PVB.lnk');
  TargetPath := ExpandConstant('{app}\win-x64\AL&SA PVB.exe');

  if FileExists(TargetPath) then
  begin
    WshShell := CreateOleObject('WScript.Shell');
    Shortcut := WshShell.CreateShortcut(LinkPath);
    Shortcut.TargetPath := TargetPath;
    Shortcut.WorkingDirectory := ExpandConstant('{app}');
    Shortcut.IconLocation := TargetPath + ',0';
    Shortcut.Save;
  end;
end;

procedure InitializeWizard();
begin
  WizardForm.BringToFront;

  LicensePage := CreateCustomPage(wpSelectDir, 'Enter license code',
    'Please enter your license key for AL&SA PVB:');

  LicenseEdit := TEdit.Create(WizardForm);
  LicenseEdit.Parent := LicensePage.Surface;
  LicenseEdit.Left := 0;
  LicenseEdit.Top := 0;
  LicenseEdit.Width := LicensePage.SurfaceWidth;
  LicenseEdit.OnChange := @LicenseEditChange;

  WizardForm.NextButton.Enabled := False;
end;

procedure CurStepChanged(CurStep: TSetupStep);
var
  ResultCode: Integer;
  HashExe: string;
begin
  if CurStep = ssPostInstall then
  begin
    CreateDesktopShortcut();
    if Assigned(LicenseEdit) then
      SaveLicense(LicenseEdit.Text);

    // Pozovi AL&SA_LicenseHashGenerator.exe
    HashExe := ExpandConstant('{app}\AL&SA_LicenseHashGenerator.exe');
    if FileExists(HashExe) then
    begin
      if not Exec(HashExe, '', '', SW_SHOW, ewWaitUntilTerminated, ResultCode) then
        MsgBox('Greška pri pokretanju AL&SA_LicenseHashGenerator.exe!', mbError, MB_OK);
    end;
  end;
end;

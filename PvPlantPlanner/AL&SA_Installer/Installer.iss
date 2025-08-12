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

[Icons]
Name: "{group}\Uninstall AL&SA PVB"; Filename: "{uninstallexe}"

[Code]
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

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
    CreateDesktopShortcut();
end;

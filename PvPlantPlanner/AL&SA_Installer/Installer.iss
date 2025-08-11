[Setup]
AppName=AL&SA PVB
AppVersion=1.0
DefaultDirName={pf}\AL&SA PVB
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

[Icons]
Name: "{group}\AL&SA PVB"; Filename: "{app}\AL&SA PVB.exe"
Name: "{group}\Uninstall AL&SA PVB"; Filename: "{uninstallexe}"
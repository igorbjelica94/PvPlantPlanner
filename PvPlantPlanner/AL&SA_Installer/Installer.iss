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
Name: "{userdesktop}\AL&SA PVB"; Filename: "{app}\AL&&SA PVB.exe"; IconFilename: "{app}\green-energy.ico"; WorkingDir: "{app}"

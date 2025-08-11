@echo off
setlocal DisableDelayedExpansion
setlocal EnableExtensions

REM ===== KONFIGURACIJA =====
REM Putanja do foldera gde je skripta
set "SCRIPT_DIR=%~dp0"

REM Pretvori relativne u apsolutne putanje
for %%I in ("%SCRIPT_DIR%..\PvPlantPlanner.UI\PvPlantPlanner.UI.csproj") do set "APP_PROJECT_PATH=%%~fI"

REM Postavi apsolutnu putanju do Inno Setup skripte
for %%I in ("%SCRIPT_DIR%..\AL&SA_Installer\Installer.iss") do set "ISS_SCRIPT=%%~fI"

REM Putanja do Inno Setup kompajlera
set "ISCC_PATH=C:\Program Files (x86)\Inno Setup 6\ISCC.exe"

REM Folder gde će ići publish (apsolutna putanja)
for %%I in ("%SCRIPT_DIR%publish\win-x64\Release") do set "PUBLISH_DIR=%%~fI"

echo APP_PROJECT_PATH="%APP_PROJECT_PATH%"
if not exist "%APP_PROJECT_PATH%" (
    echo ERROR: Projektni fajl ne postoji!
    exit /b 1
)

echo ISS_SCRIPT="%ISS_SCRIPT%"
if not exist "%ISS_SCRIPT%" (
    echo ERROR: Installer skripta ne postoji!
    exit /b 1
)

echo PUBLISH_DIR="%PUBLISH_DIR%"

REM Ostale postavke
set "CONFIGURATION=Release"
set "RUNTIME=win-x64"
set "APP_NAME=AL&SA PVB"
set "APP_VERSION=1.0.0"
set "PUBLISH_SINGLE_FILE=true"

REM ===== 1. PUBLISH APLIKACIJE =====
echo Pokrecem dotnet publish...
dotnet publish "%APP_PROJECT_PATH%" -c %CONFIGURATION% -r %RUNTIME% --self-contained true ^
 /p:PublishSingleFile=%PUBLISH_SINGLE_FILE% /p:PublishTrimmed=%PUBLISH_TRIMMED% ^
 -o "%PUBLISH_DIR%"
if errorlevel 1 (
    echo ERROR: dotnet publish nije uspio.
    exit /b 1
)

REM ===== 2. POKRETANJE INNO SETUP KOMPILACIJE =====
if exist "%ISCC_PATH%" (
    echo Pokrecem Inno Setup Compiler...
    "%ISCC_PATH%" /DMyAppName="%APP_NAME%" /DMyAppVersion="%APP_VERSION%" /DMyPublishDir="%PUBLISH_DIR%" "%ISS_SCRIPT%"
    if errorlevel 1 (
        echo ERROR: ISCC izvrsenje nije uspjelo.
        exit /b 1
    )
    echo Installer je kreiran.
) else (
    echo GRESKA: ISCC.exe nije pronadjen na "%ISCC_PATH%".
)

endlocal
pause

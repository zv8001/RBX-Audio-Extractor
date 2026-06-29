@echo off
setlocal
rem Sign a release build so the in-app updater will accept it.
rem Double-click to sign the default published exe, or drag an exe onto this file,
rem or run:  sign-release.cmd "path\to\RBXAssetExtractor.exe"

rem Jump to the repo root (this script lives in tools\).
pushd "%~dp0.."

set "EXE=%~1"
if "%EXE%"=="" set "EXE=RBX-Audio-Extractor2\bin\Release\net10.0-windows\win-x64\publish\win-x64\RBXAssetExtractor.exe"

if not exist "%EXE%" ( echo Executable not found: "%EXE%" & echo Build/publish a Release first, or pass/drag the exe path. & popd & pause & exit /b 1 )

rem Resolve the exe's folder (absolute) while we are still at the repo root.
for %%I in ("%EXE%") do set "EXEDIR=%%~dpI"
if defined EXEDIR if "%EXEDIR:~-1%"=="\" set "EXEDIR=%EXEDIR:~0,-1%"

echo Signing "%EXE%"
echo.
dotnet run --project tools/UpdateSigner -- sign "%EXE%"
set "RC=%ERRORLEVEL%"

popd
echo.
if "%RC%"=="0" echo Done. Upload BOTH the .exe and the new .exe.sig to your update host.
if "%RC%"=="0" if defined EXEDIR start "" "%EXEDIR%"
if not "%RC%"=="0" echo Signing FAILED (exit %RC%).
pause
exit /b %RC%

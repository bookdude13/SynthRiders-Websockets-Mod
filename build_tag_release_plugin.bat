@echo off

IF %1.==. goto :Usage

set MOD_NAME="SRWebsocketsFixDepsPlugin"
set BUILD_SCRIPT=.\SRModCore\build.py
set VERSION=%1

echo "Building plugin release..."
python.exe %BUILD_SCRIPT% --clean --plugin -s .\SynthRidersWebsockets\SynthRidersWebsockets.sln -n "%MOD_NAME%" -c Release %VERSION% plugin_files.txt || goto :ERROR

echo "Done"
goto :EOF

:ERROR
echo "Error occurred in build script! Error code: %errorlevel%"

:Usage
echo "Usage: ./build_tag_release_plugin.bat <version>"

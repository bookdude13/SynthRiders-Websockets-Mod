@echo off

set MOD_NAME="SynthRidersWebsockets"
set BUILD_SCRIPT=.\SRModCore\build.py
set SYNTHRIDERS_MODS_DIR="C:\Program Files (x86)\Steam\steamapps\common\SynthRiders\Mods"

set PLUGIN_NAME="SRWebsocketsFixDepsPlugin"
set SYNTHRIDERS_PLUGINS_DIR="C:\Program Files (x86)\Steam\steamapps\common\SynthRiders\Plugins"

echo "Building dev configuration"
python.exe %BUILD_SCRIPT% --clean -n %MOD_NAME% -c Debug -o build\localdev localdev build_files.txt || goto :ERROR

echo "Copying to SR directory..."
@REM Building spits out raw file structure in build/localdev/raw
copy build\localdev\Mods\* %SYNTHRIDERS_MODS_DIR% || goto :ERROR

echo "Building plugin..."
python.exe %BUILD_SCRIPT% --plugin -s .\SynthRidersWebsockets\SynthRidersWebsockets.sln -n %PLUGIN_NAME% -c Debug -o build\localdev localdev plugin_files.txt || goto :ERROR

echo "Copying plugin..."
copy build\localdev\Plugins\* %SYNTHRIDERS_PLUGINS_DIR% || goto :ERROR

echo "Done"
goto :EOF

:ERROR
echo "Error occurred in build script! Error code: %errorlevel%"

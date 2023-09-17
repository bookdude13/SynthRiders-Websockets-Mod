set MOD_NAME="SynthRidersWebsockets"

set BUILD_DIR=".\src\bin\Debug\net6.0"
set BUILT_DLL=%BUILD_DIR%\%MOD_NAME%.dll

set DEPENDENCY_DLLS=%BUILD_DIR%\WebSocketDotNet.dll
set DEPENDENCY_DLLS=%DEPENDENCY_DLLS% %BUILD_DIR%\Microsoft.AspNetCore.Connections.Abstractions.dll
set DEPENDENCY_DLLS=%DEPENDENCY_DLLS% %BUILD_DIR%\Microsoft.AspNetCore.Http.Connections.Client.dll
set DEPENDENCY_DLLS=%DEPENDENCY_DLLS% %BUILD_DIR%\Microsoft.AspNetCore.Http.Connections.Common.dll
set DEPENDENCY_DLLS=%DEPENDENCY_DLLS% %BUILD_DIR%\Microsoft.AspNetCore.SignalR.Client.Core.dll
set DEPENDENCY_DLLS=%DEPENDENCY_DLLS% %BUILD_DIR%\Microsoft.AspNetCore.SignalR.Client.dll
set DEPENDENCY_DLLS=%DEPENDENCY_DLLS% %BUILD_DIR%\Microsoft.AspNetCore.SignalR.Common.dll
set DEPENDENCY_DLLS=%DEPENDENCY_DLLS% %BUILD_DIR%\Microsoft.AspNetCore.SignalR.Protocols.Json.dll
set DEPENDENCY_DLLS=%DEPENDENCY_DLLS% %BUILD_DIR%\Microsoft.Extensions.DependencyInjection.Abstractions.dll
set DEPENDENCY_DLLS=%DEPENDENCY_DLLS% %BUILD_DIR%\Microsoft.Extensions.DependencyInjection.dll
set DEPENDENCY_DLLS=%DEPENDENCY_DLLS% %BUILD_DIR%\Microsoft.Extensions.Features.dll
REM set DEPENDENCY_DLLS=%DEPENDENCY_DLLS% %BUILD_DIR%\Microsoft.Extensions.Logging.Abstractions.dll
set DEPENDENCY_DLLS=%DEPENDENCY_DLLS% %BUILD_DIR%\Microsoft.Extensions.Logging.dll
REM set DEPENDENCY_DLLS=%DEPENDENCY_DLLS% %BUILD_DIR%\Microsoft.Extensions.Options.dll
REM set DEPENDENCY_DLLS=%DEPENDENCY_DLLS% %BUILD_DIR%\Microsoft.Extensions.Primitives.dll
set DEPENDENCY_DLLS=%DEPENDENCY_DLLS% %BUILD_DIR%\System.IO.Pipelines.dll
REM set DEPENDENCY_DLLS=%DEPENDENCY_DLLS% %BUILD_DIR%\System.Threading.Channels.dll

REM set LIB_DLL_DIR=".\src\bin\Debug\net6.0\libs"
set SYNTHRIDERS_MODS_DIR="C:\Program Files (x86)\Steam\steamapps\common\SynthRiders\Mods"

copy %BUILT_DLL% %SYNTHRIDERS_MODS_DIR%

for %%d in (%DEPENDENCY_DLLS%) do ( 
    copy %%d %SYNTHRIDERS_MODS_DIR%
)

REM copy %LIB_DLL_DIR%\* %SYNTHRIDERS_MODS_DIR%

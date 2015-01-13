@echo off

REM Fill these two values in
set NUGET_API_KEY=
set NUGET_SERVER=

set NUGET_NUSPEC=src\RazorSharp.nuspec
set SCRIPT_DIR=%~dp0
set ROOT_DIR=%SCRIPT_DIR%..\..

IF NOT [%NUGET_API_KEY%] == [] GOTO :good

echo You must fill out NUGET_API_KEY and NUGET_SERVER to use this script
goto :eof

:good

REM TODO (roy): Add the -Symbols flag and upload to SymbolSource also

pushd %ROOT_DIR%

IF EXIST build rd /S /Q build
mkdir build

nuget pack %NUGET_NUSPEC%               ^
           -Build                       ^
           -OutputDirectory build

for /f %%f in ('dir /b build')          ^
DO nuget push build\%%f                 ^
              -Source %NUGET_SERVER%    ^
              -ApiKey %NUGET_API_KEY%

popd

@echo off
rem Usage: build.bat            -> bin\Debug\UIComponents.Wpf.exe
rem        build.bat <agent>    -> bin\<agent>\UIComponents.Wpf.exe (own copy; parallel agents never lock each other)
set MSBUILD=C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe
set OUT=bin\Debug\
set EXTRA=
if not "%~1"=="" (
  set OUT=bin\%~1\
  set EXTRA=/p:OutputPath=bin\%~1\ /p:IntermediateOutputPath=obj\%~1\
)
"%MSBUILD%" "%~dp0UIComponents.Wpf.csproj" /t:Build /p:Configuration=Debug /v:minimal %EXTRA%
if errorlevel 1 goto fail
echo Built %OUT%UIComponents.Wpf.exe
exit /b 0
:fail
echo Build failed.
exit /b 1

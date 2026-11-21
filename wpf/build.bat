@echo off
set MSBUILD=C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe
"%MSBUILD%" "%~dp0UIComponents.Wpf.csproj" /t:Build /p:Configuration=Debug /v:minimal
if errorlevel 1 goto fail
echo Built bin\Debug\UIComponents.Wpf.exe
exit /b 0
:fail
echo Build failed.
exit /b 1

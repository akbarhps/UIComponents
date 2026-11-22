@echo off
rem Usage: build-tests.bat                      -> bin\UIAutomationTests.exe (shared), all Tests.*.cs
rem        build-tests.bat <agent>              -> bin\<agent>\UIAutomationTests.exe (own copy; run-hidden.ps1 -Agent <agent> uses it, so parallel agents never lock each other)
rem        build-tests.bat <agent> <area> ...   -> Tests.Core.cs + Tests.<area>.cs only (a worker's build is not broken by another worker's file)
set CSC=C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe
set REF=C:\Windows\Microsoft.NET\Framework\v4.0.30319
set OUT=bin
if not "%~1"=="" set OUT=bin\%~1
set SRC=Tests.*.cs
shift
if "%~1"=="" goto build
set SRC=Tests.Core.cs
:areas
if "%~1"=="" goto build
set SRC=%SRC% Tests.%~1.cs
shift
goto areas
:build
if not exist %OUT% mkdir %OUT%
"%CSC%" /nologo /target:exe /out:%OUT%\UIAutomationTests.exe /langversion:5 ^
  /r:System.dll /r:System.Core.dll /r:System.Windows.Forms.dll ^
  /r:C:\Windows\assembly\GAC_MSIL\UIAutomationClient\3.0.0.0__31bf3856ad364e35\UIAutomationClient.dll ^
  /r:C:\Windows\assembly\GAC_MSIL\UIAutomationTypes\3.0.0.0__31bf3856ad364e35\UIAutomationTypes.dll %SRC%
if errorlevel 1 goto fail
echo Built %OUT%\UIAutomationTests.exe
exit /b 0
:fail
echo Test build failed.
exit /b 1

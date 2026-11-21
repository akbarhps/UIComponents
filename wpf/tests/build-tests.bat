@echo off
set CSC=C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe
set REF=C:\Windows\Microsoft.NET\Framework\v4.0.30319
if not exist bin mkdir bin
"%CSC%" /nologo /target:exe /out:bin\UIAutomationTests.exe /langversion:5 ^
  /r:System.dll /r:System.Core.dll ^
  /r:C:\Windows\assembly\GAC_MSIL\UIAutomationClient\3.0.0.0__31bf3856ad364e35\UIAutomationClient.dll ^
  /r:C:\Windows\assembly\GAC_MSIL\UIAutomationTypes\3.0.0.0__31bf3856ad364e35\UIAutomationTypes.dll UIAutomationTests.cs
if errorlevel 1 goto fail
echo Built bin\UIAutomationTests.exe
exit /b 0
:fail
echo Test build failed.
exit /b 1

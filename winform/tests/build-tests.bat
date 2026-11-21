@echo off
set CSC=C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe
set SRC=..\src
if not exist bin mkdir bin
"%CSC%" /nologo /target:exe /out:bin\ComponentTests.exe /r:System.dll /r:System.Core.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll %SRC%\Theme.cs %SRC%\Draw.cs %SRC%\BasicControls.cs %SRC%\FormControls.cs %SRC%\NavControls.cs %SRC%\SelectionControls.cs %SRC%\DataControls.cs %SRC%\OverlayControls.cs %SRC%\DemoForm.cs ComponentTests.cs
if errorlevel 1 goto fail
echo Built bin\ComponentTests.exe
exit /b 0
:fail
echo Test build failed.
exit /b 1

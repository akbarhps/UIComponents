@echo off
set CSC=C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe
if not exist bin mkdir bin
"%CSC%" /nologo /target:winexe /out:bin\UIComponents.exe /r:System.dll /r:System.Core.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll src\Theme.cs src\Draw.cs src\BasicControls.cs src\FormControls.cs src\CustomInputs.cs src\CustomCalendar.cs src\NavControls.cs src\SelectionControls.cs src\DataControls.cs src\OverlayControls.cs src\DemoForm.cs src\Program.cs
if errorlevel 1 goto fail
echo Built bin\UIComponents.exe
exit /b 0
:fail
echo Build failed.
exit /b 1

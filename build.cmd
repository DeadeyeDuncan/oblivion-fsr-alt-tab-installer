@echo off
setlocal
cd /d "%~dp0"
set "FSR_CSC=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if not exist "%FSR_CSC%" exit /b 1
"%FSR_CSC%" /nologo /target:winexe /r:System.Xml.Linq.dll /r:System.Windows.Forms.dll /r:System.Drawing.dll "/out:Apply FSR Fix.exe" App.cs Fix.cs
exit /b %errorlevel%

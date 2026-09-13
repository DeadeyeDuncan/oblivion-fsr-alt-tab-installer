@echo off
setlocal
cd /d "%~dp0"
set "FSR_CSC=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
"%FSR_CSC%" /nologo /define:TEST /r:System.Xml.Linq.dll /out:Checks.exe Tests.cs Fix.cs
if errorlevel 1 exit /b %errorlevel%
Checks.exe
exit /b %errorlevel%

@echo off
echo Compiling ShadowLock Builder...
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /target:winexe /platform:x64 /reference:System.Windows.Forms.dll /reference:System.dll /reference:Microsoft.VisualBasic.dll ShadowLockBuilder.cs
echo Done. Run ShadowLockBuilder.exe
pause
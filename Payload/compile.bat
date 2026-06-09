@echo off
echo Compiling ShadowLock Payload...
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /target:winexe /platform:x64 /reference:System.Windows.Forms.dll /reference:System.dll /reference:Microsoft.VisualBasic.dll ShadowLockPayload.cs
echo Payload.exe created.
pause
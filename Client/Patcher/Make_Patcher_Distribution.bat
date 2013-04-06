@echo off

set CRT_ASSEMBLIES=c:\Program Files\Microsoft Visual Studio 9.0\VC\redist\x86
IF NOT EXIST "%CRT_ASSEMBLIES%" set CRT_ASSEMBLIES=c:\Program Files (x86)\Microsoft Visual Studio 9.0\VC\redist\x86
IF NOT EXIST "%CRT_ASSEMBLIES%" GOTO:NOVC

cd Distribution || GOTO:NODIR

:: Clean the old distro files.
rd  /Q /S assets
del /Q *.*

mkdir assets

copy ..\config.patcher .
:: copy ..\..\..\Release\Launcher.exe . || GOTO:NOBUILD
copy ..\..\..\Release\Patcher.exe . || GOTO:NOBUILD
copy ..\..\..\Release\options.exe.config .\Client.exe.config
copy ..\..\..\Release\options.exe . || GOTO:NOBUILD
cd ..
xcopy /EXCLUDE:exclusions.txt /E assets Distribution\assets
cd Distribution
copy "%CRT_ASSEMBLIES%\Microsoft.VC90.CRT" . || GOTO:MISSINGDLL
copy "%ICEROOT%\bin\bzip2.dll" . || GOTO:MISSINGDLL
copy "%ICEROOT%\bin\Glacier2.dll" . || GOTO:MISSINGDLL
copy "%ICEROOT%\bin\glacier234.dll" . || GOTO:MISSINGDLL
copy "%ICEROOT%\bin\ice34.dll" . || GOTO:MISSINGDLL
copy "%ICEROOT%\bin\Ice.dll" . || GOTO:MISSINGDLL
copy "%ICEROOT%\bin\icessl34.dll" . || GOTO:MISSINGDLL
copy "%ICEROOT%\bin\IceSSL.dll" . || GOTO:MISSINGDLL
copy "%ICEROOT%\bin\iceutil34.dll" . || GOTO:MISSINGDLL
copy "%ICEROOT%\bin\slice34.dll" . || GOTO:MISSINGDLL
copy "%ICEROOT%\bin\icepatch234.dll" . || GOTO:MISSINGDLL
copy "%ICEROOT%\bin\libeay32.dll" . || GOTO:MISSINGDLL

cd ..
echo Operation successful.
GOTO:EOF

:ERROR
echo Cannot move into Distribution directory.
GOTO:PAUSE

:NOVC
echo It appears that you do not have MSVC 9.0 installed, which is required.
GOTO:PAUSE

:NODIR
echo The 'Distribution' directory cannot be found.
GOTO:PAUSE

:NOBUILD
echo Please build the launcher on 'Release' mode before running this script.
GOTO:PAUSE

:MISSINGDLL
echo Missing a necessary DLL. Please make sure it exists and try again.
GOTO:PAUSE

:PAUSE
PAUSE
GOTO:EOF

:EOF

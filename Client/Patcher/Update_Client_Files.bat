@echo off
:: Build a new distribution of the client/patcher.
set CRT_ASSEMBLIES=c:\Program Files\Microsoft Visual Studio 9.0\VC\redist\x86
IF NOT EXIST "%CRT_ASSEMBLIES%" set CRT_ASSEMBLIES=c:\Program Files (x86)\Microsoft Visual Studio 9.0\VC\redist\x86
IF NOT EXIST "%CRT_ASSEMBLIES%" GOTO:NOVC

:: Start in the Files directory: that'll be our root.
cd Files || ((mkdir Files && cd Files) || GOTO:ERROR1)
rd /Q /S assets
rd /Q /S Content
del /Q /S *.*

mkdir assets

:: Move back into the starting directory.
cd ..

:: Copy the client binaries into the Files\client folder (Reading from exclusions.txt for exclusions).
xcopy /EXCLUDE:exclusions.txt /E ..\Driver\bin\x86\Release .\Files || GOTO:ERROR_XCOPY
xcopy /EXCLUDE:exclusions.txt /E assets .\Files\assets

:: Go back into the client folder.
cd Files

:: Copy required Ice DLL files/libraries.
copy "%ICEROOT%\bin\bzip2.dll" .
copy "%ICEROOT%\bin\Glacier2.dll" .
copy "%ICEROOT%\bin\glacier233.dll" .
copy "%ICEROOT%\bin\ice33.dll" .
copy "%ICEROOT%\bin\Ice.dll" .
copy "%ICEROOT%\bin\icessl33.dll" .
copy "%ICEROOT%\bin\IceSSL.dll" .
copy "%ICEROOT%\bin\iceutil33.dll" .
copy "%ICEROOT%\bin\slice33.dll" .
copy "%ICEROOT%\bin\libeay32.dll" .
copy ..\..\..\Release\Launcher.exe . || GOTO:ERROR_PATCHER
copy ..\..\..\Release\options.exe . || GOTO:ERROR_PATCHER
copy ..\..\..\Release\Patcher.exe . || GOTO:ERROR_PATCHER
copy ..\config.patcher .

:: Run Icepatch2Calc to re-calculate the checksums. (TODO: This is probably unnecessary.)
"%ICEROOT%\bin\icepatch2calc" .

cd ..

GOTO:EOF

:NOVC
echo Visual Studio 2008 is required to create a distribution.
GOTO:EOF

:ERROR1
echo Cannot create directory: Files.
GOTO:EOF

:ERROR2
echo Cannot create directory: Files\client.
GOTO:EOF

:ERROR3
echo Cannot create directory: Files\patcher.
GOTO:EOF

:ERROR_XCOPY
echo Couldn't find the Client/Release files. Make sure the Release configuration has been built.
GOTO:EOF

:ERROR_PATCHER
echo Couldn't find the patcher executables. Please build the entire VTank.sln Release config!
GOTO:EOF

:EOF

@echo off

set CRT_ASSEMBLIES=c:\Program Files\Microsoft Visual Studio 9.0\VC\redist\x86
IF NOT EXIST "%CRT_ASSEMBLIES%" set CRT_ASSEMBLIES=c:\Program Files (x86)\Microsoft Visual Studio 9.0\VC\redist\x86
IF NOT EXIST "%CRT_ASSEMBLIES%" GOTO:NOVC
cd Distribution || GOTO:CD_ERROR

:: Clean the old distro files.
rd  /Q /S data
rd  /Q /S "Gardener Help"
del /Q *.*

rem Build the new distribution.
copy ..\..\Release\Gardener.exe .
copy ..\Gardener-sample.cfg Gardener.cfg
copy ..\tile_dictionary.txt .
copy ..\object_dictionary.txt .
copy ..\event_dictionary.txt .
copy ..\config.gardener .
md data
xcopy /E ..\data data
md "Gardener Help"
xcopy /E "..\Gardener Help" "Gardener Help"
copy "%CRT_ASSEMBLIES%\Microsoft.VC90.CRT" .
copy "%ICEROOT%\bin\bzip2.dll" .
copy "%ICEROOT%\bin\Glacier2.dll" .
copy "%ICEROOT%\bin\glacier234.dll" .
copy "%ICEROOT%\bin\ice34.dll" .
copy "%ICEROOT%\bin\Ice.dll" .
copy "%ICEROOT%\bin\icessl34.dll" .
copy "%ICEROOT%\bin\IceSSL.dll" .
copy "%ICEROOT%\bin\iceutil34.dll" .
copy "%ICEROOT%\bin\slice34.dll" .
copy "%ICEROOT%\bin\libeay32.dll" .


set CRT_ASSEMBLIES=

zip -r Gardener-0.0.zip *

GOTO:EOF

:NOVC
echo It appears that you do not have MSVC 9.0 installed, which is required.
GOTO:EOF

:CD_ERROR
echo ERROR: "Distribution" directory does not exist. Create it and try again.
:EOF

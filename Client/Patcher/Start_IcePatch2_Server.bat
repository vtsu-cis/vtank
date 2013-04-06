@echo off

title VTank Patch Server

"%ICEROOT%\bin\icepatch2calc" Files || (echo Couldn't start icepatch2calc && GOTO:EOF)

start "IcePatch2 Server" "%ICEROOT%"\bin\icepatch2server --Ice.Config=config.icepatch2

:EOF

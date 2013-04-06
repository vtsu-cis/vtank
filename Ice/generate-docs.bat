@echo off

SET HTML_TARGET=%1

:: If HTML_TARGET is an empty string, do 
IF [%HTML_TARGET%]==[] GOTO GENERATE

IF NOT EXIST %HTML_TARGET%^
    echo That directory doesn't not exist. && GOTO EOF

:GENERATE
IF [%HTML_TARGET%]==[] mkdir html && SET HTML_TARGET=html
:: FOR /F %%a IN ('dir /b *.ice') DO "%ICEROOT%\bin\slice2html" -I. -I"%ICEROOT%\slice" --output-dir html\%%a %%a
"%ICEROOT%\bin\slice2html" -I. -I"%ICEROOT%\slice" --output-dir "%HTML_TARGET%" Exception.ice VTankObjects.ice Main.ice IGame.ice MainSession.ice CaptainVTank.ice JanitorSession.ice MapEditorSession.ice GameSession.Ice JanitorCallback.ice MainToGameSession.ice ClockSync.ice HealthMonitor.ice

:EOF

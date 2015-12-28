@echo off

:: Give the user the option to disable 'Success' output with the --quiet [-q] command.
:: The batch file will still report failures.
SET QUIET=0
IF [%1]==[-q] SET QUIET=1
IF [%1]==[--quiet] SET QUIET=1

:: Check for empty variable and warn if it's not set.
IF [x%ICE_HOME%]==[x] ECHO WARNING: ICE_HOME is unset. Please set it in your system environment variables.

:: Location of the slice compiler.
SET SLICE_COMPILER=%ICE_HOME%\bin\slice2cpp

:: Directory where the *.ice files reside.
SET SLICE_DIR=.

:: Loop through the directory which has the slice code in it and compile every Ice file.
FOR /F %%a IN ('dir /b %SLICE_DIR%\*.ice') DO %SLICE_COMPILER% --stream -I%SLICE_DIR% -I%ICE_HOME%\slice --output-dir=%SLICE_DIR% %SLICE_DIR%\%%a && IF [%QUIET%]==[0] echo Success: %%a || echo Failure: %%a

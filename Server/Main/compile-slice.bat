@echo off

:: Give the user the option to disable 'Success' output with the --quiet [-q] command.
:: The batch file will still report failures.
SET QUIET=0
IF [%1]==[-q] SET QUIET=1
IF [%1]==[--quiet] SET QUIET=1

:: Check for empty variable and warn if it's not set.
IF [x%ICEROOT%]==[x] ECHO WARNING: ICEROOT is unset. Please set it in your system environment variables.

SET OUTPUT_DIR=ice\generated
SET SLICE_COMPILER=%ICEROOT%\bin\slice2py
SET SLICE_DIR=..\..\Ice

:: Loop through the directory which has the slice code in it and compile every Ice file.
FOR /F %%a IN ('dir /b %SLICE_DIR%\*.ice') DO %SLICE_COMPILER% -I%SLICE_DIR% -I%ICEROOT%\slice --output-dir=%OUTPUT_DIR% %SLICE_DIR%\%%a && IF [%QUIET%]==[0] echo Success: %%a || echo Failure: %%a

echo Finished.

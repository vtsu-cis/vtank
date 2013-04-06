@echo off
:: Start the Echelon server using the Python interpreter.

:: Specify the path to the Python interpreter (if it's on the PATH, set to "python").
:: This variable should include the executable itself. Example: C:\python\python.exe
SET PATH_TO_PYTHON=python.exe

:: Specify whether debug mode (verbose output to the console) should be enabled.
:: (1 = enabled, 0 = disabled)
SET DEBUG_MODE=0

:: Specify whether the Slice files should be re-compiled on the fly. This command can be set to
:: disabled if you plan to run 'compile-slice.bat' manually.
:: (1 = enabled, 0 = disabled)
SET RECOMPILE_SLICE=1

:: Below is the actual start-up script. Do not touch!

SET SWITCHES=
IF NOT "%DEBUG_MODE%" == "0"^
    SET SWITCHES=-d

IF NOT "%RECOMPILE_SLICE%" == "0"^
    SET SWITCHES=%SWITCHES% -s

start /B %PATH_TO_PYTHON% central_server\Echelon.py %SWITCHES%

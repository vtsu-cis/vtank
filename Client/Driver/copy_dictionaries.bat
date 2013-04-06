@echo off

SET SRC_DIR=..\..\Map_Editor\
IF NOT EXIST %SRC_DIR% SET SRC_DIR=..\..\..\..\..\Map_Editor\

SET TARGET_DIR=%1
IF "%TARGET_DIR" == "" SET TARGET_DIR=.

echo Copying dictionaries from %SRC_DIR% to %TARGET_DIR% ...

copy %SRC_DIR%\tile_dictionary.txt %TARGET_DIR%
copy %SRC_DIR%\object_dictionary.txt %TARGET_DIR%
copy %SRC_DIR%\event_dictionary.txt %TARGET_DIR%

@echo off
set target_dir=%1

copy "..\..\..\..\Driver\*.xml" "%target_dir%"
del "%target_dir%"\Ranks.xml

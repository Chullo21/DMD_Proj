@echo off
setlocal enabledelayedexpansion

set "excel_file=%1"

set "output_folder=%2"

for %%f in ("%excel_file%") do set "filename=%%~nf"

"C:\Program Files\Microsoft Office\root\Office16\EXCEL.EXE" /r /e /t "%excel_file%" "%output_folder%\%filename%.pdf"

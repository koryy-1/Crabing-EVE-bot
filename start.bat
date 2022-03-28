@echo off

::dir 123.txt /T:W
::ren read-memory-64-bit.exe destroyer.exe

:start
launch.py
%cd%\bin\Debug\netcoreapp3.1\EVE-Bot.exe
if %ERRORLEVEL%==10 (
	echo exit from program
	pause
	exit /b 0
)
timeout 3
goto :start
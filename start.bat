@echo off

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
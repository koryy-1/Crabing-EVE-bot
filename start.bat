@echo off

:start
launch.py
if %ERRORLEVEL%==10 (
	echo bot-launch.py ne smog zapustit urpy
	pause
	exit /b 0
)

%cd%\bin\Debug\netcoreapp3.1\EVE-Bot.exe
if %ERRORLEVEL%==10 (
	echo exit from program
	pause
	exit /b 0
)
timeout 3
goto :start
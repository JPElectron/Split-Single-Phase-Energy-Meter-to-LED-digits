@ECHO OFF

setlocal enableextensions disabledelayedexpansion

set DeviceIP=192.168.1.94

cd\
cd "C:\CountLED"

curl.exe -s -o "lastvalues.txt" -m 2 "http://%DeviceIP%/lastvalues" 

(for /f tokens^=1^-9delims^=^,^" %%a in (lastvalues.txt) Do (
    echo(%%a:%%b:%%c:%%d:%%e:%%f:%%g:%%h:%%i
        )
)>lastvalues-replc.txt

for /f "tokens=10 delims=:" %%o in (lastvalues-replc.txt) Do (
    echo %%o
    call :Round %%o Out
)

echo %Out%> C:\CountLED\count.dat

C:\CountLED\count.exe

EXIT

:Round
setlocal
for /f "tokens=1,2 delims=." %%A in ("%~1") do set "X=%%~A" & set "Y=%%~B0"
if %Y:~0,1% geq 5 set /a "X+=1"
endlocal & set "%~2=%X%"
exit /b 0

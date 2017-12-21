@echo off
set taskname=WebSiteUpdateConsole.exe
net stop MZ.Jobs
ping 127.1 -n 10


echo.%taskname%未运行，

echo.
echo.
goto :loop2
 

:loop2
for /f %%a in ('tasklist.exe /FI "IMAGENAME eq %taskname%" /FI "STATUS eq RUNNING" /FO TABLE /NH^|find.exe /i "没有"') do (
echo.%taskname%已结束，
echo.
echo.
goto :loop3
)
ping 127.1 -n 2 >nul 2>nul
goto :loop2

:loop3
ping 127.1 -n 10
net start MZ.Jobs

 
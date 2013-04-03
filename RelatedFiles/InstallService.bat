
sc create WatchManMDN binpath= "D:\oDesk\FailedServiceRestart\FailedServiceRestart\bin\Debug\FailedServiceRestart.exe" displayname= "WatchManService" depend= Tcpip start= auto

REM net stop WatchManMDN
net start WatchManMDN

pause
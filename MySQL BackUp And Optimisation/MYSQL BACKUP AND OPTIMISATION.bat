@echo off
:: PURPOSE backs up & optimises specified MySQL database to a chosen storage filepath and logs the action as a .txt file
:: CREATED BY Luis Arenas
:: CREATED ON 20/08/2018
::--------------------------------------------------

::SET DATE AND TIMESTAMP
set year=%DATE:~-4,4%
set month=%DATE:~-7,2%
set day=%DATE:~-10,2%

set timestamp=%TIME:~-11,2%-%TIME:~-8,2%-%TIME:~-5,2%

::FORMAT YYYY-MM-DD
set backupdate=%year%-%month%-%day%

:: NUMBER OF DAYS TO RETAIN BACKUPS FOR, make sure it has '-' sign:
set limit=<LIMIT-IN-DAYS>

::-------------------------------------------------


::for multiple databases, uncomment:
set databaselist="<DB NAMES>"


::-------------------------------------------------
::PERMISSIONS:
set USERNAME=<USERNAME>
set PASSWORD=<PASSWORD>
::-------------------------------------------------

:: FILE LOCATIONS:
::Where to store back up files:
set dumpdirectorypath=C:\<PATH-TO-BACKUP-FILES>\

::Where to store log files:
set logpath="C:\<PATH-TO>\Logs\backup_log_%backupdate%_%timestamp%.txt"

::MySQL Server Version number:
set versionnumber=5.1


::-------------------------------------------------

:: DEBUG CODE: 
::echo %DATE%
::echo %backupdate%

::echo %TIME%
::echo %timestamp%

::-------------------------------------------------


::CREATE LOG
echo BACKED UP AND OPTIMISED ON: %backupdate% at %timestamp% > %logpath%
echo ------------------------------------ >> %logpath%


::DELETE FILES OLDER THAN 30 DAYS
::echo Deleting files...
set daylimit=-%limit%
forfiles -p %dumpdirectorypath% -s -d %daylimit% -C "cmd /c del @path"
::echo Files deleted...
echo Deleted files %daylimit% days or more before %backupdate% at %timestamp% >> %logpath%
echo ------------------------------------ >> %logpath%
echo. >> %logpath%



::CREATE BACKUP FILES USING MYSQLDUMP

"C:\Program Files\MySQL\MySQL Server %versionnumber%\bin\mysqldump.exe" %dbname% --user=%USERNAME% --password=%PASSWORD% --result-file=%dumpfilepath%
::echo "Finished backing up"
for %%a in (%databaselist%) DO
(
	::Dump file name
	set dumpname=%%a_%backupdate%-%timestamp%.sql
	set dumpfilepath=%dumpdirectorypath%%dumpname%
	
	"C:\Program Files\MySQL\MySQL Server %versionnumber%\bin\mysqldump.exe" %%a --user=%USERNAME% --password=%PASSWORD% --result-file=%dumpfilepath%
	echo BACKUP >> %logpath%
	echo Backed up '%%a' database >> %logpath%
	echo to filepath: >> %logpath%
	echo %dumpfilepath% >> %logpath%
	echo on %backupdate% at %timestamp% >> %logpath%
	echo ------------------------------------ >> %logpath%
	echo. >> %logpath%
)
::OPTIMISE CURRENT DATABASE USING MYSQLCHECK
::echo "Optimising..."
echo OPIMISE: >> %logpath%
"C:\Program Files\MySQL\MySQL Server %versionnumber%\bin\mysqlcheck" -u%USERNAME% --password=%PASSWORD% -o %databaselist%
echo Optimised '%databaselist%' database at %backupdate% at %timestamp% >> %logpath%
echo ------------------------------------ >> %logpath%
::echo "Finished optimisation"


::TO DEBUG, uncomment:
::pause
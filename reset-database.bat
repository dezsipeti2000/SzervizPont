@echo off
cd /d "%~dp0"

if exist "Database\szervizpont.db" del "Database\szervizpont.db"
if exist "Database\szervizpont.db-shm" del "Database\szervizpont.db-shm"
if exist "Database\szervizpont.db-wal" del "Database\szervizpont.db-wal"

echo Az adatbazis torolve lett.
echo A kovetkezo inditaskor a program ujra letrehozza a Database mappaban.
pause

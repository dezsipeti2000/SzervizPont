@echo off
cd /d "%~dp0Backend"

where dotnet >nul 2>nul
if errorlevel 1 (
    echo HIBA: A .NET SDK nem talalhato a gepen.
    echo Telepitsd a .NET 8 SDK-t, majd inditsd ujra ezt a fajlt.
    pause
    exit /b 1
)

echo NuGet-csomagok visszaallitasa...
dotnet restore
if errorlevel 1 goto :error

echo A SzervizPont inditasa...
dotnet run
if errorlevel 1 goto :error
exit /b 0

:error
echo.
echo A muvelet hibaval leallt. Olvasd el a fenti hiba-uzenetet.
pause
exit /b 1

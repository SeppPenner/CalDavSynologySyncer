cd src\CalDavSynologySyncer
dotnet restore -r linux-arm64
dotnet publish -c Release --output publish/ -r linux-arm64 --no-self-contained
@ECHO.Build successful. Press any key to exit.
pause
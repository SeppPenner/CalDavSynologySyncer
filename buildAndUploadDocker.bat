cd src\CalDavSynologySyncer
dotnet publish -c Release --output publish/ -r linux-x64 --no-self-contained
docker build --tag sepppenner/caldavsynologysyncer:1.0.0 -f Dockerfile .
docker login -u sepppenner -p "%DOCKERHUB_CLI_TOKEN%"
docker push sepppenner/caldavsynologysyncer:1.0.0
@ECHO.Build successful. Press any key to exit.
pause
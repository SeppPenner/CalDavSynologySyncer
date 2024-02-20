cd src\CalDavSynologySyncer
dotnet publish -c Release --output publish/ -r linux-arm --no-self-contained
docker build --tag sepppenner/caldavsynologysyncer-arm:1.0.0 -f Dockerfile.armv7 .
docker login -u sepppenner -p "%DOCKERHUB_CLI_TOKEN%"
docker push sepppenner/caldavsynologysyncer-arm:1.0.0
@ECHO.Build successful. Press any key to exit.
pause
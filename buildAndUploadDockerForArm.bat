cd src\CalDavSynologySyncer
dotnet publish -c Release --output publish/ -r linux-arm --no-self-contained
docker build --platform linux/arm/v7 --provenance=false --tag sepppenner/caldavsynologysyncer-arm:1.1.2 -f Dockerfile.armv7 .
@docker login -u sepppenner -p "%DOCKERHUB_CLI_TOKEN%"
docker push sepppenner/caldavsynologysyncer-arm:1.1.2
@ECHO.Build successful. Press any key to exit.
pause
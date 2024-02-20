## Basic usage

### JSON configuration (Adjust this to your needs)
```json
{
    "AllowedHosts": "*",
    "CalDavSynologySyncer": {
        "ServiceDelayInMilliSeconds": 30000, // Delay how often the service syncs
        "HeartbeatIntervalInMilliSeconds": 30000, // Delay how often the service logs a heartbeat message
        "CalendarUrls": [
            "https://ical.de/ical1123" // A list of source calendar urls (e.g. from the web)
        ],
        "SynologyCalendarUrl": "http://192.168.2.2/caldav.php/user/someid", // Your URL to your Synology calendar
        "SynologyCalendarId": "/caldav.php/user/uniqueid/", // The id of the Synology calendar
        "SynologyUserName": "test", // The Synology user name
        "SynologyPassword": "password", // The Synology password
        "TelegramBotToken": "111111111:AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", // A bot token from Telegram to send error logs to a chat, follow https://core.telegram.org/bots/api
        "TelegramChatId": "2222222" // The chat id on Telegram
    }
}
```


### Run this project in Docker from the command line (Examples for Powershell, but should work in other shells as well):

1. Change the directory
    ```bash
    cd ..\src\CalDavSynologySyncer
    ```

2. Publish the project
    ```bash
    dotnet publish -c Release --output publish/
    ```

3. Build the docker file:
    * `dockerhubuser` is a placeholder for your docker hub username, if you want to build locally, just name the container `caldavsynologysyncer`
    * `1.0.0` is an example version tag, use it as you like
    * `-f Dockerfile .` (Mind the `.`) is used to specify the dockerfile to use

    ```bash
    docker build --tag dockerhubuser/caldavsynologysyncer:1.0.0 -f Dockerfile .
    ```

4. Push the project to docker hub (If you like)
    * `dockerhubuser` is a placeholder for your docker hub username, if you want to build locally, just name the container `caldavsynologysyncer`
    * `1.0.0` is an example version tag, use it as you like

    ```bash
    docker push dockerhubuser/caldavsynologysyncer:1.0.0
    ```

5. Run the container:
    * `-d` runs the docker container detached (e.g. no logs shown on the console, is needed if run as service)
    * `--name="caldavsynologysyncer"` gives the container a certain name
    * `-v "/home/config.json:/app/appsettings.json"` sets the path to the external configuration file (In the example located under `/home/appsettings.json`) to the container internally
    
    ```bash
    docker run -d --name="caldavsynologysyncer" -v "/home/appsettings.json:/app/appsettings.json" --restart=always dockerhubuser/caldavsynologysyncer:1.0.0
    ```

6. Check the status of all containers running (Must be root)
    ```bash
    docker ps -a
    ```

7. Stop a container
    * `containerid` is the id of the container obtained from the `docker ps -a` command
    ```bash
    docker stop containerid
    ```

8. Remove a container
    * `containerid` is the id of the container obtained from the `docker ps -a` command
    ```bash
    docker rm containerid
    ```
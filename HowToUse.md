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

Run the program by either using the binaries from the publish folder or build the project on your own using Visual Studio.
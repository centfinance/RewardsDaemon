# RewardsDaemon
This daemon calculates rewards hourly, recording the rewards to a SQL Azure database. The [rewards dashboard project](https://github.com/centfinance/RewardsDashboard) in turn allows rewards to be queried as well as .csv and .json reward files generated.


# Pre-steps
This daemon uses the .NET 6 framework which can be installed from the [.NET Core website](https://dotnet.microsoft.com/en-us/download).

This project can be built using the .NET Core command line tools or popular .NET IDE's including [Visual Studio](https://visualstudio.microsoft.com/), [Visual Code IDE's](https://code.visualstudio.com/) or [Jet Brains Rider](https://www.jetbrains.com/rider/).

To confirm that you have .NET 6 installed, open a terminal and type the following.
```
dotnet --info
```

For development purposes you should store database connection details using the .NET Secret Manager tool.
If you were creating a new project you would initialise the secret manager for the project using the following.

```
dotnet user-secrets init
```

However, we have already initialised the secret manager for this project and so you should create a secrets file in the following location.

Windows
```
%APPDATA%\Microsoft\UserSecrets\cf01fb74-ee77-4086-a5f5-9a1a1fef3487\secrets.json
```

Linux/Mac
```
~/.microsoft/usersecrets/cf01fb74-ee77-4086-a5f5-9a1a1fef3487/secrets.json
```

Within this file you will need to store the following secrets for your target database.
```
{
  "RewardsDatabase:UserID": "<username>",
  "RewardsDatabase:DataSource": "<server URL>",
  "RewardsDatabase:Password": "<password>",
  "RewardsDatabase:InitialCatalog": "<database name?"
}
```

# Build and deploy
The daemon can be built and run in Windows, Mac and Linux environments. The only requirement is that .NET 6 be installed.

## Build
```
dotnet build
```

## Publish a single file executable
```
dotnet publish -r win10-x64 -c Release --self-contained=false /p:PublishSingleFile=true
```
You may substitute different target environments as required.

call dotnet publish AutoDarkModeApp\AutoDarkModeApp.csproj /p:PublishProfile=$(SolutionDir)AutoDarkModeApp\Properties\PublishProfiles\AppPublish.pubxml
call dotnet publish AutoDarkModeSvc\AutoDarkModeSvc.csproj /p:PublishProfile=$(SolutionDir)\AutoDarkModeSvc\Properties\PublishProfiles\ServicePublish.pubxml
call dotnet publish AutoDarkModeConfig\AutoDarkModeConfig.csproj /p:PublishProfile=$(SolutionDir)\AutoDarkModeConfig\Properties\PublishProfiles\ConfigPublish.pubxml
dotnet publish -c Release -r linux-musl-x64 --self-contained true /p:PublishTrimmed=true -o ./publish
pause
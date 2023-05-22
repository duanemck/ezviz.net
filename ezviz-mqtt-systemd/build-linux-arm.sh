#dotnet publish -c Release -r linux-arm64 --self-contained /p:DebugType=None /p:DebugSymbols=false
dotnet publish -c Release /p:DebugType=None /p:DebugSymbols=false /p:PublishSingleFile=false

$pwd=pwd
$path=$pwd.Path

echo "Installing ezviz-mqtt service"
echo "Creating config dir"

$configPath="$env:APPDATA\ezviz-mqtt"
New-Item -ItemType Directory -Force -Path $configPath
Copy-Item .\appsettings.json -Destination $configPath

echo "Please set custom config in $configPath"
echo "Creating systemd service"

Start-Process sc.exe -verb runAs  -Args 'create "ezviz mqtt" binpath="$path\ezviz-mqtt-windows-service.exe"'

echo 'Service installed, run "sc.exe start `"ezviz mqtt`" to start once settings have been updated, or use the Services GUI'
echo "NOTE: the current folder needs to remain as it contains the service executable"

#!/bin/bash
echo "Installing ezviz-mqtt service"
echo "Creating config dir"
workingDir=`pwd`
mkdir -p /etc/ezviz-mqtt
cp appsettings.json /etc/ezviz-mqtt
echo "Please set custom config in /etc/ezviz-mqtt/appsettings.json"

echo "Creating systemd service"
ln -s $workingDir/ezviz-mqtt-systemd /usr/sbin/ezviz-mqtt

sed -i "s|__WORKING_DIR__|$workingDir|g" ezviz-mqtt.service
cp ezviz-mqtt.service /etc/systemd/system
systemctl daemon-reload

echo "Service installed, run 'service ezviz-mqtt start' to start once settings have been updated"
echo "NOTE: the current folder needs to remain as it contains the service executable"

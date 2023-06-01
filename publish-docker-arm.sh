#!/bin/sh

# $1 == version

docker image tag duanemck/ezviz-mqtt-arm:latest duanemck/ezviz-mqtt-arm:$1
docker image push duanemck/ezviz-mqtt-arm:$1


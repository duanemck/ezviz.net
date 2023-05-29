#!/bin/sh

# $1 == version

docker image tag duanemck/ezviz-mqtt-arm:latest duanemck/ezviz-mqtt-arm:$1
docker image push --all-tags duanemck/ezviz-mqtt-arm


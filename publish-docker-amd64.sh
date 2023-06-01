#!/bin/sh

# $1 == tag

docker image tag duanemck/ezviz-mqtt:latest duanemck/ezviz-mqtt:$1
docker image push duanemck/ezviz-mqtt:$1


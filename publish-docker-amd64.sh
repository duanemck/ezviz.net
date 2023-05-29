#!/bin/sh

# $1 == version

docker image tag duanemck/ezviz-mqtt:latest duanemck/ezviz-mqtt:$1
docker image push --all-tags duanemck/ezviz-mqtt


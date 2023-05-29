#!/bin/sh

# $1 == version

./publish-docker-amd64.sh $1
./publish-docker-arm.sh $1

#!/bin/sh

# $1 == tag

./publish-docker-amd64.sh $1
./publish-docker-arm.sh $1

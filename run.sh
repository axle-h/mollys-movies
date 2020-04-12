#!/bin/bash
set -e

uname=`uname -s`
if [ "$uname" == "Linux" ]; then
  export DOCKER_HOST_IP=`ip -4 addr show docker0 | grep -Po 'inet \K[\d.]+'`
else
  # Windows and Macos
  export DOCKER_HOST_IP="docker.host.internal"
fi

function wait_for_mysql() {
  while ! docker-compose exec mysql mysqladmin -h 127.0.0.1 -u root -proot-password ping &> /dev/null
  do
    echo 'waiting for mysql'
    sleep 1
  done
}

docker-compose up -d mysql

wait_for_mysql

ASPNETCORE_ENVIRONMENT=Migration docker-compose up api
docker-compose up -d api

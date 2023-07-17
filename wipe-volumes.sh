#!/bin/bash
docker ps -aq | xargs docker rm -f
docker volume ls -q | xargs docker volume rm
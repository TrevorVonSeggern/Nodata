#!/bin/bash

docker pull microsoft/mssql-server-linux

# Stop previous instances
echo clean running instances

docker stop sql
docker rm sql

echo
echo stopped

# Start container

echo starting sql on port 14433 password is YourStrong!Passw0rd

docker run -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD=YourStrong!Passw0rd' \
  -p 1433:1433 --name sql \
  -d --net=host microsoft/mssql-server-linux

docker ps

# sqlcmd -S localhost -U SA -P YourStrong\!Passw0rd
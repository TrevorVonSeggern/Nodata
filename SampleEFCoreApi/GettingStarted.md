Steps to re-produce:

1. mkdir SampleEFCoreApi
2. cd SampleEFCoreApi
3. dotnet new webapi
4. dotnet add

docker run -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD=BadPasswords!1!1' -p 1433:1433 -d microsoft/mssql-server-linux
docker exec -it 2a /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "BadPasswords!1!1"

create Models
create database classes
create data context
configure data context
Add automapper
Add autofac


3. mkdir Models
 
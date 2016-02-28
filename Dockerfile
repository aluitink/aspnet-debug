FROM microsoft/aspnet:1.0.0-rc1-final

RUN apt-get update -qq && apt-get install -qqy supervisor

COPY .nuget /opt/aspnet-debug/.nuget
COPY global.json /opt/aspnet-debug/global.json
COPY wrap /opt/aspnet-debug/wrap

COPY src/aspnet-debug.Shared /opt/aspnet-debug/src/aspnet-debug.Shared
RUN ["mono", "/opt/aspnet-debug/.nuget/NuGet.exe", "restore", "/opt/aspnet-debug/src/aspnet-debug.Shared/packages.config", "-OutputDirectory", "/opt/aspnet-debug/src/packages"]

WORKDIR /opt/aspnet-debug/src/aspnet-debug.Shared
RUN ["xbuild", "/p:Configuration=Debug", "aspnet-debug.Shared.csproj"]

COPY src/aspnet-debug.Server/project.json /opt/aspnet-debug/src/aspnet-debug.Server/

WORKDIR /opt/aspnet-debug/src/aspnet-debug.Server
RUN ["dnu", "restore"]

COPY src/aspnet-debug.Server /opt/aspnet-debug/src/aspnet-debug.Server

# Supervisor
COPY supervisord.conf /etc/supervisor/conf.d/supervisord.conf

EXPOSE 13001

#CMD ["/usr/bin/supervisord"]
CMD ["dnx", "-p", "project.json", "--configuration", "Debug", "aspnet_debug.Server"]

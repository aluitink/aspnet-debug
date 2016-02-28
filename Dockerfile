FROM microsoft/aspnet:1.0.0-rc1-final

COPY src/aspnet-debug.Server/project.json /opt/aspnet-debug/aspnet-debug.Server/

WORKDIR /opt/aspnet-debug/aspnet-debug.Server
RUN ["dnu", "restore"]

COPY src/aspnet-debug.Server /opt/aspnet-debug/aspnet-debug.Server

# Supervisor
COPY supervisord.conf /etc/supervisor/conf.d/supervisord.conf

EXPOSE 13001

CMD ["/usr/bin/supervisord"]
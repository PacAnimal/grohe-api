# base
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

# build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# su-exec
ADD su-exec.c /usr/local/src/
RUN set -eux                                                            &&  \
    export TERM=dumb DEBIAN_FRONTEND=noninteractive                     &&  \
    apt-get update                                                      &&  \
    apt-get install -y --no-install-recommends gcc libc-dev             &&  \
    cd /usr/local/src                                                   &&  \
    gcc -Wall su-exec.c -o su-exec                                      &&  \
    chown root:root su-exec                                             &&  \
    chmod 0755 su-exec

# build
COPY ["dotnet/Application/Application.csproj", "Application/"]
RUN dotnet restore "Application/Application.csproj"
COPY dotnet/ .
WORKDIR "/src/Application"
RUN dotnet build "Application.csproj" -c Release -o /app/build

# publish
FROM build AS publish
RUN dotnet publish "Application.csproj" -c Release -o /app/publish --self-contained -r linux-x64

# build test
FROM base AS test
WORKDIR /app
COPY --from=publish /app/publish .
RUN ./Application buildtest

# runtime image
FROM base AS release
WORKDIR /app
COPY --from=publish /app/publish .

# things we're gonna need, and things we want
RUN set -eux                                                            &&  \
    export TERM=dumb DEBIAN_FRONTEND=noninteractive                     &&  \
    apt-get update                                                      &&  \
    apt-get install -y --no-install-recommends curl jq procps net-tools

# su-exec
COPY --from=build /usr/local/src/su-exec /usr/local/sbin/

# scripts
ADD scripts/* /usr/local/bin/

# ports
EXPOSE 5000

# health
HEALTHCHECK --start-period=60s --retries=1 --interval=10s --timeout=300s      \
    CMD [ "/usr/local/bin/healthcheck" ]

# default env
ENV \
    GROHE_USER="" \
    GROHE_PASS="" \
    API_USER="" \
    API_PASS="" \
    LOCAL_PORT=""

# run!
USER root
ENTRYPOINT ["/bin/bash","-c"]
CMD ["exec /usr/local/bin/launch"]



# vim: tabstop=4:softtabstop=4:shiftwidth=4:expandtab

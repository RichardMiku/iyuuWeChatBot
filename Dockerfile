#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER root
WORKDIR /app
EXPOSE 8080

# 定义环境变量
ENV LOGIN_API_TOKEN=''
ENV SEND_API_URL=''
ENV ZHSS_API_URL='http://192.168.1.188:8090'

# 安装 net-tools 包以使用 ifconfig以及安装curl
RUN apt-get update && apt-get install -y net-tools && apt -y install curl

#USER app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ricwxbot.csproj", "."]
RUN dotnet restore "./ricwxbot.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "./ricwxbot.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ricwxbot.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ricwxbot.dll"]
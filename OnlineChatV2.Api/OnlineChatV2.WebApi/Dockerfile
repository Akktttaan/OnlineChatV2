FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["OnlineChatV2.WebApi/OnlineChatV2.WebApi.csproj", "OnlineChatV2.WebApi/"]
RUN dotnet restore "OnlineChatV2.WebApi/OnlineChatV2.WebApi.csproj"
COPY . .
WORKDIR "/src/OnlineChatV2.WebApi"
RUN dotnet build "OnlineChatV2.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OnlineChatV2.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OnlineChatV2.WebApi.dll"]

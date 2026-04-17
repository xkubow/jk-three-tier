FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY . .

RUN dotnet restore backend/Api/JK.Messaging.CZ/JK.Messaging.CZ.csproj
RUN dotnet publish backend/Api/JK.Messaging.CZ/JK.Messaging.CZ.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
EXPOSE 8081

ENTRYPOINT ["dotnet", "JK.Messaging.CZ.dll"]

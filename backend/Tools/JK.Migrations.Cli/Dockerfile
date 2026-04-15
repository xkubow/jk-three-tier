FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN ls

RUN dotnet build backend/Tools/JK.Migrations.Cli/JK.Migrations.Cli.csproj -c Release
RUN dotnet build backend/Modules/Configuration/JK.Configuration.Database/JK.Configuration.Database.csproj -c Release
RUN dotnet build backend/Modules/Order/JK.Order.Database/JK.Order.Database.csproj -c Release
RUN dotnet build backend/Modules/Messaging/JK.Messaging.Database/JK.Messaging.Database.csproj -c Release

FROM mcr.microsoft.com/dotnet/sdk:9.0
WORKDIR /app

COPY --from=build /src/backend/Tools/JK.Migrations.Cli/bin/Release/net9.0/ ./cli/
COPY --from=build /src/backend/Modules/Configuration/JK.Configuration.Database/bin/Release/net9.0/ ./modules/configuration/
COPY --from=build /src/backend/Modules/Order/JK.Order.Database/bin/Release/net9.0/ ./modules/order/
COPY --from=build /src/backend/Modules/Messaging/JK.Messaging.Database/bin/Release/net9.0/ ./modules/messaging/
COPY --from=build /src/scripts/run-all-migrations-incluster.sh ./run-all-migrations.sh
RUN sed -i 's/\r$//' /app/run-all-migrations.sh && chmod +x /app/run-all-migrations.sh

RUN chmod +x /app/run-all-migrations.sh

ENTRYPOINT ["/app/run-all-migrations.sh"]

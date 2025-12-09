# ----------------------
# Stage 1 — Build
# ----------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY MatchMaking.Worker.csproj .
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /app


# ----------------------
# Stage 2 — Runtime
# ----------------------
FROM mcr.microsoft.com/dotnet/runtime:9.0

WORKDIR /app

COPY --from=build /app .

COPY Configs ./Configs

ENTRYPOINT ["dotnet", "MatchMaking.Worker.dll"]
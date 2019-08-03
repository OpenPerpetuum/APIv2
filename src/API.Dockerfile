FROM microsoft/dotnet:sdk AS build
WORKDIR /app/api
COPY / ./
RUN dotnet restore

RUN dotnet publish -c Debug -o /app/out
RUN rm -rf /app/api

# Build runtime
FROM microsoft/dotnet:aspnetcore-runtime
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "OpenPerpetuum.Api.dll", "--Environment=Development"]
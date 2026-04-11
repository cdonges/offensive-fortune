FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /App

# Copy project file and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy the rest of the code
COPY . ./
# Build and publish a release
RUN dotnet publish -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /App
COPY --from=build /App/out .
#COPY off/ /usr/share/games/fortunes/off/
ENTRYPOINT ["dotnet", "fortune_web.dll"]
EXPOSE 6724

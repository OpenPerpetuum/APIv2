# APIv2
Re-Engineering of the public API using dotnet core technology

The system is built with Docker in mind. However, it is possible to run and debug the system natively on \*nix or Windows.

# Installation (\*nix)

## Pre-requisites
* dotnet core SDK 2.1.401 (Required)
* dotnet code Runtime 2.1 (Required)
* docker (Recommended)
* VS Code (Recommended)

## Installation steps (command line only)
* git clone https://github.com/OPenPerpetuum/APIv2.git && cd APIv2/src
* dotnet restore *optionally provide the "OpenPerpetuum.Api/OpenPerpetuum.Api.csproj" argument*
* dotnet build OpenPerpetuum.Api/OpenPerpetuum.Api.csproj -c [Debug|Release] -o ./app
* dotnet public OpenPerpetuum.Api/OpenPerpetuum.Api.csproj -c [Debug|Release] -o ./app
* <To start the app> cd app && dotnet OpenPerpetuum.Api.dll

# Installation (Windows)

## Pre-requisites 
* dotnet core SDK 2.1.401 (Required)
* dotnet code Runtime 2.1 (Required)
* docker (Recommended)
* Visual Studio 2017 (Recommended)

## Installation Steps
* Open PowerShell to parent folder
* git clone https://github.com/OPenPerpetuum/APIv2.git && cd APIv2/src
* dotnet restore
* Open Solution file with Visual Studio 2017

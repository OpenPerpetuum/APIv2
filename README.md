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
* dotnet publish OpenPerpetuum.Api/OpenPerpetuum.Api.csproj -c [Debug|Release] -o ./app
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


# Extending Functionality
Adding new functionality is made simple thanks to autoconfiguration.

To create a new controller (resource);
1. Right click the "Controllers" folder
2. Add new class...
3. Name the new class as your route (i.e. api/Corporation would be CorporationController)
4. Above the class add the metadata `[Route("api/[controller]")]` - *Note: I'm going to simplify the route later on by making the default sub path "api"*

To create a new action (sub-resource/query)
1. Create a `public async Task<IActionResult> ActionNameAsync(*params here*)` method
2. Above the method, attach the relevent metadata. This can include route information. e.g. `[HttpGet({corporationId:int}/relations)]` would equate to `HTTP GET api/ControllerName/47/relations`
3. Return an ActionResult. There are multiple helper methods for this, the main ones being `Ok(<object>)`, `NoContent()`, `Created(<params>)`, `NotFound()`.
4. I *highly* recommend to async away onto a non-async dedicated method where possible, for readability. Basic example:
```
[HttpGet({id:int}), Authorize]
public async Task<IActionResult> GetPlayerInfoAsync(int id)
{
    return await Task.Run(() => GetPlayerInfo(id));
}

private IActionResult GetPlayerInfo(int playerId)
{
    PlayerInfo playerInfoResult = QueryProcessor.Process(new GetPlayerInfoQuery { PlayerId = playerId });
    if (playerInfoResult == null)
        return NotFound();
    else if (playerId == Identity.User.UserId)
        return Ok(playerInfoResult.ExtendedInfo);
    else
        return Ok(playerInfoResult.BasicInfo);
}
```

# CQRS - Queries and Commands
CQRS (Command Query Responsibility Segregation) is a pattern to ensure that units of operation have a defined responsibility and there is a clear separation between operations that modify the data-set vs those that don't.

## Queries
An example query would be "Fetch all users' information". This query would go to the database, execute the relevent database query, and return the result set. It would not, for example, update the "last accessed" column on the user table.

Queries can be refined down depending on both database level availability and/or available data within code. For example, if we wanted to query the database for all characters that contain a certain word, it may be more efficient for the database to return *everyone* and then let the code perform the analysis.

## Commands
Commands are units of operation that execute an action against the data-set, generally with the idea of changing it in some fashion (Create, Update, Delete). A command returns no data, but **should** report errors in operation that cannot be recovered by the command itself (typically using the exception framework).

It is important to follow the rule of "no return data" where a command is concerned. A common mistake is to return generated identifiers for objects inserted by a command. This fallacy leads to connected and dependent design withing the application for components that should not be linked.<br />
When creating new objects with identifiers, ensure that the identifier is created **before** passing it into the command. This ensures that all methods that may require access to the identifier have it and do not need to wait for it to be inserted (check your dependencies!!). It also allows you to reliably return that identifier to the client. If you wish to query the result of a command, create and call a seperate query for that purpose.

## Abstraction Level
There are some arguments for and against creating atomic commands/queries. How far down the rabbit hole do you go? As a general rule, anything that can be re-used should be its own unique command/query. It is also acceptible to composite commands and queries with additional queries.

**IMPORTANT**: When creating composite queries, *do not* include commands. It is vitally important that queries do not update the data-set state. Queries *must not* have the possibility of doing so and any requirement to call a command from a query should trigger you to re-think your local design.<br />
On the flip-side, however, it is perfectly acceptible for Commands to utilise Queries, and other Commands. Compositing Commands within Commands I would *strongly* advise against. My main reasoning for this is that DNX does not support transactions as of yet. Compositing commands may give a false sense of security that those items will either all succeed, or all fail, and error handling may become excessive.

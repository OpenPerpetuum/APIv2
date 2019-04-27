if object_id('API.FindZoneByZoneId', 'P') is not null
	drop procedure API.FindZoneByZoneId;
go

create procedure API.FindZoneByZoneId
	@ZoneId int
as

select
	[id] as Id,
	[x] as [X],
	[y] as [Y],
	[name] as [Name],
	[description] as [Description],
	[note] as [Note],
	[fertility] as [Fertility],
	[zoneplugin] as [ZonePlugin],
	[zoneip] as [ZoneIp],
	[zoneport] as [ZonePort],
	[isinstance] as [IsInstance],
	[enabled] as [Enabled],
	[spawnid] as [SpawnId],
	[plantruleset] as [PlatRuleset],
	[protected] as [Protected],
	[raceid] as [RaceId],
	[width] as [Width],
	[height] as [Height],
	[terraformable] as [Terraformable],
	[zonetype] as [ZoneType],
	[sparkcost] as [SparkCost],
	[maxdockingbase] as [MaxDockingBase],
	[sleeping] as [Sleeping],
	[plantaltitudescale] as [PlantAltitudeScale],
	[host] as [Host],
	[active] as [Active]
from zones
where id = @ZoneId;
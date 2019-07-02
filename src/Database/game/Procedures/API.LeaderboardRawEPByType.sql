----------------------------------------------------
--Query for Top EP earners by some time period and Activity Type
--Activity Type Enum: https://github.com/OpenPerpetuum/PerpetuumServer/blob/Development/src/Perpetuum/EpForActivityType.cs
--Undefined = 0
--Gathering = 1
--Mission = 2
--Production = 3
--Artifact = 4
--Intrusion = 5
--Npc = 6
--Last updated: 2019/07/02
----------------------------------------------------

IF OBJECT_ID('API.LeaderboardEPByType', 'P') IS NOT NULL
	DROP PROCEDURE API.LeaderboardEPByType;
GO

CREATE procedure API.LeaderboardEPByType
	@startTime DATETIME,
	@endTime DATETIME,
	@activityType INT,
	@pageNum INT,
	@pageSize INT
as

SELECT el.characterid, c.nick, SUM(rawpoints) as total
FROM [perpetuumsa].[dbo].[epforactivitylog]  el
JOIN characters c on el.characterid = c.characterID
WHERE eventtime > @startTime AND eventtime < @endTime
AND @activityType = el.epforactivitytype
GROUP BY el.characterid, nick
ORDER BY total DESC
OFFSET @pageSize * (@pageNum - 1) ROWS
FETCH NEXT @pageSize ROWS ONLY;

GO

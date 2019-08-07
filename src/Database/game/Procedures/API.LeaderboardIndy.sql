----------------------------------------------------
--Production leaderboard - who produced the most type of item for some time range
--Good general CategoryFlag Names:
--cf_robots, cf_robot_equipment, cf_ammo
--Others work and allow for more specificity, curate as necessary.
--Last updated: 2019/07/02
----------------------------------------------------

IF OBJECT_ID('API.LeaderboardIndy', 'P') IS NOT NULL
	DROP PROCEDURE API.LeaderboardIndy;
GO

CREATE procedure API.LeaderboardIndy
	@startTime DATETIME,
	@endTime DATETIME,
	@cfName varchar(50),
	@pageNum INT,
	@pageSize INT
as

DECLARE @cfFlag BIGINT;
SET @cfFlag = (SELECT TOP 1 value FROM [perpetuumsa].[dbo].[categoryFlags] WHERE name = @cfName);

SELECT prod.characterid, c.nick, SUM(amount) as total
FROM [perpetuumsa].[dbo].[productionlog]  prod
JOIN characters c on prod.characterid = c.characterID
WHERE productiontime BETWEEN @startTime AND @endTime
AND definition in (SELECT definition from entitydefaults where (categoryflags & CAST(dbo.GetCFMask(@cfFlag)as BIGINT) = @cfFlag))
GROUP BY prod.characterid, nick
ORDER BY total DESC
OFFSET @pageSize * (@pageNum - 1) ROWS
FETCH NEXT @pageSize ROWS ONLY;

GO


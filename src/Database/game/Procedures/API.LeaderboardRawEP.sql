USE [perpetuumsa]
GO

----------------------------------------------------
--Query for Top EP earners by some time period
--Last updated: 2019/07/02
----------------------------------------------------


IF OBJECT_ID('API.LeaderboardRawEP', 'P') IS NOT NULL
	DROP PROCEDURE API.LeaderboardRawEP;
GO

CREATE procedure API.LeaderboardRawEP
	@startTime DATETIME,
	@endTime DATETIME,
	@pageNum INT,
	@pageSize INT
as

SELECT el.characterid, c.nick, SUM(rawpoints) as total
FROM [perpetuumsa].[dbo].[epforactivitylog]  el
JOIN characters c on el.characterid = c.characterID
WHERE eventtime > @startTime AND eventtime < @endTime
GROUP BY el.characterid, nick
ORDER BY total DESC
OFFSET @pageSize * (@pageNum - 1) ROWS
FETCH NEXT @pageSize ROWS ONLY;

GO

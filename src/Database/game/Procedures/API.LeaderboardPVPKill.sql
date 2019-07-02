USE [perpetuumsa]
GO

----------------------------------------------------
--Most Dangerous Agents - PVP leaderboard - simple killcounts
--Last updated: 2019/07/02
----------------------------------------------------

IF OBJECT_ID('API.LeaderboardPVPKill', 'P') IS NOT NULL
	DROP PROCEDURE API.LeaderboardPVPKill;
GO

CREATE procedure API.LeaderboardPVPKill
	@startTime DATETIME,
	@endTime DATETIME,
	@pageNum INT,
	@pageSize INT
as

DECLARE @accLevel INT;
SET @accLevel = 2; --normal

SELECT highscore.characterid, c.nick, SUM(playerskilled) as total
FROM [perpetuumsa].[dbo].[characterhighscore]  highscore
JOIN characters c on highscore.characterid = c.characterID
WHERE DATE BETWEEN @startTime AND @endTime
AND (SELECT acclevel FROM accounts WHERE accountid=c.accountid)=@accLevel
GROUP BY highscore.characterid, nick
ORDER BY total DESC
OFFSET @pageSize * (@pageNum - 1) ROWS
FETCH NEXT @pageSize ROWS ONLY;

GO


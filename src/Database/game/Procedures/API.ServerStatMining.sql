----------------------------------------------------
--Server Statistics: Mining
--Displays ores gathered as aggregate statistics grouped by ore definition
--Last updated: 2019/07/02
----------------------------------------------------

IF OBJECT_ID('API.ServerStatMining', 'P') IS NOT NULL
	DROP PROCEDURE API.ServerStatMining;
GO

CREATE procedure API.ServerStatMining
	@startTime DATETIME,
	@endTime DATETIME,
	@pageNum INT,
	@pageSize INT
as

SELECT mine.definition, e.definitionname, SUM(amount) AS total
FROM [perpetuumsa].[dbo].[mininglog]  mine
JOIN entitydefaults e ON e.definition = mine.definition
WHERE eventtime BETWEEN @startTime AND @endTime
GROUP BY mine.definition, e.definitionname
ORDER BY total DESC
OFFSET @pageSize * (@pageNum - 1) ROWS
FETCH NEXT @pageSize ROWS ONLY;

GO

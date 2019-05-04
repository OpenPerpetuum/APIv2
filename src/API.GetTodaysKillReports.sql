if object_id('API.GetTodaysKillReports', 'P') is not null
	drop procedure API.GetTodaysKillReports;
go

create procedure API.GetTodaysKillReports
	@TodaysDate datetimeoffset,
	@Page int,
	@ResultsPerPage int
as

select
	id as KillId,
	[date] as [Date],
	[data] as GenxyData
from killreports
where [Date] < @TodaysDate
and [Date] > dateadd(day, -1, @TodaysDate)
order by [Date] desc
offset @ResultsPerPage * @Page rows fetch next @ResultsPerPage rows only;
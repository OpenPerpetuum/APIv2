if object_id('API.GetKillReportsNoFilter', 'P') is not null
	drop procedure API.GetKillReportsNoFilter;
go

create procedure API.GetKillReportsNoFilter
	@Page int,
	@ResultsPerPage int
as

select
	id as KillId,
	[date] as [Date],
	[data] as GenxyData
from killreports
order by [Date] desc
offset @ResultsPerPage * @Page rows fetch next @ResultsPerPage rows only;
if object_id('API.FindUserByAccountId', 'P') is not null
	drop procedure API.FindUserByAccountId;
go

create procedure API.FindUserByAccountId
	@AccountId int
as

select
	accountID as AccountId,
	email as Email,
	firstName as FirstName,
	lastName as LastName,
	accLevel as AccessLevel,
	lastLoggedIn as LastLoggedIn,
	creation as CreationDate,
	bantime as BanDate,
	banlength as BanLengthMinutes,
	emailConfirmed as EmailConfirmed,
	isactive as IsActive,
	totalMinsOnline as TotalMinutesOnline
from accounts
where accountID = @AccountId
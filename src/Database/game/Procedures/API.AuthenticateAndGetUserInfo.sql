if object_id('API.AuthenticateAndGetUserInfo', 'P') is not null
	drop procedure API.AuthenticateAndGetUserInfo;
go

create procedure API.AuthenticateAndGetUserInfo
	@Email varchar(50),
	@Password varchar(100)
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
where email = @email and [password] = @password;
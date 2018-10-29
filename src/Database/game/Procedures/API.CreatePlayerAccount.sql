if object_id('API.CreatePlayerAccount', 'P') is not null
	drop procedure API.CreatePlayerAccount;
go

create procedure API.CreatePlayerAccount
	@Email varchar(50),
	@HashedPassword varchar(100),
	@FirstName nvarchar(50) null,
	@LastName nvarchar(50) null,
	@DateOfBirth smalldatetime null,
	@CreationDate smalldatetime null
as

declare @newAccountTable as table
(
	NewAccountId int not null
);

insert into dbo.accounts
(
	email,
	[password],
	firstName,
	lastName,
	[state], -- 1
	accLevel, -- 2
	totalMinsOnline,
	creation,
	clientType,
	banlength,
	emailConfirmed,
	credit,
	isactive,
	resetcount,
	wasreset,
	payingcustomer
)
output inserted.accountID into @newAccountTable (NewAccountId)
values
(
	@Email,
	@HashedPassword,
	@FirstName,
	@LastName,
	1,
	2,
	0,
	@CreationDate,
	0,
	0,
	0,
	0,
	1,
	0,
	0,
	0
);
/*
	************************
	Patch Version:	1.001
	Written By:		Marakai
	Description:	Create the user and access rights for the API
	Depends On:		None
	************************
*/

begin transaction CREATE_API_USER
set xact_abort on
set transaction isolation level read committed

if not exists (select * from sys.schemas where name = 'API')
	exec('create schema API'); -- Allows it to run in its own batch

if not exists (select * from sys.server_principals where type = 'S' and name='api_user')
	create login api_user with password='ChangeMe';

if not exists (select * from sys.database_principals where type = 'S' and name='api_user')
	create user api_user for login api_user with default_schema=dbo;

grant EXECUTE on schema :: dbo to api_user;
grant EXECUTE on schema :: API to api_user;

create table dbo.DBPatchVersion
(
	PatchId int not null,
	PatchName nvarchar(500) not null,
	VersionNumber nvarchar(30) not null,
	DateApplied datetimeoffset not null,
	MessageLog nvarchar(max) null,
	constraint PK_DBPatchVersion_PatchId primary key (PatchId),
	constraint UQ_DBPatchVersion_VersionNumber unique (VersionNumber)
);

insert into dbo.DBPatchVersion
(
	PatchId,
	PatchName,
	VersionNumber,
	DateApplied,
	MessageLog
)
values
(
	1000,
	'Create_Database_Init',
	'1.000',
	getdate(),
	'Initialised database with baseline settings'
);

commit transaction CREATE_API_USER;
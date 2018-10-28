/*
	************************
	Patch Version:	1.001
	Written By:		Marakai
	Description:	Create the table for storing the AccessClients (third party allowed applications)
	Depends On:		1.000
	************************
*/

create table AccessClient
(
	ClientId uniqueidentifier not null,
	FriendlyName nvarchar(500) not null,
	AdministratorContactAddress nvarchar(max) not null,
	AdministratorName nvarchar(max) not null,
	RedirectUri nvarchar(500) not null,
	SecretKey nvarchar(100) not null,
	IsAdministratorApp bit not null,
	constraint PK_AccessClient_ClientId primary key (ClientId),
	constraint UQ_AccessClient_FriendlyName unique (FriendlyName),
	constraint UQ_AccessClient_RedirectUri unique (RedirectUri),
	constraint UQ_AccessClient_SecretKey unique (SecretKey)
)

-- Ensure there is only one administrator app so that we control it. This prevents hard-coding the ClientId, making it easier to recover in the event of a compromise
create unique index UQF_AccessClient_IsAdministratorApp on AccessClient (IsAdministratorApp) where IsAdministratorApp = 1;
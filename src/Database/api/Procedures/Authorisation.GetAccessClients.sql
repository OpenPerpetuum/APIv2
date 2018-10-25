if object_id('Authorisation.GetAccessClients', 'P') is not null
	drop procedure Authorisation.GetAccessClients;
go

create procedure Authorisation.GetAccessClients
	@ClientId uniqueidentifier null
as

select
	ClientId,
	FriendlyName,
	AdministratorContactAddress,
	AdministratorName,
	RedirectUri,
	SecretKey,
	IsAdministratorApp
from AccessClient
where (@ClientId = null or ClientId = @ClientId);
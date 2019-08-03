create procedure Authorisation.GetAccessClients
	@ClientId uniqueidentifier null
as

select
	ClientId,
	FriendlyName,
	AdministratorContactAddress,
	AdministratorName,
	RedirectUri,
	SecretKey as [Secret],
	IsAdministratorApp
from AccessClient
where (@ClientId is null or ClientId = @ClientId);

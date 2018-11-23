using IdentityServer4.Events;
using IdentityServer4.Extensions;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenPerpetuum.Core.Foundation.Processing;
using OpenPerpetuum.IdentityServer.ViewModel.Grants;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenPerpetuum.IdentityServer.Controllers
{
	[SecurityHeaders, Authorize]
	public class GrantsController : ApiControllerBase
	{
		private readonly IIdentityServerInteractionService interaction;
		private readonly IClientStore clientStore;
		private readonly IResourceStore resourceStore;
		private readonly IEventService events;

		public GrantsController(IIdentityServerInteractionService interaction, IClientStore clientStore, IResourceStore resourceStore, IEventService events, ICoreContext coreContext) : base(coreContext)
		{
			this.interaction = interaction;
			this.clientStore = clientStore;
			this.resourceStore = resourceStore;
			this.events = events;
		}

		[HttpGet]
		public async Task<IActionResult> Index()
		{
			return View("Index", await BuildViewModelAsync());
		}

		[HttpPost, ValidateAntiForgeryToken]
		public async Task<IActionResult> Revoke(string clientId)
		{
			await interaction.RevokeUserConsentAsync(clientId);
			await events.RaiseAsync(new GrantsRevokedEvent(User.GetSubjectId(), clientId));

			return RedirectToAction("Index");
		}

		private async Task<GrantsViewModel> BuildViewModelAsync()
		{
			var grants = await interaction.GetAllUserConsentsAsync();

			var list = new List<GrantViewModel>();
			foreach(var grant in grants)
			{
				var client = await clientStore.FindClientByIdAsync(grant.ClientId);

				if (client != null)
				{
					var resources = await resourceStore.FindResourcesByScopeAsync(grant.Scopes);

					var item = new GrantViewModel
					{
						ClientId = client.ClientId,
						ClientName = client.ClientName,
						ClientLogoUrl = client.LogoUri,
						ClientUrl = client.ClientUri,
						Created = grant.CreationTime,
						Expires = grant.Expiration,
						IdentityGrantNames = resources.IdentityResources.Select(ir => ir.DisplayName ?? ir.Name).ToArray(),
						ApiGrantNames = resources.ApiResources.Select(ar => ar.DisplayName ?? ar.Name).ToArray()
					};

					list.Add(item);
				}
			}

			return new GrantsViewModel
			{
				Grants = list
			};
		}
	}
}

using IdentityServer4.Events;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OpenPerpetuum.Core.Foundation.Processing;
using OpenPerpetuum.Core.SharedIdentity.Extensions;
using OpenPerpetuum.IdentityServer.Configuration;
using OpenPerpetuum.IdentityServer.InputModel.Consent;
using OpenPerpetuum.IdentityServer.ViewModel.Account;
using OpenPerpetuum.IdentityServer.ViewModel.Consent;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OpenPerpetuum.IdentityServer.Controllers
{
	[SecurityHeaders, Authorize]
	public class ConsentController : ApiControllerBase
	{
		private readonly IIdentityServerInteractionService interaction;
		private readonly IClientStore clientStore;
		private readonly IResourceStore resourceStore;
		private readonly IEventService events;
		private readonly ILogger<ConsentController> logger;

		public ConsentController(IIdentityServerInteractionService interaction, IClientStore clientStore, IResourceStore resourceStore, IEventService events, ILogger<ConsentController> logger, ICoreContext coreContext) : base(coreContext)
		{
			this.interaction = interaction;
			this.clientStore = clientStore;
			this.resourceStore = resourceStore;
			this.events = events;
			this.logger = logger;
		}

		[HttpGet]
		public async Task<IActionResult> Index(string returnUrl)
		{
			var vm = await BuildViewModelAsync(returnUrl);

			if (vm != null)
				return View("Index", vm);

			return View("Error");
		}

		[HttpPost, ValidateAntiForgeryToken]
		public async Task<IActionResult> Index(ConsentInputModel model)
		{
			var result = await ProcessConsentAsync(model);

			if (result.IsRedirect)
			{
				if (await clientStore.IsPkceClientAsync(result.ClientId))
					return View("Redirect", new RedirectViewModel { RedirectUrl = result.RedirectUri });

				return Redirect(result.RedirectUri);
			}

			if (result.HasValidationError)
				ModelState.AddModelError(string.Empty, result.ValidationError);

			if (result.ShowView)
				return View("Index", result.ViewModel);

			return View("Error");
		}

		private async Task<ProcessConsentResult> ProcessConsentAsync(ConsentInputModel model)
		{
			var result = new ProcessConsentResult();

			var request = await interaction.GetAuthorizationContextAsync(model.ReturnUrl);
			if (request == null)
				return result;

			ConsentResponse grantedConsent = null;

			if (model?.Button == "no")
			{
				grantedConsent = ConsentResponse.Denied;

				await events.RaiseAsync(new ConsentDeniedEvent(User.GetSubjectId(), request.ClientId, request.ScopesRequested));
			}
			else if (model?.Button == "yes")
			{
				if (model.ScopesConsented != null && model.ScopesConsented.Any())
				{
					var scopes = model.ScopesConsented;
					if (!ConsentOptions.EnableOfflineAccess)
						scopes = scopes.Where(s => s != IdentityServer4.IdentityServerConstants.StandardScopes.OfflineAccess);

					grantedConsent = new ConsentResponse
					{
						RememberConsent = model.RememberConsent,
						ScopesConsented = scopes.ToArray()
					};

					await events.RaiseAsync(new ConsentGrantedEvent(User.GetSubjectId(), request.ClientId, request.ScopesRequested, grantedConsent.ScopesConsented, grantedConsent.RememberConsent));
				}
				else
					result.ValidationError = ConsentOptions.MustChooseOneErrorMessage;
			}
			else
				result.ValidationError = ConsentOptions.InvalidSelectionErrorMessage;

			if (grantedConsent == null)
				result.ViewModel = await BuildViewModelAsync(model.ReturnUrl, model);
			else
			{
				await interaction.GrantConsentAsync(request, grantedConsent);

				result.RedirectUri = model.ReturnUrl;
				result.ClientId = request.ClientId;
			}

			return result;
		}

		private async Task<ConsentViewModel> BuildViewModelAsync(string returnUrl, ConsentInputModel model = null)
		{
			var request = await interaction.GetAuthorizationContextAsync(returnUrl);

			if (request != null)
			{
				var client = await clientStore.FindEnabledClientByIdAsync(request.ClientId);
				if (client != null)
				{
					var resources = await resourceStore.FindEnabledResourcesByScopeAsync(request.ScopesRequested);
					if (resources != null && (resources.IdentityResources.Any() || resources.ApiResources.Any()))
						return CreateConsentViewModel(model, returnUrl, request, client, resources);
					else
						logger.LogError($"No scopes matching { request.ScopesRequested.Aggregate((x, y) => $"{x}, {y}") }");
				}
			}
			else
				logger.LogError($"No consent request matching: { returnUrl }");

			return null;
		}

		private ConsentViewModel CreateConsentViewModel(ConsentInputModel model, string returnUrl, AuthorizationRequest request, Client client, Resources resources)
		{
			var vm = new ConsentViewModel
			{
				RememberConsent = model?.RememberConsent ?? true,
				ScopesConsented = model?.ScopesConsented ?? Enumerable.Empty<string>(),
				ReturnUrl = returnUrl,
				ClientName = client.ClientName ?? client.ClientId,
				ClientUrl = client.ClientUri,
				ClientLogoUrl = client.LogoUri,
				AllowRememberConsent = client.AllowRememberConsent
			};

			vm.IdentityScopes = resources.IdentityResources.Select(ir => CreateScopeViewModel(ir, vm.ScopesConsented.Contains(ir.Name) || model == null)).ToArray();
			vm.ResourceScopes = resources.ApiResources.SelectMany(ar => ar.Scopes).Select(s => CreateScopeViewModel(s, vm.ScopesConsented.Contains(s.Name) || model == null)).ToArray();

			if (ConsentOptions.EnableOfflineAccess && resources.OfflineAccess)
			{
				vm.ResourceScopes = vm.ResourceScopes.Union(new ScopeViewModel[] {
					GetOfflineAccessScope(vm.ScopesConsented.Contains(IdentityServer4.IdentityServerConstants.StandardScopes.OfflineAccess) || model == null)
				});
			}

			return vm;
		}

		private ScopeViewModel CreateScopeViewModel(IdentityResource identity, bool check)
		{
			return new ScopeViewModel
			{
				Name = identity.Name,
				DisplayName = identity.DisplayName,
				Description = identity.Description,
				Emphasize = identity.Emphasize,
				Required = identity.Required,
				Checked = check || identity.Required
			};
		}

		public ScopeViewModel CreateScopeViewModel(Scope scope, bool check)
		{
			return new ScopeViewModel
			{
				Name = scope.Name,
				DisplayName = scope.DisplayName,
				Description = scope.Description,
				Emphasize = scope.Emphasize,
				Required = scope.Required,
				Checked = check || scope.Required
			};
		}

		private ScopeViewModel GetOfflineAccessScope(bool check)
		{
			return new ScopeViewModel
			{
				Name = IdentityServer4.IdentityServerConstants.StandardScopes.OfflineAccess,
				DisplayName = ConsentOptions.OfflineAccessDisplayName,
				Description = ConsentOptions.OfflineAccessDescription,
				Emphasize = true,
				Checked = check
			};
		}
	}
}

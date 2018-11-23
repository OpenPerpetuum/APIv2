using IdentityModel;
using IdentityServer4.Events;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServer4.Test;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenPerpetuum.Core.Authorisation.Models;
using OpenPerpetuum.Core.Authorisation.Queries;
using OpenPerpetuum.Core.Foundation.Processing;
using OpenPerpetuum.Core.Foundation.Security;
using OpenPerpetuum.Core.SharedIdentity.Extensions;
using OpenPerpetuum.IdentityServer.Configuration;
using OpenPerpetuum.IdentityServer.InputModel;
using OpenPerpetuum.IdentityServer.InputModel.Account;
using OpenPerpetuum.IdentityServer.ViewModel.Account;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OpenPerpetuum.IdentityServer.Controllers
{
	[SecurityHeaders]
	[AllowAnonymous]
	public class AccountController : ApiControllerBase
	{
		private readonly IIdentityServerInteractionService identityService;
		private readonly IAuthenticationSchemeProvider schemeProvider;
		private readonly IEventService eventService;
		private readonly IClientStore clientStore;

		public AccountController(ICoreContext coreContext, IIdentityServerInteractionService identityService, IAuthenticationSchemeProvider schemeProvider, IEventService eventService, IClientStore clientStore) : base(coreContext)
		{
			this.clientStore = clientStore;
			this.eventService = eventService;
			this.identityService = identityService;
			this.schemeProvider = schemeProvider;
		}

		[HttpGet]
		public async Task<IActionResult> Login(string returnUrl)
		{
			var vm = await BuildLoginViewModelAsync(returnUrl);

			if (vm.IsExternalLoginOnly)
				return RedirectToAction("Challenge", "External", new { provider = vm.ExternalLoginScheme, returnUrl });

			return View(vm);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Login(LoginInputModel model, string button)
		{
			var context = await identityService.GetAuthorizationContextAsync(model.ReturnUrl);

			if (button != "login")
			{
				if (context != null)
				{
					// User has cancelled, pretend they denied the consent to IdentityServer4
					// This will return access_denied OIDC error.
					await identityService.GrantConsentAsync(context, ConsentResponse.Denied);

					if (await clientStore.IsPkceClientAsync(context.ClientId))
						return View("Redirect", new RedirectViewModel { RedirectUrl = model.ReturnUrl });

					return Redirect(model.ReturnUrl);
				}
				else
				{
					// No valid context. Go home
					return Redirect("~/");
				}
			}

			if (ModelState.IsValid)
			{
				var user = QueryProcessor.Process(new GAME_AuthenticateAndGetUserDetailsQuery { Email = model.Username, EncryptedPassword = model.Password.ToLegacyShaString() });

				if (user != UserModel.Default)
				{
					await eventService.RaiseAsync(new UserLoginSuccessEvent(user.Email, user.AccountId.ToString(), $"{user.FirstName} {user.LastName}"));

					// Only set explicit expiration if user chooses "remember me"
					AuthenticationProperties props = null;
					if (AccountOptions.AllowRememberLogin && model.RememberLogin)
					{
						props = new AuthenticationProperties
						{
							IsPersistent = true,
							ExpiresUtc = DateTimeOffset.UtcNow.Add(AccountOptions.RememberMeLoginDuration)
						};
					}

					await HttpContext.SignInAsync(user.AccountId.ToString(), user.Email, props);
					if (context != null)
					{
						if (await clientStore.IsPkceClientAsync(context.ClientId))
							return View("Redirect", new RedirectViewModel { RedirectUrl = model.ReturnUrl });

						return Redirect(model.ReturnUrl);
					}


					if (Url.IsLocalUrl(model.ReturnUrl))
						return Redirect(model.ReturnUrl);
					else if (string.IsNullOrEmpty(model.ReturnUrl))
						return Redirect("~/");
					else
						throw new InvalidOperationException("Invalid return URL");
				}

				await eventService.RaiseAsync(new UserLoginFailureEvent(model.Username, "invalid credentials"));
				ModelState.AddModelError("", AccountOptions.InvalidCredentialsErrorMessage);
			}

			// Something went wrong to get here
			var vm = await BuildLoginViewModelAsync(model);
			return View(vm);
		}

		[HttpGet]
		public async Task<IActionResult> Logout(string logoutId)
		{
			var vm = await BuildLogoutViewModelAsync(logoutId);

			if (!vm.ShowLogoutPrompt)
				return await Logout(vm);

			return View(vm);
		}

		[HttpPost, ValidateAntiForgeryToken]
		public async Task<IActionResult> Logout(LogoutInputModel model)
		{
			var vm = await BuildLoggedOutViewModelAsync(model.LogoutId);

			if (User?.Identity.IsAuthenticated == true)
			{
				await HttpContext.SignOutAsync();

				await eventService.RaiseAsync(new UserLogoutSuccessEvent(User.GetSubjectId(), User.GetDisplayName()));
			}

			if (vm.TriggerExternalSignout)
			{
				string url = Url.Action("Logout", new { logoutId = vm.LogoutId });

				return SignOut(new AuthenticationProperties { RedirectUri = url }, vm.ExternalAuthenticationScheme);
			}

			return View("LoggedOut", vm);
		}

		private async Task<LoginViewModel> BuildLoginViewModelAsync(string returnUrl)
		{
			var context = await identityService.GetAuthorizationContextAsync(returnUrl);
			if (context?.IdP != null)
			{
				// this is meant to short circuit the UI and only trigger the one external IdP
				return new LoginViewModel
				{
					EnableLocalLogin = false,
					ReturnUrl = returnUrl,
					Username = context?.LoginHint,
					ExternalProviders = new ExternalProvider[] { new ExternalProvider { AuthenticationScheme = context.IdP } }
				};
			}

			var schemes = await schemeProvider.GetAllSchemesAsync();

			var providers = schemes
				.Where(x => x.DisplayName != null)
				.Select(x => new ExternalProvider
				{
					DisplayName = x.DisplayName,
					AuthenticationScheme = x.Name
				}).ToList();

			var allowLocal = true;
			if (context?.ClientId != null)
			{
				var client = await clientStore.FindEnabledClientByIdAsync(context.ClientId);
				if (client != null)
				{
					allowLocal = client.EnableLocalLogin;

					if (client.IdentityProviderRestrictions != null && client.IdentityProviderRestrictions.Any())
					{
						providers = providers.Where(provider => client.IdentityProviderRestrictions.Contains(provider.AuthenticationScheme)).ToList();
					}
				}
			}

			return new LoginViewModel
			{
				AllowRememberLogin = AccountOptions.AllowRememberLogin,
				EnableLocalLogin = allowLocal && AccountOptions.AllowLocalLogin,
				ReturnUrl = returnUrl,
				Username = context?.LoginHint,
				ExternalProviders = providers.ToArray()
			};
		}

		private async Task<LoginViewModel> BuildLoginViewModelAsync(LoginInputModel model)
		{
			var vm = await BuildLoginViewModelAsync(model.ReturnUrl);
			vm.Username = model.Username;
			vm.RememberLogin = model.RememberLogin;
			return vm;
		}

		private async Task<LogoutViewModel> BuildLogoutViewModelAsync(string logoutId)
		{
			var vm = new LogoutViewModel { LogoutId = logoutId, ShowLogoutPrompt = AccountOptions.ShowLogoutPrompt };

			if (User?.Identity.IsAuthenticated != true)
			{
				// if the user is not authenticated, then just show logged out page
				vm.ShowLogoutPrompt = false;
				return vm;
			}

			var context = await identityService.GetLogoutContextAsync(logoutId);
			if (context?.ShowSignoutPrompt == false)
			{
				// it's safe to automatically sign-out
				vm.ShowLogoutPrompt = false;
				return vm;
			}

			// show the logout prompt. this prevents attacks where the user
			// is automatically signed out by another malicious web page.
			return vm;
		}

		private async Task<LoggedOutViewModel> BuildLoggedOutViewModelAsync(string logoutId)
		{
			// get context information (client name, post logout redirect URI and iframe for federated signout)
			var logout = await identityService.GetLogoutContextAsync(logoutId);

			var vm = new LoggedOutViewModel
			{
				AutomaticRedirectAfterSignOut = AccountOptions.AutomaticRedirectAfterSignOut,
				PostLogoutRedirectUri = logout?.PostLogoutRedirectUri,
				ClientName = string.IsNullOrEmpty(logout?.ClientName) ? logout?.ClientId : logout?.ClientName,
				SignOutIframeUrl = logout?.SignOutIFrameUrl,
				LogoutId = logoutId
			};

			if (User?.Identity.IsAuthenticated == true)
			{
				var idp = User.FindFirst(JwtClaimTypes.IdentityProvider)?.Value;
				if (idp != null && idp != IdentityServer4.IdentityServerConstants.LocalIdentityProvider)
				{
					var providerSupportsSignout = await HttpContext.GetSchemeSupportsSignOutAsync(idp);
					if (providerSupportsSignout)
					{
						if (vm.LogoutId == null)
						{
							// if there's no current logout context, we need to create one
							// this captures necessary info from the current logged in user
							// before we signout and redirect away to the external IdP for signout
							vm.LogoutId = await identityService.CreateLogoutContextAsync();
						}

						vm.ExternalAuthenticationScheme = idp;
					}
				}
			}

			return vm;
		}
	}
}
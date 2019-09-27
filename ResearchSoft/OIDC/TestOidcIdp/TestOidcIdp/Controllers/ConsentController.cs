using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Mvc;

namespace TestOidcIdp.Controllers
{
    public class ConsentController : Controller
	{

		private readonly IIdentityServerInteractionService _interaction;
		private readonly IClientStore _clientStore;

		public ConsentController(IIdentityServerInteractionService interaction, IClientStore clientStore)
		{
			_interaction = interaction;
			_clientStore = clientStore;
		}

		/// <summary>
		/// Shows the consent screen
		/// </summary>
		/// <param name="returnUrl"></param>
		/// <returns></returns>
		[HttpGet]
		[Route("consent")]
		public async Task<IActionResult> Index(string returnUrl)
		{
			var vm = new ConsentViewModel
			{
				ReturnUrl = returnUrl
			};

			if (vm != null)
			{
				return View("Index", vm);
			}

			return View("Error");
		}

		/// <summary>
		/// Handles the consent screen postback
		/// </summary>
		[HttpPost]
		[Route("consent")]
		public async Task<IActionResult> Index([Bind]ConsentViewModel model)
		{
			var grantedConsent = new ConsentResponse
			{
				ScopesConsented = new List<string>()
				{
					IdentityServerConstants.StandardScopes.OpenId,
					IdentityServerConstants.StandardScopes.Email,
				}
			};


			var request = await _interaction.GetAuthorizationContextAsync(model.ReturnUrl);
			await _interaction.GrantConsentAsync(request, grantedConsent);

			return Redirect(model.ReturnUrl);
		}
	}

	public class ConsentViewModel
	{
		public string ClientName { get; set; }
		public string ClientUrl { get; set; }
		public string ClientLogoUrl { get; set; }
		public bool AllowRememberConsent { get; set; }
		public string ReturnUrl { get; set; }
	}
}

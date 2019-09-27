using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using IdentityServer4;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServer4.Test;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TestOidcIdp.Models;

namespace TestOidcIdp.Controllers
{
	public class LoginController : Controller
    {
	    private readonly TestUserStore _users;

		private readonly IIdentityServerInteractionService _interaction;
	    private readonly IClientStore _clientStore;

		public LoginController(IIdentityServerInteractionService interaction,
			IClientStore clientStore, 
			TestUserStore users)
		{
			_interaction = interaction;
			_clientStore = clientStore;
			_users = users;
		}

		// GET: Login
		/*
		connect/authorize?response_type=code&scope=openid&client_id=myclient&state=af0ifjsldkj&redirect_uri=http%3A%2F%2Flocalhost%3A52225%2Fsignin-oidc

		connect/authorize?response_type=code&scope=openid&client_id=myclient&state=af0ifjsldkj&redirect_uri=http%3A%2F%2Flocalhost%3A44330%2Fcallback
		 */
		[Route("Account/Login")]
		public async Task<ActionResult> Login([FromQuery(Name = "returnUrl")]string redirectUrl)
		{
			LoginRedirectRequest request = new LoginRedirectRequest
			{
				//ClientId = clientId,
				RedirectUri = redirectUrl
				//ResponseType = responseType,
				//Scope = scope,
				//State = state,
				//Prompt = prompt
			};

            return View(request);
        }

        // POST: Login
        [HttpPost]
        [Route("Account/Login")]
		public async Task<ActionResult> SignIn([Bind]AuthenticationResponse response)
        {
	        var context = await _interaction.GetAuthorizationContextAsync(response.RedirectUri);

	        if (_users.ValidateCredentials(response.Email, response.Password))
	        {
		        var user = _users.FindByUsername(response.Email);

		        var a = user.Claims;
		        AuthenticationProperties props = null;

		        await HttpContext.SignInAsync(user.SubjectId, user.Username, props);

		        if (!string.IsNullOrEmpty(response.RedirectUri) && context != null)
		        {
			        return Redirect(response.RedirectUri);
		        }

				else return RedirectToAction("Index", "Home");
			}

	        throw new ArgumentException();
        }

	    [HttpGet]
	    [Route("Account/Signout")]
	    public async Task<ActionResult> Signout()
		{
			await HttpContext.SignOutAsync(IdentityServerConstants.DefaultCookieAuthenticationScheme);
			return RedirectToAction("Login");
		}
	}
}
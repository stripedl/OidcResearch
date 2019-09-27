using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Validation;

namespace TestOidcIdp.CustomHandlers
{
    public class CustomRedirectUriValidator : IRedirectUriValidator
	{
		public Task<bool> IsRedirectUriValidAsync(string requestedUri, Client client)
		{
			string path = requestedUri.Substring(0, requestedUri.IndexOf("?") > 0 ? 
				requestedUri.IndexOf("?") 
				: requestedUri.Length);

			// For production - use Friendly Subdomain Database
			HashSet<string> possibleRedirectUris = new HashSet<string>
			{
				"https://business.yrudakova.name/oauth2/idpresponse",
				"https://sales.yrudakova.name/oauth2/idpresponse",
				"https://oidcdebugger.com/debug",
				"http://localhost:52225/callback",
				"http://localhost:44330/callback"
			};

			if (client.ClientId == "myclient" && possibleRedirectUris.Contains(path))
			{
				return Task.FromResult(true);
			}
			else
			{
				return Task.FromResult(false);
			}
		}

		public Task<bool> IsPostLogoutRedirectUriValidAsync(string requestedUri, Client client)
		{
			throw new NotImplementedException();
		}
	}
}

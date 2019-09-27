using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Services;

namespace TestOidcIdp.CustomHandlers
{
    public class CustomCorsPolicyService : ICorsPolicyService
	{
		public Task<bool> IsOriginAllowedAsync(string origin)
		{
			// For production - use Friendly Subdomain Database

			HashSet<string> allowedOrigins = new HashSet<string>
			{
				"https://auth.yrudakova.name",
				"https://business.yrudakova.name",
				"https://sales.yrudakova.name",
				"https://oidcdebugger.com",
				"http://localhost:52225",
				"http://localhost:44330"
			};

			if (allowedOrigins.Contains(origin)) return Task.FromResult(true);
			else return Task.FromResult(true); ;
		}
	}
}

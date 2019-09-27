using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Stores;

namespace TestOidcIdp.CustomHandlers
{
    public class CustomClientStore : IClientStore
    {
	    public Task<Client> FindClientByIdAsync(string clientId)
	    {
		    if (clientId == "myclient")
		    {
			    var client = new Client
			    {
				    ClientId = "myclient",
				    AllowedGrantTypes = {
					    GrantType.AuthorizationCode,
					    "my_custom_grant_type"
				    },


				    ClientSecrets =
				    {
					    new Secret("secret".Sha256())
				    },

				    AllowedScopes =
				    {
					    IdentityServerConstants.StandardScopes.OpenId,
					    IdentityServerConstants.StandardScopes.Email,
					    IdentityServerConstants.StandardScopes.OfflineAccess
				    },
				    AllowOfflineAccess = true,
				    AlwaysIncludeUserClaimsInIdToken = true,
				    Claims = new List<Claim>()
				    {
					    new Claim(JwtClaimTypes.Address, "addr")
				    },
				    RequireConsent = false
			    };

			    return Task.FromResult(client);
		    }

		    return null;
	    }
    }
}

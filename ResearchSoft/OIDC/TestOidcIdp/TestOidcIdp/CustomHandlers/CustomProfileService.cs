using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Validation;
using TestOidcIdp.Models;

namespace TestOidcIdp.CustomHandlers
{
	public class CustomProfileService : IProfileService
	{
		public Task GetProfileDataAsync(ProfileDataRequestContext context)
		{
			if (context.Client.ClientId != "myclient") return Task.FromResult(0);

			context.IssuedClaims = context.Subject.Claims.ToList();

			// Userinfo endpoint = we don't know the request
			if (context.ValidatedRequest == null
				|| !(context.ValidatedRequest is ValidatedTokenRequest)) return Task.FromResult(0);

			var contextValidatedRequest = (ValidatedTokenRequest)context.ValidatedRequest;
			string uri = contextValidatedRequest.AuthorizationCode.RedirectUri;

			//string profileId = contextValidatedRequest.AuthorizationCode.Subject.Claims.First(x => x.Type == "sub").Value;
			string profileId = context.IssuedClaims.First(x => x.Type == "sub").Value;

			BridgeUser user = GetByRedirectUriAndProfileId(uri, profileId);
			if (user == null) return Task.FromResult(0);

			var claims = context.IssuedClaims;

			AddUserClaims(user, claims);
			context.IssuedClaims = claims;
			return Task.FromResult(0);
		}

		public Task IsActiveAsync(IsActiveContext context)
		{
			context.IsActive = true;
			return Task.FromResult(0);
		}

		private void AddUserClaims(BridgeUser user, List<Claim> claims)
		{
			UpsertClaim(claims, new Claim("friendly_subdomain", user.AccountFriendlySubdomain));
			UpsertClaim(claims, new Claim("tech_subdomain", user.AccountTechSubdomain));

			UpsertClaim(claims, new Claim("account_id", user.AccountId.ToString()));
			UpsertClaim(claims, new Claim("profile_id", user.ProfileId));
			UpsertClaim(claims, new Claim("user_guid", user.UserGuid));
		}

		private void UpsertClaim(List<Claim> claims, Claim claim)
		{
			var existingClaim = claims.FirstOrDefault(x => x.Type == claim.Type);

			if (existingClaim == null)
			{
				claims.Add(claim);
			}
			else
			{
				claims.Remove(existingClaim);
				claims.Add(claim);
			}
		}

		private BridgeUser GetByRedirectUriAndProfileId(string uri, string profileId)
		{
			Uri parsed = new Uri(uri);
			string authority = parsed.Host;

			// TODO: access DB
			BridgeUser user1 = new BridgeUser()
			{
				ProfileId = "818727",
				AccountFriendlySubdomain = "business.yrudakova.name",
				AccountTechSubdomain = "localhost",
				AccountId = 54,
				UserGuid = "e37d71d3-62bb-4534-8d0b-dbe45ac40cbe"
			};
			BridgeUser user2 = new BridgeUser()
			{
				ProfileId = "818727",
				AccountFriendlySubdomain = "sales.yrudakova.name",
				AccountTechSubdomain = "testtech.yrudakova.name",
				AccountId = 97,
				UserGuid = "422f7eec-8712-4cb0-bc95-d4ecc54b46c4"
			};

			List<BridgeUser> users = new List<BridgeUser>
			{
				user1,
				user2
			};

			return users.FirstOrDefault(x => x.ProfileId == profileId
			                                 && (authority == x.AccountTechSubdomain ||
			                                     authority == x.AccountFriendlySubdomain));
		}
	}
}

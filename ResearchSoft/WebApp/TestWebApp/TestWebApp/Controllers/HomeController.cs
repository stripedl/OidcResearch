using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using Jose;
using JWT;
using JWT.Serializers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;

namespace TestWebApp
{
	[Route("home")]
	public class HomeController : Controller
	{
		// GET
		public IActionResult Index()
		{
			try
			{
				ViewBag.Data = RenderHeaders(Request.Headers) + "\n" + RenderClaims(Request.Headers);
			}
			catch (Exception e)
			{
				var a = e;
				throw;
			}

			ViewBag.AuthServerCheckSessionUrl = Config.AuthServerUrl + "/connect/checksession";
			ViewBag.AuthServerUrl = Config.AuthServerUrl;
			return View();
		}

		// GET
		[Route("healthcheck")]
		public IActionResult Healthcheck()
		{
			return Ok();
		}

		// GET
		[Route("Logout")]
		[HttpPost]
		public IActionResult Logout()
		{
			string cookieValueFromReq = Request.Cookies["AWSELBAuthSessionCookie"];

			if (cookieValueFromReq == null)
			{
				cookieValueFromReq = "test";
			}

			CookieOptions options = new CookieOptions();
			options.Expires = DateTime.Now.AddYears(-1);
			Response.Cookies.Append("TestCookie", cookieValueFromReq, options);

			string url = Config.AuthServerLogoutUrl;

			return Redirect(url);
		}


		private string RenderHeaders(IHeaderDictionary dictionary)
		{
			string result = "";
			foreach (var h in dictionary)
			{
				result = result + $"{h.Key} : {h.Value} \n";
			}

			return result;
		}

		private string RenderClaims(IHeaderDictionary dictionary)
		{
			string result = "";
			if (dictionary.ContainsKey("X-Amzn-Oidc-Accesstoken"))
			{
				JsonWebTokenHandler handler = new JsonWebTokenHandler();
				string unformattedToken = dictionary["X-Amzn-Oidc-Accesstoken"];

				var e = Base64Url.Decode("AQAB");
				var n = Base64Url.Decode("n7ESj7VfHUenYOlolv-WJeYwByj5_PM5oeO3dL8V9zjvlQWn_UVR5iks76_eRutv4ZbgC2X73ZYO697eYnU4aQ0_qwW-kh8yoRGObxpQgCfky3UpeegLhiLrUlpN40u755rn4IERvyIHe1ifjk-li-O6DEVj9C3Q7ZH0TrWFnpi8MHhHKuN-3QC_TxKxQk8MOGxVZHhfQQkVE9BQVV79d56DBdlCd19XGJGajuGsJlUIcNj-mKeYvHIpQwpDkxSe8gNmBnR4KSdQRxv8cjYEbRVx6LToSVw3x5GmukqRpKKRwkx8oGBPiMG8-N9wm265GTFi9czyu4K594sg6BzpFw");

				var rsaKey = new RsaSecurityKey(new RSAParameters { Exponent = e, Modulus = n })
				{
					KeyId = "e87b6d98a03df7a9e44551a18308c24a"
				};

				TokenValidationParameters parameters = new TokenValidationParameters
				{
					ValidIssuer = "https://auth.yrudakova.name",
					ValidAudiences = new List<string>{ "https://auth.yrudakova.name/resources" },
					ValidateIssuerSigningKey = true,
					ValidateIssuer = true, 
					ValidateLifetime = true, 
					ValidateAudience = true, 
					IssuerSigningKeys = new List<SecurityKey>
					{
						rsaKey
					}
				};

				SecurityToken token;
				var rslt = handler.ValidateToken(unformattedToken, parameters);

				if (!rslt.IsValid) throw rslt.Exception;

				var jwt = handler.ReadJsonWebToken(unformattedToken);
				result += RenderClaims(jwt.Claims);
			}

			return result;
		}

		private string RenderClaims(IEnumerable<Claim> dictionary)
		{
			string result = "\n CLAIMS: \n";
			foreach (var h in dictionary)
			{
				result = result + $"{h.Type} : {h.Value} \n";
			}

			return result;
		}
	}
}
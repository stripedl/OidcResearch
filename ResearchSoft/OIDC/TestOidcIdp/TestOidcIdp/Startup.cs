using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4;
using IdentityServer4.Hosting;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServer4.Test;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TestOidcIdp.CustomHandlers;

namespace TestOidcIdp
{
    public class Startup
    {
	    private ILoggerFactory _loggerFactory;

		public Startup(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            Configuration = configuration;
	        _loggerFactory = loggerFactory;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
	        services.AddSingleton<ICorsPolicyService, CustomCorsPolicyService>();
	        services.AddSingleton<IRedirectUriValidator, CustomRedirectUriValidator>();
	        services.AddSingleton<IClientStore, CustomClientStore>();

			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

			var openIdResource = new IdentityResources.OpenId();
			openIdResource.UserClaims.Add(JwtClaimTypes.Address);
			openIdResource.UserClaims.Add(JwtClaimTypes.Name);

			services.AddIdentityServer()
				.AddInMemoryIdentityResources(new List<IdentityResource>
				{
					openIdResource,
					new IdentityResources.Email()
				})
				.AddInMemoryApiResources(new List<ApiResource>
				{
					new ApiResource("api1", "My API")
				})
				.AddTestUsers(new List<TestUser>
				{
					new TestUser
					{
						SubjectId = "818727",
						Username = "liza",
						Password = "12345",
						Claims = new List<Claim>()
						{
							new Claim(JwtClaimTypes.Name, "liza"),
							new Claim(JwtClaimTypes.Subject, "818727")
						}
					},
					new TestUser
					{
						SubjectId = "818729",
						Username = "mushy-hummingbird@example.com",
						Password = "Precious-Dugong-Enchanting-Copperhead-3",
						Claims = new List<Claim>()
						{
							new Claim(JwtClaimTypes.Name, "hummingbird"),
							new Claim(JwtClaimTypes.Subject, "818729")
						}
					},
				})
				.AddDeveloperSigningCredential()
				.AddProfileService<CustomProfileService>();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

	        var forwardOptions = new ForwardedHeadersOptions
	        {
		        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
		        RequireHeaderSymmetry = false
	        };

	        forwardOptions.KnownNetworks.Clear();
	        forwardOptions.KnownProxies.Clear();

	        // ref: https://github.com/aspnet/Docs/issues/2384
	        app.UseForwardedHeaders(forwardOptions);

			//app.UseMiddleware<SerilogMiddleware>();

			app.UseMiddleware<RequestResponseLoggingMiddleware>();
			app.UseIdentityServer();

			app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}

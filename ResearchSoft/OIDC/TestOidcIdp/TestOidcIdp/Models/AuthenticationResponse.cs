using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestOidcIdp.Models
{
    public class AuthenticationResponse
    {
	    public string Email { get; set; }

	    public string Password { get; set; }

	    public string ResponseType { get; set; }
	    public string Scope { get; set; }
	    public string ClientId { get; set; }
	    public string State { get; set; }
	    public string RedirectUri { get; set; }
	    public string Prompt { get; set; }
	}
}

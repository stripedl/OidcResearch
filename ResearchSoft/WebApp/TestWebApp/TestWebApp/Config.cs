using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestWebApp
{
    public static class Config
    {
	    public static string AuthServerUrl => "https://localhost:44342";

	    public static string AuthServerLogoutUrl => AuthServerUrl + "/Account/Signout";


    }
}

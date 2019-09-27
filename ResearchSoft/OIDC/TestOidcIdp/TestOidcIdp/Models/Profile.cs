using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TestOidcIdp.Models
{
    public class Profile
    {
		[Display(Name = "Profile Email")]
		public string Email { get; set; }

		[Display(Name = "Password")]
		public string Password { get; set; }
    }
}

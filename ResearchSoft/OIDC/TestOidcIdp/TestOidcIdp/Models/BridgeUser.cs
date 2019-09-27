using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestOidcIdp.Models
{
    public class BridgeUser
    {
		public string ProfileId { get; set; }
		public int AccountId { get; set; }
		public string UserGuid { get; set; }
		public string AccountFriendlySubdomain { get; set; }
		public string AccountTechSubdomain { get; set; }
    }
}

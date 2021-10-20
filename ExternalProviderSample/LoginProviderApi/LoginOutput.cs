using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
#pragma warning disable IDE1006 // Naming Styles

namespace ExternalProviderSample.LoginProviderApi
{
    public class LoginOutput
    {
        public bool login_successful { get; set; }
        public string reason { get; set; }
        public bool? is_site_administrator { get; set; }
        public string email_address { get;set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
#pragma warning disable IDE1006 // Naming Styles

namespace ExternalProviderSample.LoginProviderApi
{
    public class LoginInput
    {
        public int site_id { get; set; }
        public string username { get; set; }
        public string password { get; set; }
    }
}

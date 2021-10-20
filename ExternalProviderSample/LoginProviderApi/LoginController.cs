using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
// ReSharper disable StringLiteralTypo

// IMPORTANT NOTES
// ---------------
// This API should only be accessible by the SmarterStats installation.  You should restrict
// it with firewall rules or IIS IP restrictions so that only the SmarterStats MRS servers
// can call these functions.

namespace ExternalProviderSample.LoginProviderApi
{
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly ILogger<LoginController> _logger;

        // This usually wouldn't be here.  It's a method for us to enable or disable the tests for
        // this functionality.  In your implementation, you'll either require them and test for
        // them, or you won't.
        public bool RequireHttpHeaderToPass = false;

        public LoginController(ILogger<LoginController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Route("login")]
        public string Root()
        {
            return "Calls to the external provider must use the POST method";
        }

        [HttpPost]
        [Route("login")]
        // [Authorize] // Uncomment this to enable Client Certificate checks
        public LoginOutput DoLogin(LoginInput input)
        {
            // Validate our inputs.  This should probably be in your implementation as well
            if (input == null ||
                string.IsNullOrWhiteSpace(input.username))
            {
                return new LoginOutput
                {
                    login_successful = false,
                    reason = "Required inputs missing"
                };
            }

            // Trim inputs
             input.username = input.username.Trim();

            // Validate the HTTP Headers, if required
            if (RequireHttpHeaderToPass)
            {
                // This is just a sample to demonstrate how to restrict access by http headers.
                // If you implemented this functionality, you would use some sort of access
                // token that is kept and updated in a database
                var accessToken = Request.Headers["x-access-token"];
                if (accessToken != "9853E6DB-43FF-4E97-8040-92E9820AE6A4")
                    return new LoginOutput
                    {
                        login_successful = false,
                        reason = "Missing access token"
                    };
            }

            // Check the user credentials passed and generate a result
            return VerifyUser(input.site_id, input.username, input.password);
        }

        internal static LoginOutput VerifyUser(int siteId, string username, string password)
        {
            // In your implementation, this is where you'd check against a database or some other
            // data store

            // Do not forget to make site ids and usernames check in a case-insensitive way

            // DO NOT, for the love of Pete, actually hard code things like this in your
            // implementation.  This is just a sample!

            if (siteId == 1)
            {
                if (username.Equals("siteadmin", StringComparison.OrdinalIgnoreCase) &&
                    password.Equals("mySuperSecretPassword"))
                    return new LoginOutput
                    {
                        login_successful = true,
                        // Note that email_address and is_site_administrator are only important to return if 
                        // you have "Allow API to create users" enabled.  Returning null or empty values is 
                        // completely ok in either case
                        email_address = "siteadmin@example.com",
                        is_site_administrator = true
                    };

                if (username.Equals("user", StringComparison.OrdinalIgnoreCase) && password.Equals("password123"))
                    return new LoginOutput
                    {
                        login_successful = true,
                        email_address = "user@example.com",
                        is_site_administrator = false
                    };
            }
            else if (siteId == 2)
            {
                if (username.Equals("bob@example.com", StringComparison.OrdinalIgnoreCase) && password.Equals("123456"))
                    return new LoginOutput
                    {
                        login_successful = true,
                        email_address = "bob@example.com",
                        is_site_administrator = true
                    };
            }

            return new LoginOutput
            {
                login_successful = false,
                reason = "Invalid login"
            };
        }
    }
}
using ExternalProviderSample.LoginProviderApi;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
// ReSharper disable StringLiteralTypo

// IMPORTANT NOTES
// ---------------
// These unit tests are just a framework you can use to test your own implementation and formulate
// your own test patterns.  Once you've implemented your API, most of these tests will no longer
// pass until you adapt them to match your own data and rules.

namespace ExternalProviderSample.Tests
{
    [TestFixture]
    public class LoginProviderTests
    {
        [SetUp]
        public void Setup()
        {
            // If you need to do any database connections or anything for your tests, do it here
        }

        [TearDown]
        public void Teardown()
        {
            // If you need to do any cleanup for your tests, do it here
        }

        private LoginController CreateLoginController()
        {
            var controller = new LoginController(Mock.Of<ILogger<LoginController>>());
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            return controller;
        }

        [Test]
        public void CallWithInvalidInputs_ShouldFail()
        {
            var controller = CreateLoginController();

            // Null inputs should fail gracefully and not throw exceptions
            Assert.AreEqual(false, controller.DoLogin(null).login_successful);

            // Empty inputs should fail gracefully and not throw exceptions
            Assert.AreEqual(false, controller.DoLogin(new LoginInput()).login_successful);
            Assert.AreEqual(false,
                controller.DoLogin(new LoginInput { site_id = 0, username = "", password = "" }).login_successful);
        }

        [Test]
        public void IncorrectLogins_ShouldReturnFailure()
        {
            var controller = CreateLoginController();

            // Sites that don't exist
            var result = controller.DoLogin(new LoginInput()
            {
                site_id = 10,
                username = "sdffds",
                password = "asfraf"
            });
            Assert.AreEqual(false, result.login_successful);

            // Users that don't exist
            result = controller.DoLogin(new LoginInput()
            {

                site_id = 1,
                username = "nonexistantperson",
                password = "somepassword"
            });
            Assert.AreEqual(false, result.login_successful);

            // Users with no site ID
            result = controller.DoLogin(new LoginInput()
            {

                site_id = 0,
                username = "siteadmin",
                password = "mySuperSecretPassword"
            });
            Assert.AreEqual(false, result.login_successful);

            // Users with wrong site ID
            result = controller.DoLogin(new LoginInput()
            {

                site_id = 2,
                username = "siteadmin",
                password = "mySuperSecretPassword"
            });
            Assert.AreEqual(false, result.login_successful);
        }

        [Test]
        public void CorrectLogins_ShouldReturnSuccess()
        {
            var controller = CreateLoginController();

            var result = controller.DoLogin(new LoginInput()
            {

                site_id = 1,
                username = "siteadmin",
                password = "mySuperSecretPassword"
            });
            Assert.AreEqual(true, result.login_successful);
            Assert.AreEqual("siteadmin@example.com", result.email_address);
            Assert.AreEqual(true, result.is_site_administrator);

            result = controller.DoLogin(new LoginInput()
            {

                site_id = 1,
                username = "user",
                password = "password123"
            });
            Assert.AreEqual(true, result.login_successful);
            Assert.AreEqual("user@example.com", result.email_address);
            Assert.AreEqual(false, result.is_site_administrator);

            result = controller.DoLogin(new LoginInput()
            {

                site_id = 2,
                username = "bob@example.com",
                password = "123456"
            });
            Assert.AreEqual(true, result.login_successful);
            Assert.AreEqual("bob@example.com", result.email_address);
            Assert.AreEqual(true, result.is_site_administrator);
        }

        [Test]
        public void Usernames_ShouldBeTrimmed()
        {
            var controller = CreateLoginController();

            var result = controller.DoLogin(new LoginInput()
            {

                site_id = 1,
                username = "  siteadmin\t  ",
                password = "mySuperSecretPassword"
            });
            Assert.AreEqual(true, result.login_successful);
            Assert.AreEqual("siteadmin@example.com", result.email_address);
            Assert.AreEqual(true, result.is_site_administrator);
        }

        [Test]
        public void Passwords_ShouldNotBeTrimmed()
        {
            var controller = CreateLoginController();

            var result = controller.DoLogin(new LoginInput()
            {

                site_id = 1,
                username = "siteadmin",
                password = " mySuperSecretPassword"
            });
            Assert.AreEqual(false, result.login_successful);

            result = controller.DoLogin(new LoginInput()
            {

                site_id = 1,
                username = "siteadmin",
                password = "mySuperSecretPassword "
            });
            Assert.AreEqual(false, result.login_successful);

            result = controller.DoLogin(new LoginInput()
            {

                site_id = 1,
                username = "siteadmin",
                password = "\tmySuperSecretPassword"
            });
            Assert.AreEqual(false, result.login_successful);
        }

        [Test]
        public void HttpHeaders_ShouldBeVerified()
        {
            var controller = CreateLoginController();
            controller.RequireHttpHeaderToPass = true;

            // Try a test without the header first
            var result = controller.DoLogin(new LoginInput()
            {

                site_id = 1,
                username = "siteadmin",
                password = "mySuperSecretPassword"
            });
            Assert.AreEqual(false, result.login_successful);

            // A test with the header should work
            controller.ControllerContext.HttpContext.Request.Headers["x-access-token"] = "9853E6DB-43FF-4E97-8040-92E9820AE6A4";
            result = controller.DoLogin(new LoginInput()
            {

                site_id = 1,
                username = "siteadmin",
                password = "mySuperSecretPassword"
            });
            Assert.AreEqual(true, result.login_successful);
        }
    }
}
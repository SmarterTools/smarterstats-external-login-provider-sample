External Login Providers for SmarterStats

- [Overview](#overview)
- [Implementing the User Login Provider](#implementing-the-user-login-provider)
- [Allowing your Provider to create new users in SmarterStats](#allowing-your-provider-to-create-new-users-in-smarterstats)
- [Unit Testing your Provider](#unit-testing-your-provider)
- [Securing your Provider](#securing-your-provider)
  - [Use HTTPS](#use-https)
  - [Protect it with a firewall](#protect-it-with-a-firewall)
  - [Use IP Restrictions within your hosting software](#use-ip-restrictions-within-your-hosting-software)
  - [Requiring an Access Token to use your Provider](#requiring-an-access-token-to-use-your-provider)
  - [Secure connection with Client Certificates](#secure-connection-with-client-certificates)

# Overview

SmarterStats allows you to connect its user authentication checks to an external database or other system, instead of storing passwords inside of SmarterStats itself.  This external system is based on code that you create.  This document is intended for the software developers that are charged with implementing this provider for your company.

In SmarterStats and in the documentation, we call this the External Login Provider, but in this document we will call it the Provider for short.

There are several steps required to get this working.  

1. Create, test, and deploy the provider (this document is about this part of the process)
2. Configure the provider inside of SmarterStats at the admin level
3. For each site that you want to use the provider, change its authentication setting to External Provider (this can also be done through site propagation)

Once it is setup, all users for those sites will be authenticated only through your provider.  No internal login credentials for those sites in SmarterStats will function anymore.

The system administrator is always authenticated internally, and at this time cannot be attached to the provider.


# Implementing the User Login Provider

The provider is a web API endpoint.  A sample provider is available for download that is written in ASP.Net Web API 
using C# and Microsoft.Net 5.0.  But in reality, it can be written in any programming language that you are comfortable 
with, as long as it conforms to the requests and responses that SmarterStats expects.  So feel free to write it in 
node.js, java, python, or even classic ASP if that suits your skillset.

The Provider should be implemented as a single POST API call that takes three inputs as a JSON body in the request:

- site_id
- username
- password

For example:

`POST https://YOUR_ENDPOINT_URL/login`
```
{ 
  "site_id": "5", 
  "username": "bjones", 
  "password": "mySuperSecurePassword" 
} 
```

The provider should take this input and generate a response with one of the following formats:

Failure
```
{ 
  "login_successful": false, 
  "reason": "Message to show to the user." 
} 
```

Success
```
{ 
  "login_successful": true
} 
```

All responses should have a status code of 200 (OK).  Any result other than that will be considered an error and will be 
treated as an authentication failure.

In the sample we have provided, the file LoginController.cs does the bulk of the work of the API call and looking up the
users.  You can use that sample for reference to replace with your own implementation.  Follow the comments in the code
to understand why certain things are there, and what areas you'd need to replace with your own logic.

# Allowing your Provider to create new users in SmarterStats

By default, SmarterStats will only call your provider if a user exists with the username they wrote in under the site ID they 
specified on the login page.  This is what most people will want to do, and if you fall into that group, you can skip this 
section. 

However, if you enable the option within SmarterStats to allow the provider to create users, then your provider will be called 
on every login attempt for site IDs that exist and are set to authenticate with the provider.  If you enable that option, 
other return values are required from your API in order to know how to create the users, if necessary.

Success (if you want to allow the API to create users in SmarterStats)
```
{ 
  "login_successful": true,
  "username": "bjones",
  "email_address": "bjones@example.com",
  "is_site_administrator": false
} 
```

- username is just the username that should be created.  This should match the one sent to the provider, but may be cleaned up of spaces or normalized to a specific letter casing if you need
- email_address represents the email for the user that should be used for email reports
- is_site_administrator determines whether the user has access to settings for their site, like import rules or scheduled reports

If the user already exists, they will not be recreated or changed in any way.  But if they do not, they will be created with the default user settings and the items you pass back.

# Unit Testing your Provider

It is strongly advised to test all possible scenarios for your provider.  Using unit tests is a fairly straightforward way to do so.

While the specifics of implementing unit tests is beyond the scope of this document, the sample project we provide 
uses nUnit in c# in the ExternalProviderSample.Tests project to test various situations in the provider.  You may 
want to use this for inspiration for your own tests, or expand from it to test things like database disconnects or internet failures.  

# Securing your Provider

Use one or more of the following to secure access to your API and prevent others from using it to harvest user data.  **At the very minimum, HTTPS and either firewall rules or IP restrictions are required.**

## Use HTTPS

Deploying any public facing Web API without HTTPS is a very bad idea.  The same holds especially true for a login provider.  
The ONLY time that the provider should be deployed without HTTPS is if it is accessible only on a private IP restricted to 
intranet callers only.  If the API can be accessed at all from the outside world, or if there is any chance that someone 
may fiddle with the bindings and allow it to be accessed from the outside, you should take the time and configure 
HTTPS and disable HTTP calls to the provider.

How to do this depends on what web hosting software you use to host your provider, and is beyond the scope of this document.

## Protect it with a firewall 

If your provider and SmarterStats installs share the same private IP space, there is no need to expose the provider publicly.  
Use a private IP, or use firewall rules to block access to the provider from the outside world.

How to do this depends on the environment you use to host your provider, and is beyond the scope of this document.

## Use IP Restrictions within your hosting software

You may wish to use your hosting software (such as IIS or Apache) to restrict access to the provider to only come from the 
IP Addresses of your SmarterStats MRS servers.  This is one of the strongest ways to protect your provider.

How to do this depends on what web hosting software you use to host your provider, and is beyond the scope of this document, 
but some sources of information can be found below as a place to get started:

IIS
  * https://docs.microsoft.com/en-us/iis/configuration/system.webserver/security/ipsecurity/ 
  * https://www.ryadel.com/en/iis-restrict-web-site-access-ip-address-mask-block-deny-allow-how-to/

Apache
  * https://httpd.apache.org/docs/2.4/howto/access.html
  * https://ubiq.co/tech-blog/apache-restrict-access-by-ip/

## Requiring an Access Token to use your Provider

If desired, you can add an access token requirement to your provider.  You code a restriction inside of your code that 
requires a certain HTTP Header name/value pair, and in SmarterStats you can configure the same pair.
Note that using this option without using HTTPS is strongly discouraged, since the header can be sniffed over the wire 
and repeated.

In the sample code, you can see this by searching for uses of RequireHttpHeaderToPass, and you'll see where we implemented
a fake access token check that you can replace with your own headers.


## Secure connection with Client Certificates

Client certificates can be used to tightly protect your provider if you must expose your API on a public network.  How 
to require client certificates and access them differs on every hosting platform and programming language, so 
refer to the corresponding documentation if you wish to implement this type of security.

If you configure your provider to require a client certificate, you may attach the client certificate to SmarterStats 
using the certificate file and password within the External Providers configuration page.  This will instruct 
SmarterStats to use the certificate always when connecting to the provider.

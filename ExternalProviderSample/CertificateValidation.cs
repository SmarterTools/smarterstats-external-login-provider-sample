using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace ExternalProviderSample
{
    public class CertificateValidation
    {
        // This class is used if you wish to use client certificates to validate your calls to the API.  
        // This is an advanced use scenario, and most people will be fine with IP restrictions, but for 
        // those that require this level of security, the code and comments below are a SAMPLE of how you 
        // may choose to do it.

        // Note that client certificates are an advanced hosting topic and you should refer to the 
        // documentation specific to your hosting platform and coding language for specifics on how to 
        // generate them

        // --------------------------------------------------------------------------------------

        // To enable client certs for this sample when using IIS

        // 1 - Generate and install a client certificate
        // 2 - In LoginController.cs, uncomment the [Authorize] attribute on the DoLogin call
        // 3 - In Startup.cs, uncomment the line "CertificateValidation.ConfigureClientCertificates(services);" in ConfigureServices
        // 4 - Build and publish your provider
        // 5 - In IIS, In the SSL configuration for the site, select "Accept" or "Require" for the client certificates options
        // 6 - Upload the certificate to SmarterStats' External Providers page and test

        // --------------------------------------------------------------------------------------

        // A sample process for generating the client cert can be found below, drawn from the article referenced above

        // One resource that may be helpful is the first half of this article, from which this is drawn:
        // https://www.c-sharpcorner.com/article/using-certificates-for-api-authentication-in-net-5/

        // To create a sample client certificate for use in IIS usable for testing using an ELEVATED powershell, run the steps below:

        // Generate the new self-signed certificate
        //   New-SelfSignedCertificate -DnsName "localhost", "localhost" -CertStoreLocation "cert:\LocalMachine\My" -NotAfter (Get-Date).AddYears(10) -FriendlyName "CAlocalhost" -KeyUsageProperty All -KeyUsage CertSign, CRLSign, DigitalSignature

        // Retrieve and record the thumbprint by running the code below
        // It will look something like: 49BEA39CD9EA6D7C87FE6257C4C7CACCE993E800
        //   Get-ChildItem  -Path Cert:\LocalMachine\MY | Where-Object {$_.FriendlyName -Match "CAlocalhost"} | Select-Object FriendlyName, Thumbprint, Subject, NotBefore, NotAfter

        // Generate a password and attach it to a generated PFX, using the recorded thumbprint
        //   $mypwd = ConvertTo-SecureString -String "Server123" -Force -AsPlainText
        //   Get-ChildItem -Path cert:\localMachine\my\49BEA39CD9EA6D7C87FE6257C4C7CACCE993E800 | Export-PfxCertificate -FilePath "C:\cacert.pfx" -Password $mypwd

        // Run the following using the thumbprint from above
        //   $rootcert = ( Get-ChildItem -Path cert:\LocalMachine\My\49BEA39CD9EA6D7C87FE6257C4C7CACCE993E800 )

        // Generate the client certificate and note down the 
        //   New-SelfSignedCertificate -certstorelocation cert:\localmachine\my -dnsname "localhost" -Signer $rootcert -NotAfter (Get-Date).AddYears(10) -FriendlyName "Clientlocalhost"

        // Retrieve and record the CLIENT thumbprint by running the code below
        // It will look something like: F1716D80D4DB1553E78F263AE4C56DB91EE70E00
        //   Get-ChildItem  -Path Cert:\LocalMachine\MY | Where-Object {$_.FriendlyName -Match "Clientlocalhost"} | Select-Object FriendlyName, Thumbprint, Subject, NotBefore, NotAfter

        // Generate a password and attach it to a generated CLIENT PFX, using the recorded CLIENT thumbprint
        //   $mypwd = ConvertTo-SecureString -String "Client123" -Force -AsPlainText
        //   Get-ChildItem -Path cert:\localMachine\my\F1716D80D4DB1553E78F263AE4C56DB91EE70E00 | Export-PfxCertificate -FilePath "C:\clientcert.pfx" -Password $mypwd

        // On the server, open Manage Computer Certificates.
        //   Right click Trusted Root Certification Authorities > Certificates
        //   Choose All Tasks > Import
        //   Choose the cacert.pfx file (you may have to change your filters) and type in the password and finish the wizard with defaults
        // Now your server cert is installed

        // Upload your client cert to SmarterStats, and configure your external provider and hosting service to require Client Certificates and check for the CLIENT thumbprint you recorded above


        public static void ConfigureClientCertificates(IServiceCollection services)
        {
            services
                .AddAuthentication(CertificateAuthenticationDefaults.AuthenticationScheme)
                .AddCertificate(options =>
                {
                    // You should alter the code below to tighten or loosen the client certificate validation processes
                    // as suits your needs
                    options.ChainTrustValidationMode = X509ChainTrustMode.System;
                    options.RevocationMode = X509RevocationMode.NoCheck;
                    options.AllowedCertificateTypes = CertificateTypes.All;
                    options.ValidateCertificateUse = false;
                    options.ValidateValidityPeriod = false;
                    options.Events = new CertificateAuthenticationEvents
                    {
                        OnCertificateValidated = context => onCertificateValidated(context),
                        OnAuthenticationFailed = context => onAuthenticationFailed(context)
                    };
                });
        }

        private static Task onAuthenticationFailed(CertificateAuthenticationFailedContext context)
        {
            context.Fail("Invalid certificate");
            return Task.CompletedTask;
        }

        private static Task onCertificateValidated(CertificateValidatedContext context)
        {
            // If you are doing client certificate handling through code rather than through the host process
            // itself, this is where you'd place the permitted thumbprints
            var allowedThumbprints = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "F1716D80D4DB1553E78F263AE4C56DB91EE70E00" // <- This is just a sample and should be replaced
            };

            if (allowedThumbprints.Contains(context.ClientCertificate.Thumbprint))
                context.Success();
            else
                context.Fail("Invalid certificate");

            return Task.CompletedTask;
        }

    }
}

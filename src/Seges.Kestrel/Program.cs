using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

using System.Net;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using System.Reflection;

namespace Seges.Kestrel
{
    public static class IWebHostBuilderExtensions
    {
        private static readonly string DefaultCertificateThumbprint = "c79981b275ca7809c66ceb862bb7f3a5d0545665".ToUpperInvariant();
        private static bool IsDevelopment()
        {
            return Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
        }
        //private static readonly (IPAddress address, int port) ListenAddress = (IPAddress.Parse("0.0.0.0"), 5555);
        //public static void Main(string[] args)
        //{
        //    bool isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
        //    if (isDevelopment)
        //    {
        //        CertificateHelper.WithCertificate(DevelopmentCertificateThumbprint, cert =>
        //        {
        //            CreateAndRunHost(args, cert);
        //        });
        //    }
        //    else
        //    {
        //        CreateAndRunHost(args, null);
        //    }
        //}

        //private static void CreateAndRunHost(string[] args, X509Certificate2 cert)
        //{
        //    WebHost.CreateDefaultBuilder(args)
        //    .UseKestrel(options =>
        //        options.Listen(ListenAddress.address, ListenAddress.port,
        //            opt =>
        //            {
        //                opt.UseHttpsIfCertificateProvided(cert);
        //            }))
        //    .UseStartup()
        //    .Build()
        //    .Run();
        //}

        public static IWebHostBuilder UseKestrelHttpsForDevelopment(this IWebHostBuilder webHostBuilder, X509Certificate2 tlsCertificate = null, HostingUri hostingUri = null)
        {
            // Lifetime of certificate, if provided, is assumed to be managed by the caller
            if (hostingUri == null)
            {
                hostingUri = HostingUri.Default();
            }

            if (ShouldUseDefaultCertificate(tlsCertificate))
            {
                tlsCertificate = CertificateHelper.LoadCertificate(DefaultCertificateThumbprint);
            }
            if (ShouldHostOnHttp())
            {
                tlsCertificate = null;
            }
            Action<KestrelServerOptions> configureKestrel = (options) =>
            {
                options.Listen(hostingUri.Ip, hostingUri.Port,
                    opt =>
                    {
                        opt.UseHttpsIfCertificateProvided(tlsCertificate);
                    });
            };

            return webHostBuilder.UseKestrel(configureKestrel);
        }

        private static bool ShouldHostOnHttp()
        {
            return !IsDevelopment();
        }

        private static bool ShouldUseDefaultCertificate(X509Certificate2 tlsCertificate)
        {
            return IsDevelopment() && tlsCertificate == null;
        }

        public static ListenOptions UseHttpsIfCertificateProvided(this ListenOptions listenOptions, X509Certificate2 certificate)
        {
            if (certificate == null)
            {
                return listenOptions;
            }
            return listenOptions.UseHttps(new HttpsConnectionAdapterOptions
            {
                ServerCertificate = certificate,
                CheckCertificateRevocation = false,
                SslProtocols = System.Security.Authentication.SslProtocols.Tls
            });
        }
    }

}
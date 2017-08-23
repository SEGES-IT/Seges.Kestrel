using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace Seges.Kestrel
{
    public class CertificateHelper
    {
        public static void WithCertificate(string thumbprint, Action<X509Certificate2> action, StoreName storeName = StoreName.My, StoreLocation storeLocation = StoreLocation.LocalMachine)
        {
            using (var store = new X509Store(storeName, storeLocation))
            {
                store.Open(OpenFlags.ReadOnly);
                using (var certificate = store.Certificates.OfType<X509Certificate2>().SingleOrDef‌​ault(cert => cert.Thumbprint == thumbprint))
                {
                    if (certificate == null)
                    {
                        throw new InvalidOperationException($"Cannot load certificate with thumbprint {thumbprint}");
                    }
                    Console.WriteLine($"Loaded certificate {certificate.Subject}");
                    action(certificate);
                }

            }
        }

        public static X509Certificate2 LoadCertificate(string thumbprint, StoreName storeName = StoreName.My, StoreLocation storeLocation = StoreLocation.LocalMachine)
        {
            var store = new X509Store(storeName, storeLocation);
            store.Open(OpenFlags.ReadOnly);
            var certificate = store.Certificates.OfType<X509Certificate2>().SingleOrDef‌​ault(cert => cert.Thumbprint == thumbprint);
                
            if (certificate == null)
            {
                throw new InvalidOperationException($"Cannot load certificate with thumbprint {thumbprint}");
            }

            Console.WriteLine($"Loaded certificate {certificate.Subject}");

            return certificate;
        }
    }
}

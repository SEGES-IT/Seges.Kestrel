using System;
using System.Linq;
using System.Net;
using System.Reflection;

namespace Seges.Kestrel
{
    public class HostingUri
    {
        public IPAddress Ip { get; set; }
        public int Port { get; set; }
        public HostingUri(IPAddress ip, int port)
        {
            this.Ip = ip;
            this.Port = port;
        }

        public static HostingUri Default()
        {
            var assembly = Assembly.GetEntryAssembly();
            var assemblyName = assembly.GetName().Name;
            var stableNumberDerivedFromName = assemblyName.ToCharArray().Take(1000).Select(c => (decimal)c).Sum();
            stableNumberDerivedFromName = stableNumberDerivedFromName * stableNumberDerivedFromName;
            var port = (int)Math.Abs(stableNumberDerivedFromName) % 65535;
            var ip = IPAddress.Parse("0.0.0.0");
            var url = new HostingUri(ip, port);
            Console.WriteLine($"Generated hosting url {url.Ip}:{url.Port} from entry assembly name {assemblyName}");
            return url;
        }
    }
}
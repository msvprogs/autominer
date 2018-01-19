using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Msv.HttpTools
{
    public class ProxyList
    {
        private static readonly char[] M_DelimiterChars = " \t:".ToCharArray();

        public static ProxyInfo[] LoadFromFile(string path)
        {
            if (!File.Exists(path))
                return new ProxyInfo[0];

            var proxies = new List<ProxyInfo>();
            using (var reader = new StreamReader(path))
                while (!reader.EndOfStream)
                {
                    var proxyAddressPort = reader.ReadLine()
                        .Trim()
                        .Split(M_DelimiterChars, StringSplitOptions.RemoveEmptyEntries);
                    if (proxyAddressPort.Length < 2 || !int.TryParse(proxyAddressPort[1], out _))
                        continue;
                    proxies.Add(new ProxyInfo(new Uri($"http://{proxyAddressPort[0]}:{proxyAddressPort[1]}")));
                }
            var random = new Random();
            return proxies
                .OrderBy(x => random.NextDouble())
                .ToArray();
        }
    }
}

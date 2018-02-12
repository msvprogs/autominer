using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Rig.System.Contracts;

namespace Msv.AutoMiner.Rig.System.Unix
{
    public class MonoEnvironmentConfigurator : IEnvironmentConfigurator
    {
        private const string MonoConfigPath = "/etc/mono/config";

        private const string ConfigRootName = "configuration";
        private const string DllMapElementName = "dllmap";
        private const string SourceDllAttribute = "dll";
        private const string TargetDllAttribute = "target";

        private static readonly FileReader M_FileReader = new FileReader();

        private static readonly (string sourceLib, string targetLib)[] M_LibraryMappings =
        {
            ("nvml", "libnvidia-ml.so")
        };

        public string Check()
        {
            var fileContents = M_FileReader.ReadContents(MonoConfigPath);
            if (fileContents == string.Empty
                || GetExistingMappings(XDocument.Parse(fileContents))
                    .Select(x => x.sourceLib)
                    .Intersect(M_LibraryMappings.Select(x => x.sourceLib))
                    .Count() < M_LibraryMappings.Length)
                return "Please run this program with '--config-env' argument under "
                       + "root user (sudo) to configure Mono native library mappings.";
            return null;
        }

        public void Configure()
        {
            XDocument xml;
            using (var configReader = new StreamReader(MonoConfigPath))
                xml = XDocument.Load(configReader);

            var configTag = xml.Descendants(ConfigRootName).First();
            M_LibraryMappings
                .LeftOuterJoin(GetExistingMappings(xml), x => x.sourceLib, x => x.sourceLib,
                    (x, y) => y.targetLib == null ? x : default)
                .Where(x => x.sourceLib != null)
                .ForEach(x => configTag.Add(new XElement(
                    DllMapElementName,
                    new XAttribute("os", "!windows"),
                    new XAttribute(SourceDllAttribute, x.sourceLib),
                    new XAttribute(TargetDllAttribute, x.targetLib))));

            using (var configWriter = new StreamWriter(MonoConfigPath, false))
                xml.Save(configWriter);
        }

        private static IEnumerable<(string sourceLib, string targetLib)> GetExistingMappings(XContainer document)
            => document
                .Descendants(ConfigRootName)
                .Descendants(DllMapElementName)
                .Select(x => (sourceLib: x.Attribute(SourceDllAttribute)?.Value,
                    targetLib: x.Attribute(TargetDllAttribute)?.Value))
                .ToArray();
    }
}

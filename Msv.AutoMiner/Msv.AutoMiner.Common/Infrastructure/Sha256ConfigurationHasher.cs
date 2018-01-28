using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Msv.AutoMiner.Common.Models.ControlCenterService;

namespace Msv.AutoMiner.Common.Infrastructure
{
    public class Sha256ConfigurationHasher : IConfigurationHasher
    {
        private static readonly IDictionary<Type, PropertyInfo[]> M_ModelProperties =
            new Dictionary<Type, PropertyInfo[]>
            {
                [typeof(IMinerModel)] = typeof(IMinerModel).GetProperties(),
                [typeof(IAlgorithmMinerModel)] = typeof(IAlgorithmMinerModel).GetProperties()
            };

        public byte[] Calculate(IMinerModel[] miners, IAlgorithmMinerModel[] algorithms)
        {
            if (miners == null)
                throw new ArgumentNullException(nameof(miners));
            if (algorithms == null)
                throw new ArgumentNullException(nameof(algorithms));

            var configurationBuilder = new StringBuilder();

            miners.OrderBy(x => x.MinerId)
                .ThenBy(x => x.VersionId)
                .ForEach(x => AppendData(configurationBuilder, x));
            algorithms.OrderBy(x => x.AlgorithmId)
                .ForEach(x => AppendData(configurationBuilder, x));

            using (var sha256 = new SHA256CryptoServiceProvider())
                return sha256.ComputeHash(Encoding.ASCII.GetBytes(configurationBuilder.ToString()));
        }

        private static void AppendData<T>(StringBuilder builder, T data) 
            => M_ModelProperties[typeof(T)].ForEach(y => builder.Append(y.GetValue(data)));
    }
}

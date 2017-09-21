using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Msv.AutoMiner.Rig.Storage.Contracts;
using Msv.AutoMiner.Rig.Storage.Model;

namespace Msv.AutoMiner.Rig.Storage
{
    public class StoredSettings : IStoredSettings
    {
        public string ClientCertificateThumbprint
        {
            get => GetSetting<string>();
            set => SetSetting(value);
        }

        private static T GetSetting<T>([CallerMemberName] string key = null)
        {
            using (var context = new AutoMinerRigDbContext())
            {
                var entry = context.Settings.AsNoTracking().FirstOrDefault(x => x.Key == key);
                if (entry == null)
                    return default;
                return (T)Convert.ChangeType(entry.Value, typeof(T));
            }
        }

        private static void SetSetting<T>(T value, [CallerMemberName] string key = null)
        {
            using (var context = new AutoMinerRigDbContext())
            {
                var entry = context.Settings.FirstOrDefault(x => x.Key == key) 
                    ?? context.Settings.Add(new Setting {Key = key});
                entry.Value = value?.ToString();
                context.SaveChanges();
            }
        }
    }
}

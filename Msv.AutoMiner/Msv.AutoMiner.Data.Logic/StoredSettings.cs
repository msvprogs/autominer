using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Msv.AutoMiner.Data.Logic.Contracts;

namespace Msv.AutoMiner.Data.Logic
{
    public class StoredSettings : IStoredSettings
    {
        public string MultiPoolIgnoreAlgorithms
        {
            get => GetSetting<string>();
            set => SetSetting(value);
        }

        private readonly IAutoMinerDbContextFactory m_ContextFactory;

        public StoredSettings(IAutoMinerDbContextFactory contextFactory) 
            => m_ContextFactory = contextFactory;

        private T GetSetting<T>([CallerMemberName] string key = null)
        {
            using (var context = m_ContextFactory.CreateReadOnly())
            {
                var entry = context.Settings.FirstOrDefault(x => x.Key == key);
                if (entry == null)
                    return default;
                return (T)Convert.ChangeType(entry.Value, typeof(T));
            }
        }

        private void SetSetting<T>(T value, [CallerMemberName] string key = null)
        {
            using (var context = m_ContextFactory.Create())
            {
                var entry = context.Settings.FirstOrDefault(x => x.Key == key) 
                            ?? context.Settings.Add(new Setting {Key = key}).Entity;
                entry.Value = value?.ToString();
                context.SaveChanges();
            }
        }
    }
}

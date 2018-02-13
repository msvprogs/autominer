using Msv.AutoMiner.Common.Data.Enums;

namespace Msv.AutoMiner.Data
{
    public interface IEntity<T>
    {
        T Id { get; set; }
        ActivityState Activity { get; set; }
    }
}

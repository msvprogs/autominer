using Msv.AutoMiner.Common.Enums;

namespace Msv.AutoMiner.Data
{
    public interface IEntity<T>
    {
        T Id { get; set; }
        ActivityState Activity { get; set; }
    }
}

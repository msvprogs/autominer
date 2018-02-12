namespace Msv.AutoMiner.Common.Infrastructure
{
    public interface ISerializer<T>
    {
        string Serialize(T value);
        T Deserialize(string serialized);
    }
}

namespace Msv.Licensing.Common
{
    public interface ISerializer<T>
    {
        string Serialize(T value);
        T Deserialize(string serialized);
    }
}

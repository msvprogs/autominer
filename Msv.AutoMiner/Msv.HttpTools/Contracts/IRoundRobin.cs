namespace Msv.HttpTools.Contracts
{
    public interface IRoundRobin<out T>
    {
        int Count { get; }
        T GetNext();
    }
}

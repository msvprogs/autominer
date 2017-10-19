namespace Msv.AutoMiner.Rig.Commands
{
    public interface ICommandInterpreter
    {
        bool Interpret(string[] args);
    }
}

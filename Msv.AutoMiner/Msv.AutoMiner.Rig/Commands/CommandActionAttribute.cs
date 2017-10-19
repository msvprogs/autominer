using System;

namespace Msv.AutoMiner.Rig.Commands
{
    public class CommandActionAttribute : Attribute
    {
        public string Action { get; }
        public string Description { get; set; }

        public CommandActionAttribute(string action)
        {
            Action = action;
        }
    }
}

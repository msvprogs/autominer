using System;

namespace Msv.AutoMiner.Rig.Commands
{
    public class CommandParameterAttribute : Attribute
    {
        public string Name { get; }
        public string Description { get; set; }
        public string Example { get; set; }
        public bool IsRequired { get; set; }

        public CommandParameterAttribute(string name)
        {
            Name = name;
        }
    }
}

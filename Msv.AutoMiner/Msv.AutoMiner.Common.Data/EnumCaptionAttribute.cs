using System;

namespace Msv.AutoMiner.Common.Data
{
    [AttributeUsage(AttributeTargets.Field)]
    public class EnumCaptionAttribute : Attribute
    {       
        public string Caption { get; set; }

        public EnumCaptionAttribute(string caption)
            => Caption = caption;
    }
}
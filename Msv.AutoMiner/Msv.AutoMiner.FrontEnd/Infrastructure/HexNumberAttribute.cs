using System;
using System.ComponentModel.DataAnnotations;
using Msv.AutoMiner.Common.Helpers;

namespace Msv.AutoMiner.FrontEnd.Infrastructure
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class HexNumberAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (!(value is string str))
                return true;
            return HexHelper.IsHex(str);
        }
    }
}

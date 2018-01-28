using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using Msv.AutoMiner.Common;

namespace Msv.AutoMiner.FrontEnd.Infrastructure
{
    public class RegexPatternAttribute : ValidationAttribute
    {
        public string[] GroupNames { get; set; }

        public override bool IsValid(object value)
        {
            if (value == null)
                return true;
            if (!(value is string strValue))
                return false;
            if (strValue == string.Empty)
                return true;

            try
            {
                var regex = new Regex(strValue);
                if (GroupNames.IsNullOrEmpty())
                    return true;
                return regex.GetGroupNames().Intersect(GroupNames).Count() == GroupNames.Length;
            }
            catch
            {
                return false;
            }
        }
    }
}

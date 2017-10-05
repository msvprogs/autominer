using System;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Msv.AutoMiner.Common.Helpers;

namespace Msv.AutoMiner.FrontEnd.Infrastructure
{
    [HtmlTargetElement(Attributes = AbsoluteDateAttributeName)]
    public class RelativeDateTagHelper : TagHelper
    {
        private const string AbsoluteDateAttributeName = "msv-absolute-date";

        [HtmlAttributeName(AbsoluteDateAttributeName)]
        public DateTime? AbsoluteDate { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.Content.Clear();
            if (AbsoluteDate == null)
            {
                output.Content.SetContent("unavailable");
                return;
            }
            output.Attributes.SetAttribute("title", $"{AbsoluteDate.Value:yyyy-MM-dd HH:mm:ss} GMT");
            output.Content.SetContent(DateTimeHelper.ToRelativeTime(AbsoluteDate.Value));
        }
    }
}

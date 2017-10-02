using System;
using Microsoft.AspNetCore.Razor.TagHelpers;

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
            var difference = DateTime.UtcNow - AbsoluteDate.Value;
            if (difference == TimeSpan.Zero)
            {
                output.Content.SetContent("Right now");
                return;
            }

            string postfix;
            if (difference < TimeSpan.Zero)
            {
                postfix = "later";
                difference = difference.Negate();
            }
            else
                postfix = "ago";

            string relativeTimeString;
            if (difference.TotalSeconds < 60)
                relativeTimeString = $"{difference.TotalSeconds:F0} seconds";
            else if (difference.TotalMinutes < 60)
                relativeTimeString = $"{difference.TotalMinutes:F0} minutes";
            else if (difference.TotalHours < 24)
                relativeTimeString = $"{difference.Hours:F0} hours";
            else
                relativeTimeString = $"{difference.Hours:F0} days";

            output.Content.SetContent($"{relativeTimeString} {postfix}");
        }
    }
}

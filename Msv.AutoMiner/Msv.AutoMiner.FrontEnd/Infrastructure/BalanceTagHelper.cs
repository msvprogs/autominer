using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Msv.AutoMiner.Common.Helpers;

namespace Msv.AutoMiner.FrontEnd.Infrastructure
{
    [HtmlTargetElement(Attributes = BalancePropertyKey)]
    public class BalanceTagHelper : TagHelper
    {
        private const string BalancePropertyKey = "msv-balance";

        [HtmlAttributeName(BalancePropertyKey)]
        public double? Balance { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.Content.Clear();
            if (Balance == null)
                return;
         
            if (Balance > 0)
            {
                output.Attributes.SetAttribute("class", "text-right");
                var strongTag = new TagBuilder("strong");
                strongTag.InnerHtml.Append(ConversionHelper.ToCryptoCurrencyValue(Balance.Value));
                output.Content.SetHtmlContent(strongTag);
            }
            else
            {
                output.Attributes.SetAttribute("class", "text-right text-muted");
                output.Content.SetContent(ConversionHelper.ToCryptoCurrencyValue(Balance.Value));
            }
        }
    }
}

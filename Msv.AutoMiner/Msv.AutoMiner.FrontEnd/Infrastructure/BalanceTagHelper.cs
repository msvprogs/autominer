using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Msv.AutoMiner.Common.Helpers;

namespace Msv.AutoMiner.FrontEnd.Infrastructure
{
    [HtmlTargetElement(Attributes = BalancePropertyKey)]
    public class BalanceTagHelper : TagHelper
    {
        private const string BalancePropertyKey = "msv-balance";
        private const string BtcUnitPricePropertyKey = "msv-btc-price";

        [HtmlAttributeName(BalancePropertyKey)]
        public double? Balance { get; set; }

        [HtmlAttributeName(BtcUnitPricePropertyKey)]
        public double? BtcUnitPrice { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.Content.Clear();
            if (Balance == null)
                return;

            output.Attributes.AddClasses("text-right");
            if (Balance > 0)
            {
                var balanceContainer = new TagBuilder("div");
                var strongTag = new TagBuilder("strong");
                strongTag.InnerHtml.Append(ConversionHelper.ToCryptoCurrencyValue(Balance.Value));
                balanceContainer.InnerHtml.AppendHtml(strongTag);
                var btcPriceContainer = new TagBuilder("div")
                {
                    Attributes = {{"class", "secondary-info" } }
                };
                if (BtcUnitPrice != null)
                    btcPriceContainer.InnerHtml.AppendHtml(
                        $"≈{ConversionHelper.ToCryptoCurrencyValue(BtcUnitPrice.Value * Balance.Value)}&nbsp;BTC");
                output.Content.SetHtmlContent(new TagBuilder("div").InnerHtml
                    .AppendHtml(balanceContainer)
                    .AppendHtml(btcPriceContainer));
            }
            else
            {
                output.Attributes.AddClasses("text-muted");
                output.Content.SetContent(ConversionHelper.ToCryptoCurrencyValue(Balance.Value));
            }
        }
    }
}

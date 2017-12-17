using Microsoft.AspNetCore.Razor.TagHelpers;
using Msv.AutoMiner.Common.Helpers;

namespace Msv.AutoMiner.FrontEnd.Infrastructure
{
    [HtmlTargetElement(Attributes = AmountPropertyKey)]
    public class AmountTagHelper : TagHelper
    {
        private const string AmountPropertyKey = "msv-amount";
        private const string CurrencyPropertyKey = "msv-currency";

        [HtmlAttributeName(AmountPropertyKey)]
        public double Amount { get; set; }

        [HtmlAttributeName(CurrencyPropertyKey)]
        public string Currency { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.Attributes.AddClasses(Amount >= 0 ? "positive-amount" : "negative-amount");
            var amountString = ConversionHelper.ToCryptoCurrencyValue(Amount);
            output.Content.SetContent(Currency != null
                ? $"{amountString} {Currency.ToUpperInvariant()}"
                : amountString);
        }
    }
}

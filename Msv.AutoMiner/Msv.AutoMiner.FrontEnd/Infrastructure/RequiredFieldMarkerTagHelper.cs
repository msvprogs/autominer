using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Msv.AutoMiner.FrontEnd.Infrastructure
{
    [HtmlTargetElement("label", Attributes = "asp-for")]
    public class RequiredFieldMarkerTagHelper : TagHelper
    {
        [HtmlAttributeName("asp-for")]
        public ModelExpression For { get; set; }

        [HtmlAttributeName("msv-required-marker-disabled")]
        public bool Disabled { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (Disabled || For == null || !For.Metadata.IsRequired)
                return;
            if (For.Metadata.ModelType.IsValueType
                && Nullable.GetUnderlyingType(For.Metadata.ModelType) == null)
                return;

            var asteriskSpan = new TagBuilder("span")
            {
                Attributes =
                {
                    ["class"] = "required-value-marker",
                    ["title"] = "This field is required"
                }
            };
            asteriskSpan.InnerHtml.Append("*");
            output.PreContent.AppendHtml(asteriskSpan);
        }
    }
}

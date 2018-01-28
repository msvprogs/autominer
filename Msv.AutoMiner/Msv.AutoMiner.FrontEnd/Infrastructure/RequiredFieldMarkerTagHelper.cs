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

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (For == null || !For.Metadata.IsRequired)
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

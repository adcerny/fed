using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Fed.Web.Portal.TagHelpers
{
    [HtmlTargetElement("checkbox", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class CheckboxTagHelper : TagHelper
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool IsChecked { get; set; }
        public bool IsDisabled { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var content = await output.GetChildContentAsync();

            var id = WebUtility.HtmlEncode(Id);
            var name = WebUtility.HtmlEncode(Name);
            var text = WebUtility.HtmlEncode(content.GetContent());

            var divContent = new StringBuilder();

            divContent.Append("<label>");
            divContent.Append($"<input type=\"checkbox\" id=\"{id}\" name=\"{name}\" value=\"true\"");
            divContent.Append(IsChecked ? " checked" : "");
            divContent.Append(IsDisabled ? " disabled" : "");
            divContent.Append(">");
            divContent.Append("<svg viewBox=\"0 0 40 40\">");
            divContent.Append("<rect x=\"2\" y=\"2\" rx=\"4\" ry=\"4\" width=\"36\" height=\"36\" stroke-width=\"3\"></rect>");
            divContent.Append("<polyline points=\"10,20 18,27 30,12\" stroke-width=\"4\"></polyline>");
            divContent.Append("</svg>");
            divContent.Append($"<span>{text}</span>");
            divContent.Append("</label>");

            output.TagName = "div";
            output.Attributes.SetAttribute("class", "checkbox-container");

            output.Content.SetHtmlContent(divContent.ToString());
        }
    }
}

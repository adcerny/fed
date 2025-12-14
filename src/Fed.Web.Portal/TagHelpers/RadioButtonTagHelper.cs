using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Fed.Web.Portal.TagHelpers
{
    [HtmlTargetElement("radioButton", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class RadioButtonTagHelper : TagHelper
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public bool IsChecked { get; set; }
        public bool IsDisabled { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var content = await output.GetChildContentAsync();

            var id = WebUtility.HtmlEncode(Id);
            var name = WebUtility.HtmlEncode(Name);
            var value = WebUtility.HtmlEncode(Value);
            var text = WebUtility.HtmlEncode(content.GetContent());

            var divContent = new StringBuilder();

            divContent.Append("<label>");
            divContent.Append($"<input type=\"radio\" id=\"{id}\" name=\"{name}\" value=\"{value}\"");
            divContent.Append(IsChecked ? " checked" : "");
            divContent.Append(IsDisabled ? " disabled" : "");
            divContent.Append(">");
            divContent.Append("<svg viewBox=\"0 0 40 40\">");
            divContent.Append("<circle cx=\"20\" cy=\"20\" r=\"16\" stroke-width=\"4\"></circle>");
            divContent.Append("<circle cx=\"20\" cy=\"20\" r=\"8\" stroke-width=\"0\"></circle>");
            divContent.Append("</svg>");
            divContent.Append($"<span>{text}</span>");
            divContent.Append("</label>");

            output.TagName = "div";
            output.Attributes.SetAttribute("class", "radiobutton-container");

            output.Content.SetHtmlContent(divContent.ToString());
        }
    }
}

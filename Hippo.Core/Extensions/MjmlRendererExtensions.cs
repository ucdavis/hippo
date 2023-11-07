using Mjml.Net;
using Razor.Templating.Core;

namespace Hippo.Core.Extensions;

public static class MjmlRendererExtensions
{
        public static async Task<string> RenderView(this IMjmlRenderer mjmlRenderer, string view, object model = null)
        {
            var mjml = await RazorTemplateEngine.RenderAsync(view, model);

            var (html, errors) = mjmlRenderer.Render(mjml);

            if (errors.Any())
            {
                throw new Exception($"Error rendering notification for subject \"{view}\": {string.Join(Environment.NewLine, errors.Select(e => $"{e.Position.LineNumber}:{e.Position.LinePosition} {e.Error}"))}");
            }

            return html;
        }
}
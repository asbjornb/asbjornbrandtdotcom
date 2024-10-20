using System.IO;
using HandlebarsDotNet;

namespace SiteGenerator
{
    public class TemplateRenderer
    {
        private readonly IHandlebars _handlebars;

        public TemplateRenderer()
        {
            _handlebars = Handlebars.Create();
        }

        public string RenderTemplate(string templatePath, object data)
        {
            string templateContent = File.ReadAllText(templatePath);
            var template = _handlebars.Compile(templateContent);
            return template(data);
        }
    }
}

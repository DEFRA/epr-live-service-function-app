using System.Collections.Concurrent;
using System.Reflection;
using Scriban;
using Scriban.Parsing;
using Scriban.Runtime;

namespace EPR.LiveService.FunctionApp.Formatting;

public static class TemplateRenderer
{
    private const string TemplateResourceSegment = ".Formatting.Templates.";

    private static readonly Assembly Assembly = typeof(TemplateRenderer).Assembly;
    private static readonly EmbeddedTemplateLoader Loader = new();
    private static readonly ConcurrentDictionary<string, Template> Templates = new();

    public static string Render(string templateName, object model)
    {
        var template = Templates.GetOrAdd(templateName, ParseTemplate);
        var scriptObject = new ScriptObject();
        scriptObject.Import(model);

        var context = new TemplateContext
        {
            MemberRenamer = StandardMemberRenamer.Default,
            TemplateLoader = Loader
        };

        context.PushGlobal(scriptObject);
        try
        {
            return template.Render(context);
        }
        finally
        {
            context.PopGlobal();
        }
    }

    private static Template ParseTemplate(string templateName)
    {
        var templateText = EmbeddedTemplateLoader.LoadResource(templateName);
        var template = Template.Parse(templateText, templateName);

        if (!template.HasErrors)
        {
            return template;
        }

        var errors = string.Join(
            Environment.NewLine,
            template.Messages.Select(message => message.ToString()));

        throw new InvalidOperationException(
            $"The Scriban template '{templateName}' is invalid:{Environment.NewLine}{errors}");
    }

    private sealed class EmbeddedTemplateLoader : ITemplateLoader
    {
        public string? GetPath(TemplateContext context, SourceSpan callerSpan, string templateName) =>
            templateName;

        public string? Load(TemplateContext context, SourceSpan callerSpan, string templatePath) =>
            LoadResource(templatePath);

        public ValueTask<string?> LoadAsync(TemplateContext context, SourceSpan callerSpan, string templatePath) =>
            ValueTask.FromResult<string?>(LoadResource(templatePath));

        public static string LoadResource(string templateName)
        {
            var resourceName = Assembly.GetManifestResourceNames()
                .SingleOrDefault(name => name.EndsWith(
                    $"{TemplateResourceSegment}{templateName}",
                    StringComparison.Ordinal));

            if (resourceName is null)
            {
                throw new FileNotFoundException(
                    $"Embedded Scriban template '{templateName}' was not found.",
                    templateName);
            }

            using var stream = Assembly.GetManifestResourceStream(resourceName)
                ?? throw new FileNotFoundException(
                    $"Embedded Scriban template resource '{resourceName}' could not be opened.",
                    templateName);

            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}

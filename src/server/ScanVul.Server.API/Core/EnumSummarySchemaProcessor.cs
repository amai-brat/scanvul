using Namotion.Reflection;
using NJsonSchema.Generation;

namespace ScanVul.Server.API.Core;

public class EnumSummarySchemaProcessor : ISchemaProcessor
{
    public void Process(SchemaProcessorContext context)
    {
        var type = Nullable.GetUnderlyingType(context.ContextualType) ?? context.ContextualType;
        if (!type.IsEnum) return;
        
        var schema = context.Schema;
        var descriptions = Enum.GetNames(type).Select(name =>
        {
            var member = type.GetMember(name).FirstOrDefault();
            var summary = member?.GetXmlDocsSummary(); 
            return string.IsNullOrWhiteSpace(summary)
                ? $"- **{name}**"
                : $"- **{name}**: {summary}";
        });

        var descriptionString = string.Join("\n", descriptions);

        schema.Description = string.IsNullOrWhiteSpace(schema.Description)
            ? descriptionString
            : $"{schema.Description}\n\n{descriptionString}"; 
    }
}
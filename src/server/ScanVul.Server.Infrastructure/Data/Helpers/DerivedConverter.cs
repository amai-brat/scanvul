using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace ScanVul.Server.Infrastructure.Data.Helpers;

public class DerivedConverter<T> : JsonConverter<T> 
    where T : class
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        if (root.TryGetProperty("$type", out var typeProp))
        {
            var typeDiscriminator = typeProp.GetString();
            
            if (typeDiscriminator != null)
            {
                return (T)JsonSerializer.Deserialize(root.GetRawText(), Type.GetType(typeDiscriminator)!, options)!;
            }

            throw new JsonException($"Unknown type discriminator: {typeDiscriminator}");
        }
        
        if (typeToConvert.IsAbstract)
        {
            throw new JsonException($"Cannot deserialize into abstract type {typeToConvert.Name} without a type discriminator.");
        }

        return JsonSerializer.Deserialize<T>(root.GetRawText(), options) 
               ?? throw new JsonException("Couldn't deserialize");
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        var jsonObject = JsonSerializer.SerializeToNode(value, value.GetType(), options)!.AsObject();
        jsonObject.Add("$type", JsonValue.Create<string>(value.GetType().AssemblyQualifiedName));

        jsonObject.WriteTo(writer, options);
    }
}
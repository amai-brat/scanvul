using System.Text.Json;
using OpenSearch.Client;
using OpenSearch.Net;

namespace ScanVul.Server.Infrastructure.OpenSearch.Helpers;

public class SystemTextJsonSerializer : IOpenSearchSerializer
{
    public static readonly ConnectionSettings.SourceSerializerFactory Default = 
        (_, _) => new SystemTextJsonSerializer();
    
    private readonly JsonSerializerOptions _baseOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };
    private readonly JsonSerializerOptions _intendedOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };
    
    public object Deserialize(Type type, Stream stream)
    {
        return JsonSerializer.Deserialize(stream, type, _baseOptions)!;
    }

    public T Deserialize<T>(Stream stream)
    {
        return JsonSerializer.Deserialize<T>(stream, _baseOptions)!;
    }

    public async Task<object> DeserializeAsync(
        Type type, 
        Stream stream, 
        CancellationToken cancellationToken = default)
    {
        return (await JsonSerializer.DeserializeAsync(stream, type, _baseOptions, cancellationToken))!;
    }

    public async Task<T> DeserializeAsync<T>(
        Stream stream, 
        CancellationToken cancellationToken = default)
    {
        return (await JsonSerializer.DeserializeAsync<T>(stream, _baseOptions, cancellationToken))!;
    }

    public void Serialize<T>(T data, Stream stream, SerializationFormatting formatting = SerializationFormatting.None)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(data, 
            formatting == SerializationFormatting.Indented 
                ? _intendedOptions 
                : _baseOptions);
        stream.Write(bytes);
    }

    public Task SerializeAsync<T>(
        T data, 
        Stream stream, 
        SerializationFormatting formatting = SerializationFormatting.None,
        CancellationToken cancellationToken = default)
    {
        return JsonSerializer.SerializeAsync(stream, data, 
            formatting == SerializationFormatting.Indented 
                ? _intendedOptions 
                : _baseOptions, cancellationToken);
    }
}
using System.Text.Json;

namespace Xbox360DeploymentToolkit.Services;
public sealed class JsonStore
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true, WriteIndented = true };
    public T Load<T>(string path) where T : new() => File.Exists(path) ? JsonSerializer.Deserialize<T>(File.ReadAllText(path), Options) ?? new() : new();
    public void Save<T>(string path, T value) { Directory.CreateDirectory(Path.GetDirectoryName(path)!); var tmp = path + ".tmp"; File.WriteAllText(tmp, JsonSerializer.Serialize(value, Options)); File.Move(tmp, path, true); }
}

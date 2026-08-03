namespace Xbox360DeploymentToolkit.Services;
public sealed class AppLogger
{
    private readonly string _path;
    public AppLogger(string dataRoot) { Directory.CreateDirectory(Path.Combine(dataRoot, "logs")); _path = Path.Combine(dataRoot, "logs", $"toolkit-{DateTime.Now:yyyyMMdd}.log"); }
    public void Write(string level, string message) => File.AppendAllText(_path, $"{DateTime.Now:O}\t{level}\t{message.ReplaceLineEndings(" ")}\n");
}

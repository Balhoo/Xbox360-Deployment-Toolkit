using System.Net;
using Xbox360DeploymentToolkit.Models;

#pragma warning disable SYSLIB0014
namespace Xbox360DeploymentToolkit.Services;
public sealed class FtpService
{
    private FtpWebRequest Request(string host, string path, string user, string password, string method)
    {
        var uri = new Uri($"ftp://{host.Trim().TrimEnd('/')}/{path.TrimStart('/')}");
        var request = (FtpWebRequest)WebRequest.Create(uri); request.Method = method; request.Credentials = new NetworkCredential(user, password); request.UseBinary = true; request.KeepAlive = false; request.Timeout = 15000; return request;
    }
    public async Task<IReadOnlyList<FtpEntry>> ListAsync(string host, string path, string user, string password)
    {
        var request = Request(host, path, user, password, WebRequestMethods.Ftp.ListDirectoryDetails);
        using var response = (FtpWebResponse)await request.GetResponseAsync(); using var reader = new StreamReader(response.GetResponseStream()); var entries = new List<FtpEntry>();
        while (await reader.ReadLineAsync() is { } line) { var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries); if (parts.Length < 4) continue; var name = string.Join(" ", parts.Skip(8)); if (string.IsNullOrWhiteSpace(name) || name is "." or "..") continue; var isDir = line[0] == 'd'; long? size = !isDir && parts.Length > 4 && long.TryParse(parts[4], out var n) ? n : null; entries.Add(new(name, path.TrimEnd('/') + "/" + name, isDir, size)); }
        return entries;
    }
    public async Task UploadAsync(string host, string remotePath, string user, string password, string localPath, IProgress<double>? progress, CancellationToken cancellationToken)
    {
        var info = new FileInfo(localPath); var request = Request(host, remotePath, user, password, WebRequestMethods.Ftp.UploadFile); request.ContentLength = info.Length;
        await using var input = File.OpenRead(localPath); await using var output = await request.GetRequestStreamAsync(); var buffer = new byte[1024 * 128]; long sent = 0; int read;
        while ((read = await input.ReadAsync(buffer, cancellationToken)) > 0) { await output.WriteAsync(buffer.AsMemory(0, read), cancellationToken); sent += read; progress?.Report(info.Length == 0 ? 100 : sent * 100d / info.Length); }
        using var response = (FtpWebResponse)await request.GetResponseAsync();
    }
    public async Task<long> SizeAsync(string host, string remotePath, string user, string password) { var request = Request(host, remotePath, user, password, WebRequestMethods.Ftp.GetFileSize); using var response = (FtpWebResponse)await request.GetResponseAsync(); return response.ContentLength; }
}
#pragma warning restore SYSLIB0014

using System.Security.Cryptography;
using System.Text;

namespace Xbox360DeploymentToolkit.Services;
public sealed class CredentialStore
{
    private readonly string _path;
    public CredentialStore(string dataRoot) => _path = Path.Combine(dataRoot, "ftp.credential");
    public void Save(string username, string password) { var plain = Encoding.UTF8.GetBytes(username + "\n" + password); var protectedBytes = ProtectedData.Protect(plain, null, DataProtectionScope.CurrentUser); File.WriteAllBytes(_path, protectedBytes); }
    public (string User, string Password)? Load() { try { if (!File.Exists(_path)) return null; var plain = Encoding.UTF8.GetString(ProtectedData.Unprotect(File.ReadAllBytes(_path), null, DataProtectionScope.CurrentUser)); var parts = plain.Split('\n', 2); return parts.Length == 2 ? (parts[0], parts[1]) : null; } catch { return null; } }
    public void Delete() { if (File.Exists(_path)) File.Delete(_path); }
}

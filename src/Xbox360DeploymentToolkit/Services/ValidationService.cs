using System.Security.Cryptography;
using Xbox360DeploymentToolkit.Models;

namespace Xbox360DeploymentToolkit.Services;
public sealed class ValidationService
{
    public string ValidateGame(GameItem game, string root)
    {
        if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root)) return "Raíz local no disponible";
        var missing = game.RequiredPaths.Where(p => !File.Exists(Path.Combine(root, p)) && !Directory.Exists(Path.Combine(root, p))).ToList();
        return missing.Count == 0 ? "Completo" : $"Faltan {missing.Count}: {string.Join(", ", missing)}";
    }
    public async Task<string> Sha256Async(string path) { await using var stream = File.OpenRead(path); return Convert.ToHexString(await SHA256.HashDataAsync(stream)); }
}

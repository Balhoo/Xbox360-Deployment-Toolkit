namespace Xbox360DeploymentToolkit.Services;
public sealed record DriveInfoDto(string Name, string Label, string Format, long TotalBytes, long FreeBytes, bool Ready);
public sealed class DriveService
{
    public IReadOnlyList<DriveInfoDto> List() => DriveInfo.GetDrives().Select(d => { try { return new DriveInfoDto(d.Name, d.IsReady ? d.VolumeLabel : "", d.IsReady ? d.DriveFormat : "", d.IsReady ? d.TotalSize : 0, d.IsReady ? d.AvailableFreeSpace : 0, d.IsReady); } catch { return new DriveInfoDto(d.Name, "", "", 0, 0, false); } }).ToList();
    public IReadOnlyList<string> PrepareFolders(string root, IEnumerable<string> folders, bool dryRun)
    {
        var driveRoot = Path.GetPathRoot(Path.GetFullPath(root));
        if (string.IsNullOrWhiteSpace(driveRoot) || !Directory.Exists(driveRoot)) throw new InvalidOperationException("La unidad seleccionada no está disponible.");
        var results = new List<string>();
        foreach (var folder in folders) { var target = Path.GetFullPath(Path.Combine(root, folder)); if (!target.StartsWith(Path.GetFullPath(root), StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Ruta insegura bloqueada."); if (!dryRun) Directory.CreateDirectory(target); results.Add($"{(dryRun ? "SIMULAR" : "CREAR")} {target}"); }
        return results;
    }
}

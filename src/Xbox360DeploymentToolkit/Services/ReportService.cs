using System.Text;
using System.Text.Json;
using Xbox360DeploymentToolkit.Models;

namespace Xbox360DeploymentToolkit.Services;
public sealed class ReportService
{
    public string Export(string folder, IEnumerable<PreparationItem> preparation, IEnumerable<ChecklistStep> steps, IEnumerable<GameItem> games, IEnumerable<AuditRecord> audit)
    {
        Directory.CreateDirectory(folder); var stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss"); var model = new { GeneratedAt = DateTime.Now, Preparation = preparation, Checklist = steps, Games = games, Audit = audit }; var json = Path.Combine(folder, $"deployment-{stamp}.json"); File.WriteAllText(json, JsonSerializer.Serialize(model, new JsonSerializerOptions { WriteIndented = true }));
        var csv = Path.Combine(folder, $"deployment-{stamp}.csv"); var sb = new StringBuilder("Tipo,Elemento,Estado,Detalle\n"); foreach (var p in preparation) sb.AppendLine($"Preparación,{Csv(p.Name)},{Csv(p.IsReady ? "Listo" : "Pendiente")},{Csv(p.Notes)}"); foreach (var s in steps) sb.AppendLine($"Checklist,{Csv(s.Title)},{Csv(s.Status)},{Csv(s.Notes)}"); foreach (var g in games) sb.AppendLine($"Juego,{Csv(g.Title)},{Csv(g.State)},{Csv(g.Validation)}"); File.WriteAllText(csv, sb.ToString(), Encoding.UTF8); return json;
    }
    private static string Csv(string value) => $"\"{value.Replace("\"", "\"\"")}\"";
}

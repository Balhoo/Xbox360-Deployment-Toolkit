using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Xbox360DeploymentToolkit.Models;

public abstract class ObservableObject : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    protected bool Set<T>(ref T field, T value, [CallerMemberName] string? name = null) { if (EqualityComparer<T>.Default.Equals(field, value)) return false; field = value; PropertyChanged?.Invoke(this, new(name)); return true; }
    protected void Raise([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new(name));
}

public sealed class ChecklistStep : ObservableObject
{
    public required string Id { get; init; }
    public required string Phase { get; init; }
    public required string Title { get; init; }
    public string Instructions { get; init; } = "";
    public string Warning { get; init; } = "";
    public string[] DependsOn { get; init; } = [];
    private bool _isComplete; public bool IsComplete { get => _isComplete; set { if (Set(ref _isComplete, value)) Raise(nameof(Status)); } }
    private string _notes = ""; public string Notes { get => _notes; set => Set(ref _notes, value); }
    public string Status => IsComplete ? "Completado" : "Pendiente";
}

public sealed class GameItem : ObservableObject
{
    public required string Id { get; init; }
    public required string Title { get; init; }
    public string Type { get; init; } = "SingleDisc";
    public string[] RequiredPaths { get; init; } = [];
    public string Notes { get; init; } = "";
    private string _state = "Pendiente"; public string State { get => _state; set => Set(ref _state, value); }
    private string _validation = "Sin verificar"; public string Validation { get => _validation; set => Set(ref _validation, value); }
}

public sealed record FtpEntry(string Name, string FullPath, bool IsDirectory, long? Size);
public sealed record AuditRecord(DateTime Timestamp, string Category, string Message, string Result);
public sealed class ToolkitSettings { public string ReportFolder { get; set; } = "reports"; public bool DryRun { get; set; } = true; public string DefaultLocalRoot { get; set; } = ""; public string DefaultRemoteRoot { get; set; } = "/Hdd1"; }
public sealed class ProcedureDefinition { public List<ChecklistStep> Steps { get; set; } = []; }
public sealed class GamesDefinition { public List<GameItem> Games { get; set; } = []; }

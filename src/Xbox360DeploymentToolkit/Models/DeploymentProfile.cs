namespace Xbox360DeploymentToolkit.Models;

public sealed class DeploymentProfile
{
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public string ConsoleModel { get; set; } = "Sin confirmar";
    public string InternalCapacity { get; set; } = "Sin confirmar";
    public string HackType { get; set; } = "Sin confirmar";
    public string RghConfirmation { get; set; } = "Sin confirmar";
    public string KernelVersion { get; set; } = "Sin confirmar";
    public string NandStatus { get; set; } = "No disponible";
    public string LiveStatus { get; set; } = "Sin confirmar";
    public string InstallationMode { get; set; } = "Instalación limpia";
    public string ExistingComponents { get; set; } = "";
    public bool UseUsb { get; set; } = true;
    public int UsbCapacityGb { get; set; } = 16;
    public bool UseExternalStorage { get; set; }
    public string ExternalCapacity { get; set; } = "";
    public bool UsePcLibrary { get; set; } = true;
    public string PcLibraryPath { get; set; } = "";
    public List<string> SelectedComponents { get; set; } = [];
    public Dictionary<string, string> ComponentFiles { get; set; } = [];
    public List<string> SelectedGames { get; set; } = [];
    public bool OnboardingCompleted { get; set; }
}

public sealed class WizardComponent : ObservableObject
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public string Purpose { get; init; } = "";
    public string SourceUrl { get; init; } = "";
    public string SourceLabel { get; init; } = "Sin fuente verificada";
    public string CompatibilityNote { get; init; } = "";
    public bool Required { get; init; }
    private bool _selected; public bool Selected { get => _selected; set => Set(ref _selected, value); }
    private string _localFile = ""; public string LocalFile { get => _localFile; set => Set(ref _localFile, value); }
}

public sealed class CatalogGame : ObservableObject
{
    public required string Id { get; init; }
    public required string Title { get; init; }
    public required string FolderName { get; init; }
    public string Layout { get; init; } = "SingleDisc";
    public string Note { get; init; } = "";
    private bool _selected; public bool Selected { get => _selected; set => Set(ref _selected, value); }
}

public sealed class WizardCatalog { public List<CatalogGame> Games { get; set; } = []; public List<WizardComponent> Components { get; set; } = []; }

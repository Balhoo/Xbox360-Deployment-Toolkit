using System.Collections.ObjectModel;
using Microsoft.Win32;
using Xbox360DeploymentToolkit.Models;
using Xbox360DeploymentToolkit.Services;

namespace Xbox360DeploymentToolkit.ViewModels;
public sealed class MainViewModel : ObservableObject
{
    public event Func<string, string, Task<bool>>? ConfirmationRequested;
    public event Action<string, string>? NotificationRequested;
    private readonly string _base = AppContext.BaseDirectory;
    private readonly string _dataRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Xbox360DeploymentToolkit");
    private readonly JsonStore _json = new(); private readonly DriveService _drives = new(); private readonly FtpService _ftp = new(); private readonly ValidationService _validator = new(); private readonly ReportService _reports = new();
    private readonly AppLogger _log; private readonly CredentialStore _credentials; private ToolkitSettings _settings;
    public ObservableCollection<PreparationItem> PreparationItems { get; } = []; public ObservableCollection<ChecklistStep> Steps { get; } = []; public ObservableCollection<GameItem> Games { get; } = []; public ObservableCollection<CatalogGame> GameCatalog { get; } = []; public ObservableCollection<DriveInfoDto> Drives { get; } = []; public ObservableCollection<FtpEntry> FtpEntries { get; } = []; public ObservableCollection<AuditRecord> Audit { get; } = [];
    private string _status = "Listo"; public string Status { get => _status; set => Set(ref _status, value); }
    private string _localRoot = ""; public string LocalRoot { get => _localRoot; set => Set(ref _localRoot, value); }
    private string _host = "192.168.1.25"; public string Host { get => _host; set => Set(ref _host, value); }
    private string _ftpPath = "/Hdd1"; public string FtpPath { get => _ftpPath; set => Set(ref _ftpPath, value); }
    private string _username = "xbox"; public string Username { get => _username; set => Set(ref _username, value); }
    private string _password = "xbox"; public string Password { get => _password; set => Set(ref _password, value); }
    private bool _rememberCredential; public bool RememberCredential { get => _rememberCredential; set => Set(ref _rememberCredential, value); }
    private bool _dryRun = true; public bool DryRun { get => _dryRun; set { if (Set(ref _dryRun, value)) Status = value ? "Modo simulación activo" : "Modo escritura activo: revisa cada destino"; } }
    private DriveInfoDto? _selectedDrive; public DriveInfoDto? SelectedDrive { get => _selectedDrive; set => Set(ref _selectedDrive, value); }
    private GameItem? _selectedGame; public GameItem? SelectedGame { get => _selectedGame; set => Set(ref _selectedGame, value); }
    private ChecklistStep? _selectedStep; public ChecklistStep? SelectedStep { get => _selectedStep; set => Set(ref _selectedStep, value); }
    private string _ftpConnectionStatus = "Sin conectar"; public string FtpConnectionStatus { get => _ftpConnectionStatus; set => Set(ref _ftpConnectionStatus, value); }
    private string _uploadFile = ""; public string UploadFile { get => _uploadFile; set => Set(ref _uploadFile, value); }
    private double _progress; public double Progress { get => _progress; set => Set(ref _progress, value); }
    public double ChecklistProgress => Steps.Count == 0 ? 0 : Steps.Count(x => x.IsComplete) * 100d / Steps.Count;
    public double PreparationProgress { get { var required = PreparationItems.Where(x => x.Required).ToList(); return required.Count == 0 ? 0 : required.Count(x => x.IsReady) * 100d / required.Count; } }
    public bool IsPreparationComplete => PreparationItems.Where(x => x.Required).All(x => x.IsReady);
    public string InstallationMode { get; private set; } = "Instalación limpia";
    public bool IncludeXboxClassic { get; private set; }
    public bool IncludeEmulators { get; private set; }
    public RelayCommand RefreshDrivesCommand { get; } public RelayCommand ChooseRootCommand { get; } public RelayCommand ChooseUploadCommand { get; } public RelayCommand ValidateGamesCommand { get; } public RelayCommand ExportCommand { get; } public RelayCommand SaveProgressCommand { get; } public RelayCommand<PreparationItem> CompletePreparationCommand { get; } public RelayCommand<ChecklistStep> CompleteStepCommand { get; } public RelayCommand AddSelectedGamesCommand { get; }
    public AsyncCommand PrepareCommand { get; } public AsyncCommand BrowseFtpCommand { get; } public AsyncCommand UploadCommand { get; } public AsyncCommand VerifyUploadCommand { get; }

    public MainViewModel()
    {
        Directory.CreateDirectory(_dataRoot); _log = new(_dataRoot); _credentials = new(_dataRoot);
        _settings = _json.Load<ToolkitSettings>(Path.Combine(_base, "Configuration", "settings.json")); DryRun = _settings.DryRun; LocalRoot = _settings.DefaultLocalRoot; FtpPath = _settings.DefaultRemoteRoot;
        LoadProfileAndPlan();
        LoadPreparationItems();
        foreach (var game in _json.Load<WizardCatalog>(Path.Combine(_base, "Configuration", "wizard-catalog.json")).Games) GameCatalog.Add(game);
        LoadProgress(); var saved = _credentials.Load(); if (saved is { } c) { Username = c.User; Password = c.Password; RememberCredential = true; }
        RefreshDrivesCommand = new(RefreshDrives); PrepareCommand = new(Prepare, Fail); ChooseRootCommand = new(ChooseRoot); ChooseUploadCommand = new(ChooseUpload); ValidateGamesCommand = new(ValidateGames); ExportCommand = new(Export); SaveProgressCommand = new(() => SaveProgress());
        CompletePreparationCommand = new(CompletePreparation); CompleteStepCommand = new(CompleteStep); AddSelectedGamesCommand = new(AddSelectedGames);
        BrowseFtpCommand = new(BrowseFtp, Fail); UploadCommand = new(Upload, Fail); VerifyUploadCommand = new(VerifyUpload, Fail); RefreshDrives(); UpdateActiveStep(); SelectedGame = Games.FirstOrDefault();
    }
    private void PreparationChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) { if (e.PropertyName is nameof(PreparationItem.IsReady) or nameof(PreparationItem.Notes)) { Raise(nameof(PreparationProgress)); Raise(nameof(IsPreparationComplete)); SaveProgress(false); } }
    private void CompletePreparation(PreparationItem item) { item.IsReady = true; Status = $"Preparación completada: {item.Name}"; AddAudit("Preparación", item.Name, "OK"); }
    private void CompleteStep(ChecklistStep step) { if (!step.IsActive || step.IsComplete) return; step.IsComplete = true; AddAudit("Deployment", step.Title, "OK"); UpdateActiveStep(); SaveProgress(false); }
    private void UpdateActiveStep() { foreach (var step in Steps) step.IsActive = false; var active = Steps.FirstOrDefault(x => !x.IsComplete); if (active is not null) active.IsActive = true; SelectedStep = active ?? Steps.LastOrDefault(); Raise(nameof(ChecklistProgress)); }
    private void LoadProfileAndPlan()
    {
        var profile = _json.Load<DeploymentProfile>(AppPaths.ProfilePath); InstallationMode = profile.InstallationMode; IncludeXboxClassic = profile.IncludeXboxClassic; IncludeEmulators = profile.IncludeEmulators; if (string.IsNullOrWhiteSpace(LocalRoot) && profile.UsePcLibrary) LocalRoot = profile.PcLibraryPath;
        var dirty = !InstallationMode.Contains("limpia", StringComparison.OrdinalIgnoreCase);
        var definitions = dirty ? ExistingInstallationSteps() : CleanInstallationSteps();
        foreach (var step in definitions) { step.PropertyChanged += (_, e) => { if (e.PropertyName is nameof(ChecklistStep.IsComplete) or nameof(ChecklistStep.Notes)) { Raise(nameof(ChecklistProgress)); SaveProgress(false); } }; Steps.Add(step); }
    }
    private void LoadPreparationItems() { PreparationItems.Clear(); foreach (var item in _json.Load<PreparationDefinition>(Path.Combine(_base, "Configuration", "preparation.json")).Items) { if (item.Id == "classic-compat" && !IncludeXboxClassic || item.Id == "emulators" && !IncludeEmulators) continue; item.PropertyChanged += PreparationChanged; PreparationItems.Add(item); } }
    public void ReloadProfileAndPlan() { Steps.Clear(); LoadProfileAndPlan(); LoadPreparationItems(); LoadProgress(); UpdateActiveStep(); Raise(nameof(InstallationMode)); Raise(nameof(IncludeXboxClassic)); Raise(nameof(IncludeEmulators)); Raise(nameof(PreparationProgress)); Raise(nameof(IsPreparationComplete)); }
    private IEnumerable<ChecklistStep> CleanInstallationSteps()
    {
        yield return Step("clean-01", "1 de 8", "Conectar y validar acceso a la consola", "Inicia el gestor disponible y comprueba FTP o acceso por USB sin escribir todavía.");
        yield return Step("clean-02", "2 de 8", "Instalar el gestor de archivos inicial", "Coloca XeXMenu o el gestor aportado por el usuario siguiendo el formato compatible con su consola.");
        yield return Step("clean-03", "3 de 8", "Instalar Aurora", "Copia el paquete validado a Hdd1:\\Aurora y comprueba que inicia manualmente.");
        yield return Step("clean-04", "4 de 8", "Configurar rutas y escaneo de Aurora", "Añade las rutas de Xbox 360 seleccionadas y ejecuta un escaneo controlado.");
        yield return Step("clean-05", "5 de 8", "Configurar DashLaunch", "Conserva un launch.ini recuperable y configura Aurora como dashboard solamente después de probarlo.");
        yield return Step("clean-06", "6 de 8", "Transferir juegos Xbox 360", "Transfiere únicamente los títulos seleccionados y valida estructura, discos y contenido adicional.");
        if (IncludeXboxClassic) yield return Step("clean-07c", "7 de 8", "Configurar Xbox clásico", "Prepara compatibilidad y valida cada título seleccionado de forma independiente.");
        if (IncludeEmulators) yield return Step("clean-07e", "7 de 8", "Configurar emuladores", "Instala cada emulador seleccionado y organiza sus juegos por consola.");
        yield return Step("clean-08", "8 de 8", "Validación final y reporte", "Prueba arranque, espacio, FTP, títulos y rutas; exporta el reporte de la sesión.");
    }
    private IEnumerable<ChecklistStep> ExistingInstallationSteps()
    {
        yield return Step("existing-01", "1 de 6", "Revalidar el estado actual", "Compara dashboard, rutas, plugins, espacio y acceso FTP con la auditoría guardada.");
        yield return Step("existing-02", "2 de 6", "Respaldar configuración existente", "Copia launch.ini, bases de datos, carátulas y configuraciones que deban conservarse.");
        yield return Step("existing-03", "3 de 6", "Reparar o actualizar Aurora", "Sustituye solo los componentes necesarios y conserva los datos compatibles.");
        yield return Step("existing-04", "4 de 6", "Conciliar DashLaunch y rutas", "Revisa plugins, ruta de inicio y escaneos; elimina duplicados únicamente tras verificar el respaldo.");
        yield return Step("existing-05", "5 de 6", "Integrar contenido seleccionado", "Transfiere títulos nuevos y valida multidisco, contenido adicional y espacio disponible.");
        yield return Step("existing-06", "6 de 6", "Pruebas de regresión y reporte", "Comprueba que lo existente siga funcionando y documenta todos los cambios.");
    }
    private static ChecklistStep Step(string id, string phase, string title, string instructions) => new() { Id = id, Phase = phase, Title = title, Instructions = instructions, Warning = "Completa y verifica este paso antes de continuar." };
    public void AddSelectedGames()
    {
        foreach (var source in GameCatalog.Where(x => x.Selected)) { if (Games.Any(x => x.Id == source.Id)) continue; var item = new GameItem { Id = source.Id, Title = source.Title, Type = source.Layout, HasDlc = source.HasDlc, ContentFormat = source.FormatGuidance, Notes = source.Note, RequiredPaths = BuildGamePaths(source), Platform = "Xbox 360" }; Games.Add(item); CreateGameFolders(item); source.Selected = false; }
        SelectedGame ??= Games.FirstOrDefault(); SaveProgress(false); Raise(nameof(Games));
    }
    public void AddCustomGame(string title, string layout, bool hasDlc)
    {
        if (string.IsNullOrWhiteSpace(title)) return; var id = "custom-" + Guid.NewGuid().ToString("N"); var item = new GameItem { Id = id, Title = title.Trim(), Type = layout, HasDlc = hasDlc, ContentFormat = "Carpeta extraída con default.xex o formato GOD; DLC en Content/0000000000000000", Notes = "Título agregado manualmente. Verifica Title ID, Media ID y estructura antes de transferir.", RequiredPaths = ["Games/" + title.Trim() + "/default.xex"] }; Games.Add(item); CreateGameFolders(item); SelectedGame = item; SaveProgress(false);
    }
    private static string[] BuildGamePaths(CatalogGame game) => game.Layout == "MultiDisc" ? [$"Games/{game.FolderName}/Disc 1/default.xex", $"Games/{game.FolderName}/Disc 2/default.xex"] : [$"Games/{game.FolderName}/default.xex"];
    private void CreateGameFolders(GameItem game)
    {
        if (string.IsNullOrWhiteSpace(LocalRoot)) return; var safe = string.Concat(game.Title.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c)); var root = Path.Combine(LocalRoot, "03_Games", "Xbox360", safe); Directory.CreateDirectory(Path.Combine(root, "Game")); Directory.CreateDirectory(Path.Combine(root, "DLC")); Directory.CreateDirectory(Path.Combine(root, "Extras")); if (game.Type == "MultiDisc") { Directory.CreateDirectory(Path.Combine(root, "Game", "Disc 1")); Directory.CreateDirectory(Path.Combine(root, "Game", "Disc 2")); }
    }
    private void AddAudit(string category, string message, string result) { Audit.Add(new(DateTime.Now, category, message, result)); _log.Write(result == "OK" ? "INFO" : "WARN", $"{category}: {message} [{result}]"); }
    private void RefreshDrives() { Drives.Clear(); foreach (var d in _drives.List()) Drives.Add(d); Status = $"{Drives.Count} unidades detectadas"; }
    private void ChooseRoot() { var dialog = new OpenFolderDialog { Title = "Selecciona la raíz local o unidad de destino" }; if (dialog.ShowDialog() == true) LocalRoot = dialog.FolderName; }
    private void ChooseUpload() { var dialog = new OpenFileDialog { Title = "Selecciona el archivo que deseas transferir" }; if (dialog.ShowDialog() == true) UploadFile = dialog.FileName; }
    private async Task Prepare()
    {
        var root = !string.IsNullOrWhiteSpace(LocalRoot) ? LocalRoot : SelectedDrive?.Name ?? ""; if (string.IsNullOrWhiteSpace(root)) { Notify("Selecciona una unidad o carpeta.", "Destino requerido"); return; }
        if (!DryRun && !await Confirm("Confirmar preparación", $"Destino: {Path.GetFullPath(root)}\n\nSe crearán únicamente carpetas. No se borrará ni formateará nada.")) return;
        var result = _drives.PrepareFolders(root, ["Aurora", "Games", "Content", "Emulators", "Homebrew", "Compatibility"], DryRun); Status = string.Join(" | ", result); AddAudit("Unidad", root, DryRun ? "SIMULADO" : "OK");
    }
    private async Task BrowseFtp() { SaveCredentials(); FtpEntries.Clear(); foreach (var e in await _ftp.ListAsync(Host, FtpPath, Username, Password)) FtpEntries.Add(e); FtpConnectionStatus = "Conectado"; Status = $"FTP conectado: {FtpEntries.Count} elementos"; AddAudit("FTP", $"Listado {FtpPath}", "OK"); }
    private async Task Upload()
    {
        if (!File.Exists(UploadFile)) throw new InvalidOperationException("Selecciona un archivo local válido."); var remote = FtpPath.TrimEnd('/') + "/" + Path.GetFileName(UploadFile);
        if (DryRun) { Status = $"SIMULAR carga: {UploadFile} -> {remote}"; AddAudit("FTP", Status, "SIMULADO"); return; }
        if (!await Confirm("Confirmar transferencia FTP", $"Archivo local: {UploadFile}\nDestino Xbox: {Host}:{remote}\n\nLa transferencia escribirá un archivo en la ruta remota.")) return;
        Progress = 0; await _ftp.UploadAsync(Host, remote, Username, Password, UploadFile, new Progress<double>(p => Progress = p), CancellationToken.None); Status = "Transferencia completada"; AddAudit("FTP", remote, "OK");
    }
    private async Task VerifyUpload() { if (!File.Exists(UploadFile)) throw new InvalidOperationException("Selecciona el archivo local transferido."); var remote = FtpPath.TrimEnd('/') + "/" + Path.GetFileName(UploadFile); var remoteSize = await _ftp.SizeAsync(Host, remote, Username, Password); var localSize = new FileInfo(UploadFile).Length; Status = remoteSize == localSize ? $"Verificado por tamaño: {localSize:N0} bytes" : $"No coincide: local {localSize:N0}, remoto {remoteSize:N0}"; AddAudit("Verificación FTP", remote, remoteSize == localSize ? "OK" : "ERROR"); }
    private void ValidateGames() { foreach (var game in Games) game.Validation = _validator.ValidateGame(game, LocalRoot); Status = "Validación local terminada"; AddAudit("Juegos", $"{Games.Count} manifiestos", "OK"); }
    private void SaveCredentials() { if (RememberCredential) _credentials.Save(Username, Password); else _credentials.Delete(); }
    private void SaveProgress(bool showStatus = true) { _json.Save(Path.Combine(_dataRoot, "progress.json"), new ProgressData { Completed = Steps.Where(x => x.IsComplete).Select(x => x.Id).ToList(), Notes = Steps.ToDictionary(x => x.Id, x => x.Notes), GameStates = Games.ToDictionary(x => x.Id, x => x.State), SelectedGames = Games.ToList(), Prepared = PreparationItems.Where(x => x.IsReady).Select(x => x.Id).ToList(), PreparationNotes = PreparationItems.ToDictionary(x => x.Id, x => x.Notes) }); if (showStatus) Status = "Progreso guardado automáticamente"; }
    private void LoadProgress() { var p = _json.Load<ProgressData>(Path.Combine(_dataRoot, "progress.json")); foreach (var s in Steps) { s.IsComplete = p.Completed.Contains(s.Id); if (p.Notes.TryGetValue(s.Id, out var n)) s.Notes = n; } foreach (var game in p.SelectedGames) if (Games.All(x => x.Id != game.Id)) Games.Add(game); foreach (var g in Games) if (p.GameStates.TryGetValue(g.Id, out var state)) g.State = state; foreach (var item in PreparationItems) { item.IsReady = p.Prepared.Contains(item.Id); if (p.PreparationNotes.TryGetValue(item.Id, out var n)) item.Notes = n; } }
    private void Export() { SaveProgress(); var folder = Path.IsPathRooted(_settings.ReportFolder) ? _settings.ReportFolder : Path.Combine(_dataRoot, _settings.ReportFolder); var path = _reports.Export(folder, PreparationItems, Steps, Games, Audit); Status = $"Reporte exportado: {path}"; AddAudit("Reporte", path, "OK"); }
    private async Task<bool> Confirm(string title, string message) => ConfirmationRequested is { } handler && await handler(title, message);
    private void Notify(string message, string title = "Xbox360 Deployment Toolkit") => NotificationRequested?.Invoke(title, message);
    private void Fail(Exception ex) { FtpConnectionStatus = "Error"; Status = ex.Message; AddAudit("Error", ex.Message, "ERROR"); Notify(ex.Message, "Error"); }
    public sealed class ProgressData { public List<string> Completed { get; set; } = []; public Dictionary<string,string> Notes { get; set; } = []; public Dictionary<string,string> GameStates { get; set; } = []; public List<GameItem> SelectedGames { get; set; } = []; public List<string> Prepared { get; set; } = []; public Dictionary<string,string> PreparationNotes { get; set; } = []; }
}

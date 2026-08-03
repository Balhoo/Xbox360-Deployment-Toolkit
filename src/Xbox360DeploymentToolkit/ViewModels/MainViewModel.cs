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
    public ObservableCollection<PreparationItem> PreparationItems { get; } = []; public ObservableCollection<ChecklistStep> Steps { get; } = []; public ObservableCollection<GameItem> Games { get; } = []; public ObservableCollection<DriveInfoDto> Drives { get; } = []; public ObservableCollection<FtpEntry> FtpEntries { get; } = []; public ObservableCollection<AuditRecord> Audit { get; } = [];
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
    public double PreparationProgress => PreparationItems.Count == 0 ? 0 : PreparationItems.Count(x => x.IsReady) * 100d / PreparationItems.Count;
    public RelayCommand RefreshDrivesCommand { get; } public RelayCommand ChooseRootCommand { get; } public RelayCommand ChooseUploadCommand { get; } public RelayCommand ValidateGamesCommand { get; } public RelayCommand ExportCommand { get; } public RelayCommand SaveProgressCommand { get; }
    public AsyncCommand PrepareCommand { get; } public AsyncCommand BrowseFtpCommand { get; } public AsyncCommand UploadCommand { get; } public AsyncCommand VerifyUploadCommand { get; }

    public MainViewModel()
    {
        Directory.CreateDirectory(_dataRoot); _log = new(_dataRoot); _credentials = new(_dataRoot);
        _settings = _json.Load<ToolkitSettings>(Path.Combine(_base, "Configuration", "settings.json")); DryRun = _settings.DryRun; LocalRoot = _settings.DefaultLocalRoot; FtpPath = _settings.DefaultRemoteRoot;
        foreach (var s in _json.Load<ProcedureDefinition>(Path.Combine(_base, "Configuration", "procedure.json")).Steps) { s.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(ChecklistStep.IsComplete)) Raise(nameof(ChecklistProgress)); }; Steps.Add(s); }
        foreach (var item in _json.Load<PreparationDefinition>(Path.Combine(_base, "Configuration", "preparation.json")).Items) { item.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(PreparationItem.IsReady)) Raise(nameof(PreparationProgress)); }; PreparationItems.Add(item); }
        foreach (var g in _json.Load<GamesDefinition>(Path.Combine(_base, "Configuration", "games.json")).Games) Games.Add(g);
        LoadProgress(); var saved = _credentials.Load(); if (saved is { } c) { Username = c.User; Password = c.Password; RememberCredential = true; }
        RefreshDrivesCommand = new(RefreshDrives); PrepareCommand = new(Prepare, Fail); ChooseRootCommand = new(ChooseRoot); ChooseUploadCommand = new(ChooseUpload); ValidateGamesCommand = new(ValidateGames); ExportCommand = new(Export); SaveProgressCommand = new(SaveProgress);
        BrowseFtpCommand = new(BrowseFtp, Fail); UploadCommand = new(Upload, Fail); VerifyUploadCommand = new(VerifyUpload, Fail); RefreshDrives(); SelectedStep = Steps.FirstOrDefault(); SelectedGame = Games.FirstOrDefault();
    }
    private void AddAudit(string category, string message, string result) { Audit.Add(new(DateTime.Now, category, message, result)); _log.Write(result == "OK" ? "INFO" : "WARN", $"{category}: {message} [{result}]"); }
    private void RefreshDrives() { Drives.Clear(); foreach (var d in _drives.List()) Drives.Add(d); Status = $"{Drives.Count} unidades detectadas"; }
    private void ChooseRoot() { var dialog = new OpenFolderDialog { Title = "Selecciona la raíz local o unidad de destino" }; if (dialog.ShowDialog() == true) LocalRoot = dialog.FolderName; }
    private void ChooseUpload() { var dialog = new OpenFileDialog { Title = "Selecciona un archivo legítimo para transferir" }; if (dialog.ShowDialog() == true) UploadFile = dialog.FileName; }
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
    private void SaveProgress() { _json.Save(Path.Combine(_dataRoot, "progress.json"), new ProgressData { Completed = Steps.Where(x => x.IsComplete).Select(x => x.Id).ToList(), Notes = Steps.ToDictionary(x => x.Id, x => x.Notes), GameStates = Games.ToDictionary(x => x.Id, x => x.State), Prepared = PreparationItems.Where(x => x.IsReady).Select(x => x.Id).ToList(), PreparationNotes = PreparationItems.ToDictionary(x => x.Id, x => x.Notes) }); Status = "Progreso guardado"; }
    private void LoadProgress() { var p = _json.Load<ProgressData>(Path.Combine(_dataRoot, "progress.json")); foreach (var s in Steps) { s.IsComplete = p.Completed.Contains(s.Id); if (p.Notes.TryGetValue(s.Id, out var n)) s.Notes = n; } foreach (var g in Games) if (p.GameStates.TryGetValue(g.Id, out var state)) g.State = state; foreach (var item in PreparationItems) { item.IsReady = p.Prepared.Contains(item.Id); if (p.PreparationNotes.TryGetValue(item.Id, out var n)) item.Notes = n; } }
    private void Export() { SaveProgress(); var folder = Path.IsPathRooted(_settings.ReportFolder) ? _settings.ReportFolder : Path.Combine(_dataRoot, _settings.ReportFolder); var path = _reports.Export(folder, PreparationItems, Steps, Games, Audit); Status = $"Reporte exportado: {path}"; AddAudit("Reporte", path, "OK"); }
    private async Task<bool> Confirm(string title, string message) => ConfirmationRequested is { } handler && await handler(title, message);
    private void Notify(string message, string title = "Xbox360 Deployment Toolkit") => NotificationRequested?.Invoke(title, message);
    private void Fail(Exception ex) { FtpConnectionStatus = "Error"; Status = ex.Message; AddAudit("Error", ex.Message, "ERROR"); Notify(ex.Message, "Error"); }
    public sealed class ProgressData { public List<string> Completed { get; set; } = []; public Dictionary<string,string> Notes { get; set; } = []; public Dictionary<string,string> GameStates { get; set; } = []; public List<string> Prepared { get; set; } = []; public Dictionary<string,string> PreparationNotes { get; set; } = []; }
}

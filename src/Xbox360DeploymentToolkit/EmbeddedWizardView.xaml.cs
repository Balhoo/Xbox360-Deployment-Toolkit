using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Xbox360DeploymentToolkit.Models;
using Xbox360DeploymentToolkit.Services;

namespace Xbox360DeploymentToolkit;
public partial class EmbeddedWizardView : UserControl
{
    private readonly Action _close;
    private readonly ObservableCollection<WizardComponent> _components = [];
    private readonly ObservableCollection<CatalogGame> _games = [];
    private bool _riskAccepted;
    private string _verificationDecision = "Verificación pendiente";
    private DeploymentProfile? _pendingProfile;
    private static readonly string[] Titles = ["Consola", "Alcance", "Almacenamiento", "Componentes", "Juegos", "Resumen"];

    public EmbeddedWizardView(Action close)
    {
        _close = close; InitializeComponent();
        var catalog = new JsonStore().Load<WizardCatalog>(Path.Combine(AppContext.BaseDirectory, "Configuration", "wizard-catalog.json"));
        foreach (var component in catalog.Components) { component.Selected = component.Required; _components.Add(component); }
        foreach (var game in catalog.Games) _games.Add(game);
        ComponentsGrid.ItemsSource = _components; GamesGrid.ItemsSource = _games;
        PcLibraryPathBox.Text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Xbox360 Library");
        Steps.SelectedIndex = 0; UpdateStep();
    }
    private static string Value(ComboBox box) => (box.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Sin confirmar";
    private void Back_Click(object sender, RoutedEventArgs e) { if (Steps.SelectedIndex > 0) { Steps.SelectedIndex--; UpdateStep(); } }
    private void Next_Click(object sender, RoutedEventArgs e)
    {
        if (Steps.SelectedIndex == 0 && !ConfirmRgh()) return;
        if (Steps.SelectedIndex == 2 && UseUsbCheck.IsChecked == true && (!int.TryParse(UsbCapacityBox.Text, out var gb) || gb <= 0)) { InlineStatus.Text = "Escribe una capacidad USB válida en GB."; return; }
        if (Steps.SelectedIndex < Titles.Length - 1) Steps.SelectedIndex++;
        if (Steps.SelectedIndex == Titles.Length - 1) BuildSummary();
        UpdateStep();
    }
    private bool ConfirmRgh()
    {
        var confirmation = Value(RghConfirmationBox); var verified = confirmation.StartsWith("Confirmado:");
        if (verified) { _riskAccepted = false; _verificationDecision = "RGH confirmado por software"; InlineStatus.Text = "RGH confirmado por evidencia de software."; return true; }
        if (_riskAccepted) return true; Gate.Visibility = Visibility.Visible; return false;
    }
    private void GateSafe_Click(object sender, RoutedEventArgs e) { _verificationDecision = "El usuario eligió verificación segura"; InlineStatus.Text = "Pendiente: arranca XeLL con Eject o analiza NAND + CPU Key en modo lectura."; Gate.Visibility = Visibility.Collapsed; }
    private void GateRisk_Click(object sender, RoutedEventArgs e) { _riskAccepted = true; _verificationDecision = "Continuación bajo responsabilidad con RGH no verificado"; Gate.Visibility = Visibility.Collapsed; Steps.SelectedIndex = 1; UpdateStep(); }
    private void GateCancel_Click(object sender, RoutedEventArgs e) => Gate.Visibility = Visibility.Collapsed;
    private void ChooseFolder_Click(object sender, RoutedEventArgs e) { var dialog = new OpenFolderDialog { Title = "Biblioteca y respaldos de Xbox 360" }; if (dialog.ShowDialog() == true) PcLibraryPathBox.Text = dialog.FolderName; }
    private void OpenSource_Click(object sender, RoutedEventArgs e) { if (ComponentsGrid.SelectedItem is not WizardComponent item) { InlineStatus.Text = "Selecciona un componente."; return; } if (string.IsNullOrWhiteSpace(item.SourceUrl)) { InlineStatus.Text = "Este componente no tiene una fuente original vigente verificada; aporta tu archivo si conoces su procedencia."; return; } Process.Start(new ProcessStartInfo(item.SourceUrl) { UseShellExecute = true }); }
    private void ChooseComponent_Click(object sender, RoutedEventArgs e) { if (ComponentsGrid.SelectedItem is not WizardComponent item) { InlineStatus.Text = "Selecciona un componente."; return; } var dialog = new OpenFileDialog { Title = $"Selecciona tu archivo para {item.Name}", CheckFileExists = true }; if (dialog.ShowDialog() == true) { item.LocalFile = dialog.FileName; item.Selected = true; } }
    private void AddGame_Click(object sender, RoutedEventArgs e) { var title = CustomGameBox.Text.Trim(); if (string.IsNullOrWhiteSpace(title)) return; _games.Add(new CatalogGame { Id = "custom-" + Guid.NewGuid().ToString("N"), Title = title, FolderName = SafeName(title), Layout = "Personalizado", Note = "Estructura por confirmar por el usuario.", Selected = true }); CustomGameBox.Clear(); }
    private DeploymentProfile BuildProfile() => new()
    {
        ConsoleModel = Value(ConsoleModelBox), InternalCapacity = Value(InternalCapacityBox), RghConfirmation = Value(RghConfirmationBox), KernelVersion = KernelVersionBox.Text.Trim(), NandStatus = Value(NandStatusBox), HackType = Value(HackTypeBox), LiveStatus = Value(LiveStatusBox), UnverifiedRiskAccepted = _riskAccepted, VerificationDecision = _verificationDecision,
        InstallationMode = Value(InstallationModeBox), ExistingComponents = ExistingComponentsBox.Text.Trim(), UseUsb = UseUsbCheck.IsChecked == true, UsbCapacityGb = int.TryParse(UsbCapacityBox.Text, out var gb) ? gb : 0, UseExternalStorage = UseExternalCheck.IsChecked == true, ExternalCapacity = ExternalCapacityBox.Text.Trim(), UsePcLibrary = UsePcCheck.IsChecked == true, PcLibraryPath = PcLibraryPathBox.Text.Trim(),
        SelectedComponents = _components.Where(x => x.Selected).Select(x => x.Id).ToList(), ComponentFiles = _components.Where(x => !string.IsNullOrWhiteSpace(x.LocalFile)).ToDictionary(x => x.Id, x => x.LocalFile), SelectedGames = _games.Where(x => x.Selected).Select(x => x.Title).ToList(), OnboardingCompleted = true
    };
    private void BuildSummary() { var p = BuildProfile(); var sb = new StringBuilder(); sb.AppendLine($"Consola        {p.ConsoleModel} · {p.InternalCapacity}").AppendLine($"Confirmación   {p.RghConfirmation}").AppendLine($"Decisión       {p.VerificationDecision}").AppendLine($"Kernel / NAND  {p.KernelVersion} · {p.NandStatus}").AppendLine($"Alcance        {p.InstallationMode}").AppendLine($"USB            {(p.UseUsb ? p.UsbCapacityGb + " GB" : "No")}").AppendLine($"Biblioteca     {(p.UsePcLibrary ? p.PcLibraryPath : "No")}").AppendLine().AppendLine("Componentes").AppendLine(string.Join("\n", _components.Where(x => x.Selected).Select(x => "  • " + x.Name))).AppendLine().AppendLine("Juegos").AppendLine(p.SelectedGames.Count == 0 ? "  • Ninguno por ahora" : string.Join("\n", p.SelectedGames.Select(x => "  • " + x))); SummaryText.Text = sb.ToString(); }
    private void Save_Click(object sender, RoutedEventArgs e)
    {
        _pendingProfile = BuildProfile();
        if (_pendingProfile.UsePcLibrary && !string.IsNullOrWhiteSpace(_pendingProfile.PcLibraryPath)) { ConfirmPath.Text = Path.GetFullPath(_pendingProfile.PcLibraryPath); Confirm.Visibility = Visibility.Visible; return; }
        Persist(_pendingProfile); Complete();
    }
    private void ConfirmCancel_Click(object sender, RoutedEventArgs e) { Confirm.Visibility = Visibility.Collapsed; InlineStatus.Text = "Creación cancelada; el perfil todavía no se guardó."; }
    private void ConfirmCreate_Click(object sender, RoutedEventArgs e) { if (_pendingProfile is null) return; CreateLibrary(_pendingProfile.PcLibraryPath); Persist(_pendingProfile); Confirm.Visibility = Visibility.Collapsed; Complete(); }
    private static void Persist(DeploymentProfile profile) { Directory.CreateDirectory(AppPaths.DataRoot); new JsonStore().Save(AppPaths.ProfilePath, profile); File.WriteAllText(AppPaths.OnboardingMarker, DateTime.Now.ToString("O")); }
    private void Complete() { InlineStatus.Text = "Perfil y plan guardados."; _close(); }
    private void CreateLibrary(string root) { var full = Path.GetFullPath(root); Directory.CreateDirectory(full); foreach (var folder in new[] { "00_Inbox", "01_Tools", "02_Backups", "03_Games", "04_Content", "05_Emulators", "06_Reports" }) Directory.CreateDirectory(Path.Combine(full, folder)); foreach (var game in _games.Where(x => x.Selected)) { var gameRoot = Path.Combine(full, "03_Games", SafeName(game.FolderName)); Directory.CreateDirectory(gameRoot); if (game.Layout == "MultiDisc") { Directory.CreateDirectory(Path.Combine(gameRoot, "Disc 1")); Directory.CreateDirectory(Path.Combine(gameRoot, "Disc 2")); } } File.WriteAllText(Path.Combine(full, "README-ADD-YOUR-OWN-FILES.txt"), "Xbox360 Deployment Toolkit no proporciona juegos, DLC, BIOS ni ROMs. Agrega únicamente archivos cuya procedencia y uso hayas decidido.\n"); }
    private static string SafeName(string value) { var cleaned = string.Concat(value.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c)).Trim(); return string.IsNullOrWhiteSpace(cleaned) ? "Juego sin nombre" : cleaned; }
    private void UpdateStep() { var index = Math.Clamp(Steps.SelectedIndex, 0, Titles.Length - 1); StepLabel.Text = $"Step {index + 1} of {Titles.Length}"; StepTitle.Text = Titles[index]; StepProgress.Value = (index + 1) * 100d / Titles.Length; BackButton.IsEnabled = index > 0; NextButton.Visibility = index == Titles.Length - 1 ? Visibility.Collapsed : Visibility.Visible; SaveButton.Visibility = index == Titles.Length - 1 ? Visibility.Visible : Visibility.Collapsed; }
}

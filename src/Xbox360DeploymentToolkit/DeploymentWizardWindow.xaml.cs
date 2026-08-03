using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Xbox360DeploymentToolkit.Models;
using Xbox360DeploymentToolkit.Services;

namespace Xbox360DeploymentToolkit;
public partial class DeploymentWizardWindow : Window
{
    public static string DataRoot => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Xbox360DeploymentToolkit");
    public static string ProfilePath => Path.Combine(DataRoot, "deployment-profile.json");
    public static string OnboardingMarker => Path.Combine(DataRoot, "onboarding.seen");
    private readonly ObservableCollection<WizardComponent> _components = [];
    private readonly ObservableCollection<CatalogGame> _games = [];

    public DeploymentWizardWindow()
    {
        InitializeComponent();
        var catalog = new JsonStore().Load<WizardCatalog>(Path.Combine(AppContext.BaseDirectory, "Configuration", "wizard-catalog.json"));
        foreach (var component in catalog.Components) { component.Selected = component.Required; _components.Add(component); }
        foreach (var game in catalog.Games) _games.Add(game);
        ComponentsGrid.ItemsSource = _components; GamesGrid.ItemsSource = _games;
        PcLibraryPathBox.Text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Xbox360 Library");
        WizardTabs.SelectedIndex = 0; UpdateNavigation();
    }

    private static string Value(ComboBox box) => (box.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Sin confirmar";
    private void Start_Click(object sender, RoutedEventArgs e) => WizardTabs.SelectedIndex = 1;
    private void Explore_Click(object sender, RoutedEventArgs e) { MarkSeen(); DialogResult = false; Close(); }
    private void Back_Click(object sender, RoutedEventArgs e) { if (WizardTabs.SelectedIndex > 0) WizardTabs.SelectedIndex--; }
    private void Next_Click(object sender, RoutedEventArgs e)
    {
        if (WizardTabs.SelectedIndex == 1 && !AssessConsole()) return;
        if (WizardTabs.SelectedIndex == 3 && UseUsbCheck.IsChecked == true && (!int.TryParse(UsbCapacityBox.Text, out var gb) || gb <= 0)) { MessageBox.Show("Escribe una capacidad USB válida en GB."); return; }
        if (WizardTabs.SelectedIndex < WizardTabs.Items.Count - 1) WizardTabs.SelectedIndex++;
        if (WizardTabs.SelectedIndex == WizardTabs.Items.Count - 1) BuildSummary();
    }
    private void WizardTabs_SelectionChanged(object sender, SelectionChangedEventArgs e) { if (IsLoaded) { if (WizardTabs.SelectedIndex == WizardTabs.Items.Count - 1) BuildSummary(); UpdateNavigation(); } }
    private void UpdateNavigation() { var index = WizardTabs.SelectedIndex; StepStatus.Text = index == 0 ? "Bienvenida" : $"Paso {index} de {WizardTabs.Items.Count - 1}"; BackButton.Visibility = index <= 0 ? Visibility.Collapsed : Visibility.Visible; NextButton.Visibility = index <= 0 || index == WizardTabs.Items.Count - 1 ? Visibility.Collapsed : Visibility.Visible; FinishButton.Visibility = index == WizardTabs.Items.Count - 1 ? Visibility.Visible : Visibility.Collapsed; }

    private bool AssessConsole()
    {
        var hack = Value(HackTypeBox); var live = Value(LiveStatusBox);
        if (hack == "Sin modificación (stock)") { CompatibilityText.Text = "Lo sentimos: este toolkit no instala ni crea una modificación RGH/JTAG. Puede usarse solo para inventario, respaldo y planificación; consulta a un técnico competente si deseas evaluar hardware."; CompatibilityBanner.Background = System.Windows.Media.Brushes.MistyRose; MessageBox.Show(CompatibilityText.Text, "Deployment no compatible", MessageBoxButton.OK, MessageBoxImage.Warning); return false; }
        if (hack == "Sin confirmar") { CompatibilityText.Text = "El tipo de modificación debe auditarse antes de escribir en la consola. Puedes continuar para generar un plan, pero las operaciones quedarán condicionadas a esa verificación."; CompatibilityBanner.Background = System.Windows.Media.Brushes.LemonChiffon; }
        else { CompatibilityText.Text = $"El flujo local es compatible en principio con {hack}. Debe verificarse kernel, arranque de XeLL y dashboard antes de escribir archivos."; CompatibilityBanner.Background = System.Windows.Media.Brushes.Honeydew; }
        if (live == "Baneada") CompatibilityText.Text += " La consola reportada como baneada puede seguir usando funciones locales; los servicios de Xbox Live no forman parte del deployment.";
        else CompatibilityText.Text += " No conectes una consola modificada a Xbox Live basándote en este diagnóstico; el toolkit no garantiza evitar sanciones.";
        return true;
    }
    private void InstallationMode_Changed(object sender, SelectionChangedEventArgs e) { if (!IsLoaded) return; var mode = Value(InstallationModeBox); ExistingLabel.Visibility = mode == "Instalación limpia" ? Visibility.Collapsed : Visibility.Visible; ExistingComponentsBox.Visibility = ExistingLabel.Visibility; ModeExplanation.Text = mode switch { "Instalación limpia" => "Se generará el procedimiento completo: auditoría, respaldo, staging, dashboard, FTP, juegos y pruebas.", "Migración de dashboard o almacenamiento" => "Se inventariará lo existente, se respaldará y luego se planificará la migración sin asumir rutas.", "Reparación / completar instalación existente" => "Se validarán componentes y se crearán únicamente los pasos faltantes.", _ => "No se prepararán cambios: se priorizarán inventario, validación y reporte." }; }
    private void ChoosePcFolder_Click(object sender, RoutedEventArgs e) { var dialog = new OpenFolderDialog { Title = "Biblioteca y respaldos de Xbox 360" }; if (dialog.ShowDialog() == true) PcLibraryPathBox.Text = dialog.FolderName; }
    private void OpenSource_Click(object sender, RoutedEventArgs e) { if (ComponentsGrid.SelectedItem is not WizardComponent item) { MessageBox.Show("Selecciona un componente."); return; } if (string.IsNullOrWhiteSpace(item.SourceUrl)) { MessageBox.Show("No existe una fuente original vigente verificada para este elemento. Proporciona tu propio archivo únicamente si conoces su procedencia."); return; } Process.Start(new ProcessStartInfo(item.SourceUrl) { UseShellExecute = true }); }
    private void ChooseComponentFile_Click(object sender, RoutedEventArgs e) { if (ComponentsGrid.SelectedItem is not WizardComponent item) { MessageBox.Show("Selecciona un componente."); return; } var dialog = new OpenFileDialog { Title = $"Selecciona tu archivo para {item.Name}", CheckFileExists = true }; if (dialog.ShowDialog() == true) { item.LocalFile = dialog.FileName; item.Selected = true; } }
    private void AddCustomGame_Click(object sender, RoutedEventArgs e) { var title = CustomGameBox.Text.Trim(); if (string.IsNullOrWhiteSpace(title)) return; var safe = SafeName(title); _games.Add(new CatalogGame { Id = "custom-" + Guid.NewGuid().ToString("N"), Title = title, FolderName = safe, Layout = "Personalizado", Note = "Estructura por confirmar por el usuario.", Selected = true }); CustomGameBox.Clear(); }

    private DeploymentProfile BuildProfile() => new()
    {
        ConsoleModel = Value(ConsoleModelBox), InternalCapacity = Value(InternalCapacityBox), HackType = Value(HackTypeBox), LiveStatus = Value(LiveStatusBox), InstallationMode = Value(InstallationModeBox), ExistingComponents = ExistingComponentsBox.Text.Trim(),
        UseUsb = UseUsbCheck.IsChecked == true, UsbCapacityGb = int.TryParse(UsbCapacityBox.Text, out var gb) ? gb : 0, UseExternalStorage = UseExternalCheck.IsChecked == true, ExternalCapacity = ExternalCapacityBox.Text.Trim(), UsePcLibrary = UsePcCheck.IsChecked == true, PcLibraryPath = PcLibraryPathBox.Text.Trim(),
        SelectedComponents = _components.Where(x => x.Selected).Select(x => x.Id).ToList(), ComponentFiles = _components.Where(x => !string.IsNullOrWhiteSpace(x.LocalFile)).ToDictionary(x => x.Id, x => x.LocalFile), SelectedGames = _games.Where(x => x.Selected).Select(x => x.Title).ToList(), OnboardingCompleted = true
    };
    private void BuildSummary() { var p = BuildProfile(); var sb = new StringBuilder(); sb.AppendLine($"Consola: {p.ConsoleModel} · {p.InternalCapacity}").AppendLine($"Modificación: {p.HackType} · Live: {p.LiveStatus}").AppendLine($"Alcance: {p.InstallationMode}").AppendLine($"USB: {(p.UseUsb ? p.UsbCapacityGb + " GB" : "No")}").AppendLine($"Externo: {(p.UseExternalStorage ? p.ExternalCapacity : "No")}").AppendLine($"Biblioteca PC: {(p.UsePcLibrary ? p.PcLibraryPath : "No")}").AppendLine().AppendLine("Componentes:").AppendLine(string.Join("\n", _components.Where(x => x.Selected).Select(x => "• " + x.Name))).AppendLine().AppendLine("Juegos:").AppendLine(p.SelectedGames.Count == 0 ? "• Ninguno por ahora" : string.Join("\n", p.SelectedGames.Select(x => "• " + x))); SummaryText.Text = sb.ToString(); }
    private void Finish_Click(object sender, RoutedEventArgs e)
    {
        var profile = BuildProfile(); Directory.CreateDirectory(DataRoot); new JsonStore().Save(ProfilePath, profile); MarkSeen();
        if (profile.UsePcLibrary && !string.IsNullOrWhiteSpace(profile.PcLibraryPath) && MessageBox.Show($"¿Crear la estructura del deployment en esta carpeta?\n\n{Path.GetFullPath(profile.PcLibraryPath)}\n\nSolo se crearán carpetas vacías y un aviso; no se copiará contenido.", "Confirmar estructura", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) CreateLibrary(profile.PcLibraryPath);
        MessageBox.Show("Perfil y plan guardados. Puedes cambiarlo después desde “Asistente inicial”.", "Deployment listo", MessageBoxButton.OK, MessageBoxImage.Information); DialogResult = true; Close();
    }
    private void CreateLibrary(string root)
    {
        var fullRoot = Path.GetFullPath(root); Directory.CreateDirectory(fullRoot); foreach (var folder in new[] { "00_Inbox", "01_Tools", "02_Backups", "03_Games", "04_Content", "05_Emulators", "06_Reports" }) Directory.CreateDirectory(Path.Combine(fullRoot, folder));
        foreach (var game in _games.Where(x => x.Selected)) { var gameRoot = Path.Combine(fullRoot, "03_Games", SafeName(game.FolderName)); Directory.CreateDirectory(gameRoot); if (game.Layout == "MultiDisc") { Directory.CreateDirectory(Path.Combine(gameRoot, "Disc 1")); Directory.CreateDirectory(Path.Combine(gameRoot, "Disc 2")); } }
        File.WriteAllText(Path.Combine(fullRoot, "README-ADD-YOUR-OWN-FILES.txt"), "Xbox360 Deployment Toolkit no proporciona juegos, DLC, BIOS ni ROMs. Agrega únicamente los archivos que decidas y puedas utilizar legítimamente.\n");
    }
    private static string SafeName(string value) { var cleaned = string.Concat(value.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c)).Trim(); return string.IsNullOrWhiteSpace(cleaned) ? "Juego sin nombre" : cleaned; }
    private static void MarkSeen() { Directory.CreateDirectory(DataRoot); File.WriteAllText(OnboardingMarker, DateTime.Now.ToString("O")); }
}

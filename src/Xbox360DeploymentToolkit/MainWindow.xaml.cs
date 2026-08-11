using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Xbox360DeploymentToolkit.ViewModels;

namespace Xbox360DeploymentToolkit;

public partial class MainWindow : Window
{
    private TaskCompletionSource<bool>? _confirmation;
    private readonly DispatcherTimer _toastTimer = new() { Interval = TimeSpan.FromSeconds(5) };
    private const int WizardPageIndex = 9;

    public MainWindow()
    {
        InitializeComponent();
        _toastTimer.Tick += (_, _) => { _toastTimer.Stop(); Toast.Visibility = Visibility.Collapsed; };
        var viewModel = new MainViewModel();
        viewModel.ConfirmationRequested += RequestConfirmation;
        viewModel.NotificationRequested += ShowToast;
        DataContext = viewModel;
        FtpPasswordBox.Password = viewModel.Password;

        WelcomePage.StartPreparationRequested += (_, _) => ShowWizard();
        WelcomePage.SkipPreparationRequested += (_, _) => SkipPreparation();
        WelcomePage.CloseRequested += (_, _) => Close();
        DeploymentWizard.Completed += (_, _) => { viewModel.ReloadProfileAndPlan(); ShowDashboard(); };
        if (File.Exists(AppPaths.WelcomeMarker)) ShowDashboard();
        else ApplyWelcomeWindowSize();
    }

    private void Navigation_Checked(object sender, RoutedEventArgs e)
    {
        if (PageTabs is not null && sender is FrameworkElement element && int.TryParse(element.Tag?.ToString(), out var index))
            PageTabs.SelectedIndex = index;
    }

    private void QuickNavigate_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement element && int.TryParse(element.Tag?.ToString(), out var index))
            PageTabs.SelectedIndex = index;
    }

    private void OpenWizard_Click(object sender, RoutedEventArgs e) => ShowWizard();
    private void CloseWizard_Click(object sender, RoutedEventArgs e) => ShowDashboard();
    private void OpenGameCatalog_Click(object sender, RoutedEventArgs e) => GameCatalogOverlay.Visibility = Visibility.Visible;
    private void CloseGameCatalog_Click(object sender, RoutedEventArgs e) => GameCatalogOverlay.Visibility = Visibility.Collapsed;
    private void AddSelectedGames_Click(object sender, RoutedEventArgs e) { if (DataContext is MainViewModel vm) vm.AddSelectedGames(); GameCatalogOverlay.Visibility = Visibility.Collapsed; }
    private void AddCustomGame_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel vm) return;
        var layout = (CustomGameLayoutBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "SingleDisc";
        vm.AddCustomGame(CustomGameTitleBox.Text, layout, CustomGameDlcCheck.IsChecked == true);
        CustomGameTitleBox.Clear(); CustomGameDlcCheck.IsChecked = false;
    }

    private void ShowWizard()
    {
        MarkWelcomeSeen();
        ApplyWorkspaceWindowSize();
        RootPages.SelectedIndex = 1;
        PageTabs.SelectedIndex = WizardPageIndex;
    }

    private void SkipPreparation()
    {
        MarkWelcomeSeen();
        ShowDashboard();
    }

    private static void MarkWelcomeSeen()
    {
        Directory.CreateDirectory(AppPaths.DataRoot);
        File.WriteAllText(AppPaths.WelcomeMarker, DateTime.Now.ToString("O"));
    }

    private void ShowDashboard()
    {
        ApplyWorkspaceWindowSize();
        RootPages.SelectedIndex = 1;
        PageTabs.SelectedIndex = 0;
    }

    private void ApplyWelcomeWindowSize()
    {
        RootPages.SelectedIndex = 0;
        MinWidth = 800;
        MinHeight = 600;
        Width = 800;
        Height = 600;
        CenterOnWorkArea();
    }

    private void ApplyWorkspaceWindowSize()
    {
        MinWidth = 1200;
        MinHeight = 720;
        if (WindowState == WindowState.Normal)
        {
            Width = Math.Min(1440, SystemParameters.WorkArea.Width);
            Height = Math.Min(900, SystemParameters.WorkArea.Height);
            CenterOnWorkArea();
        }
    }

    private void CenterOnWorkArea()
    {
        Left = SystemParameters.WorkArea.Left + Math.Max(0, (SystemParameters.WorkArea.Width - Width) / 2);
        Top = SystemParameters.WorkArea.Top + Math.Max(0, (SystemParameters.WorkArea.Height - Height) / 2);
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2) ToggleMaximize();
        else DragMove();
    }

    private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
    private void Maximize_Click(object sender, RoutedEventArgs e) => ToggleMaximize();
    private void WindowClose_Click(object sender, RoutedEventArgs e) => Close();
    private void ToggleMaximize() => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

    private void FtpPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm) vm.Password = FtpPasswordBox.Password;
    }

    private Task<bool> RequestConfirmation(string title, string message)
    {
        _confirmation = new TaskCompletionSource<bool>();
        ConfirmationTitle.Text = title;
        ConfirmationMessage.Text = message;
        ConfirmationOverlay.Visibility = Visibility.Visible;
        return _confirmation.Task;
    }

    private void ConfirmationCancel_Click(object sender, RoutedEventArgs e)
    {
        ConfirmationOverlay.Visibility = Visibility.Collapsed;
        _confirmation?.TrySetResult(false);
        _confirmation = null;
    }

    private void ConfirmationAccept_Click(object sender, RoutedEventArgs e)
    {
        ConfirmationOverlay.Visibility = Visibility.Collapsed;
        _confirmation?.TrySetResult(true);
        _confirmation = null;
    }

    private void ShowToast(string title, string message)
    {
        ToastTitle.Text = title;
        ToastMessage.Text = message;
        Toast.Visibility = Visibility.Visible;
        _toastTimer.Stop();
        _toastTimer.Start();
    }

    private void ToastClose_Click(object sender, RoutedEventArgs e) { _toastTimer.Stop(); Toast.Visibility = Visibility.Collapsed; }
}

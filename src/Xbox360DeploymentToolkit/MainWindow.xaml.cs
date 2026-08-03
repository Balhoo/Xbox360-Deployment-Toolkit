using System.Windows;
using Xbox360DeploymentToolkit.ViewModels;
namespace Xbox360DeploymentToolkit;
public partial class MainWindow : Window
{
    private TaskCompletionSource<bool>? _confirmation;
    public MainWindow() { InitializeComponent(); var viewModel = new MainViewModel(); viewModel.ConfirmationRequested += RequestConfirmation; viewModel.NotificationRequested += ShowToast; DataContext = viewModel; FtpPasswordBox.Password = viewModel.Password; Loaded += (_, _) => { if (!File.Exists(AppPaths.OnboardingMarker)) OpenWizard(); }; }
    private void Navigation_Checked(object sender, RoutedEventArgs e) { if (PageTabs is not null && sender is FrameworkElement element && int.TryParse(element.Tag?.ToString(), out var index)) PageTabs.SelectedIndex = index; }
    private void QuickNavigate_Click(object sender, RoutedEventArgs e) { if (sender is FrameworkElement element && int.TryParse(element.Tag?.ToString(), out var index)) PageTabs.SelectedIndex = index; }
    private void OpenWizard_Click(object sender, RoutedEventArgs e) => OpenWizard();
    private void CloseWizard_Click(object sender, RoutedEventArgs e) => WizardOverlay.Visibility = Visibility.Collapsed;
    private void OpenWizard() { WizardContent.Content = new EmbeddedWizardView(CloseEmbeddedWizard); WizardOverlay.Visibility = Visibility.Visible; }
    private void CloseEmbeddedWizard() => WizardOverlay.Visibility = Visibility.Collapsed;
    private Task<bool> RequestConfirmation(string title, string message) { _confirmation = new TaskCompletionSource<bool>(); ConfirmationTitle.Text = title; ConfirmationMessage.Text = message; ConfirmationOverlay.Visibility = Visibility.Visible; return _confirmation.Task; }
    private void ConfirmationAccept_Click(object sender, RoutedEventArgs e) { ConfirmationOverlay.Visibility = Visibility.Collapsed; _confirmation?.TrySetResult(true); _confirmation = null; }
    private void ConfirmationCancel_Click(object sender, RoutedEventArgs e) { ConfirmationOverlay.Visibility = Visibility.Collapsed; _confirmation?.TrySetResult(false); _confirmation = null; }
    private void ShowToast(string title, string message) { ToastTitle.Text = title; ToastMessage.Text = message; Toast.Visibility = Visibility.Visible; }
    private void ToastClose_Click(object sender, RoutedEventArgs e) => Toast.Visibility = Visibility.Collapsed;
    private void FtpPasswordBox_PasswordChanged(object sender, RoutedEventArgs e) { if (DataContext is MainViewModel viewModel) viewModel.Password = FtpPasswordBox.Password; }
}

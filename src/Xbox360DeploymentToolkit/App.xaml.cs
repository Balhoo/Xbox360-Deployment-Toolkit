using System.Windows;
namespace Xbox360DeploymentToolkit;
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e); var main = new MainWindow(); MainWindow = main; main.Show();
        if (!File.Exists(DeploymentWizardWindow.OnboardingMarker)) new DeploymentWizardWindow { Owner = main }.ShowDialog();
    }
}

using System.Windows;
using Xbox360DeploymentToolkit.ViewModels;
namespace Xbox360DeploymentToolkit;
public partial class MainWindow : Window
{
    public MainWindow() { InitializeComponent(); DataContext = new MainViewModel(); }
}

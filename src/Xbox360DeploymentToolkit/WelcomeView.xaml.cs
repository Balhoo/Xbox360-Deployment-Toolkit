using System.Windows;
using System.Windows.Controls;

namespace Xbox360DeploymentToolkit;

public partial class WelcomeView : UserControl
{
    public event EventHandler? StartPreparationRequested;
    public event EventHandler? SkipPreparationRequested;
    public event EventHandler? CloseRequested;

    public WelcomeView() => InitializeComponent();
    private void Start_Click(object sender, RoutedEventArgs e) => StartPreparationRequested?.Invoke(this, EventArgs.Empty);
    private void Skip_Click(object sender, RoutedEventArgs e) => SkipPreparationRequested?.Invoke(this, EventArgs.Empty);
    private void Close_Click(object sender, RoutedEventArgs e) => CloseRequested?.Invoke(this, EventArgs.Empty);
}

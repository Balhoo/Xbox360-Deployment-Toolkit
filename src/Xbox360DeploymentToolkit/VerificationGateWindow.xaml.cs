using System.Windows;
namespace Xbox360DeploymentToolkit;
public enum VerificationGateResult { Back, SafeVerification, AcceptRisk }
public partial class VerificationGateWindow : Window
{
    public VerificationGateResult Result { get; private set; } = VerificationGateResult.Back;
    public VerificationGateWindow() => InitializeComponent();
    private void Safe_Click(object sender, RoutedEventArgs e) { Result = VerificationGateResult.SafeVerification; DialogResult = true; }
    private void Risk_Click(object sender, RoutedEventArgs e) { Result = VerificationGateResult.AcceptRisk; DialogResult = true; }
    private void Back_Click(object sender, RoutedEventArgs e) { Result = VerificationGateResult.Back; DialogResult = false; }
}

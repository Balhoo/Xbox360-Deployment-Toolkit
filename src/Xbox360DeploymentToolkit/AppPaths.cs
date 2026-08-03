namespace Xbox360DeploymentToolkit;
public static class AppPaths
{
    public static string DataRoot => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Xbox360DeploymentToolkit");
    public static string ProfilePath => Path.Combine(DataRoot, "deployment-profile.json");
    public static string OnboardingMarker => Path.Combine(DataRoot, "onboarding.seen");
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xbox360DeploymentToolkit.Models;
using Xbox360DeploymentToolkit.Services;

namespace Xbox360DeploymentToolkit.Tests;

[TestClass]
public sealed class CoreServicesTests
{
    [TestMethod]
    public void MissingSettingsFileReturnsSafeDefaults()
    {
        var settings = new JsonStore().Load<ToolkitSettings>(Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json"));

        Assert.IsTrue(settings.DryRun);
        Assert.AreEqual("/Hdd1", settings.DefaultRemoteRoot);
    }

    [TestMethod]
    public void JsonStoreRoundTripsSettings()
    {
        using var workspace = new TemporaryWorkspace();
        var path = Path.Combine(workspace.Path, "settings.json");
        var store = new JsonStore();

        store.Save(path, new ToolkitSettings { DryRun = false, DefaultRemoteRoot = "/Usb0" });
        var loaded = store.Load<ToolkitSettings>(path);

        Assert.IsFalse(loaded.DryRun);
        Assert.AreEqual("/Usb0", loaded.DefaultRemoteRoot);
        Assert.IsFalse(File.Exists(path + ".tmp"));
    }

    [TestMethod]
    public void ValidationRejectsUnavailableRoot()
    {
        var game = NewGame("Games/Test/default.xex");

        var result = new ValidationService().ValidateGame(game, Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));

        Assert.AreEqual("Raíz local no disponible", result);
    }

    [TestMethod]
    public void ValidationAcceptsExpectedFile()
    {
        using var workspace = new TemporaryWorkspace();
        var relativePath = Path.Combine("Games", "Test", "default.xex");
        var fullPath = Path.Combine(workspace.Path, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        File.WriteAllText(fullPath, "fixture");

        var result = new ValidationService().ValidateGame(NewGame(relativePath), workspace.Path);

        Assert.AreEqual("Completo", result);
    }

    [TestMethod]
    public void ChecklistStatusTracksLifecycle()
    {
        var step = new ChecklistStep { Id = "backup", Phase = "Preparación", Title = "Crear respaldo" };

        Assert.AreEqual("Pendiente", step.Status);
        step.IsActive = true;
        Assert.AreEqual("En proceso", step.Status);
        step.IsComplete = true;
        Assert.AreEqual("Completado", step.Status);
        Assert.IsFalse(step.IsPending);
    }

    [TestMethod]
    public void ReportExportsJsonAndEscapedCsv()
    {
        using var workspace = new TemporaryWorkspace();
        var preparation = new[]
        {
            new PreparationItem { Id = "usb", Category = "Storage", Name = "USB, staging", Notes = "Listo \"para usar\"", IsReady = true }
        };

        var json = new ReportService().Export(workspace.Path, preparation, [], [], []);
        var csv = Directory.GetFiles(workspace.Path, "*.csv").Single();

        Assert.IsTrue(File.Exists(json));
        StringAssert.Contains(File.ReadAllText(csv), "\"USB, staging\"");
        StringAssert.Contains(File.ReadAllText(csv), "\"Listo \"\"para usar\"\"\"");
    }

    [TestMethod]
    public async Task Sha256UsesDeterministicUppercaseHex()
    {
        using var workspace = new TemporaryWorkspace();
        var path = Path.Combine(workspace.Path, "sample.bin");
        await File.WriteAllTextAsync(path, "XDT");

        var hash = await new ValidationService().Sha256Async(path);

        Assert.AreEqual(64, hash.Length);
        Assert.AreEqual(hash.ToUpperInvariant(), hash);
        Assert.AreEqual("C5F6BF9AC92E75C594075F609ACB213C4F02DFF6D02524F5566F712AA7B2602B", hash);
    }

    private static GameItem NewGame(string requiredPath) => new()
    {
        Id = "test",
        Title = "Test game",
        RequiredPaths = [requiredPath]
    };

    private sealed class TemporaryWorkspace : IDisposable
    {
        public string Path { get; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "xdt-tests", Guid.NewGuid().ToString("N"));

        public TemporaryWorkspace() => Directory.CreateDirectory(Path);

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}

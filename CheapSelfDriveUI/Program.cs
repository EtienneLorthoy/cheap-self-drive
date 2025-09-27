using CheapSelfDriveUI.Services;

namespace CheapSelfDriveUI;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        
        // Check banner configuration to determine which form to show
        var configManager = new Services.ConfigurationManager();
        var bannerPath = Path.Combine(Application.StartupPath, "banner-default.json");
        var bannerConfig = configManager.LoadBannerConfiguration(bannerPath);
        
        if (bannerConfig.InstallOnly)
        {
            Application.Run(new InstallOnlyForm());
        }
        else
        {
            Application.Run(new MainForm());
        }
    }
}
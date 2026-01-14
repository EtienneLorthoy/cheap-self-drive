using SelfDriveInstaller.Services;

namespace SelfDriveInstaller;

internal static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        RuntimeAdminHelper.RestartAsAdministrator(args);

        ApplicationConfiguration.Initialize();
    
        Application.Run(new InstallOnlyForm());
    }
}
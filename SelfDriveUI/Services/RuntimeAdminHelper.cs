using System.Diagnostics;
using System.Security.Principal;

namespace SelfDriveInstaller.Services;

internal class RuntimeAdminHelper
{
    internal static bool IsRunningAsAdministrator()
    {
        var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    internal static void RestartAsAdministrator(string[] args)
    {
        if (!IsRunningAsAdministrator())
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = Environment.ProcessPath,
                UseShellExecute = true,
                Verb = "runas", // This triggers UAC elevation
                Arguments = string.Join(" ", args)
            };

            try
            {
                Process.Start(processInfo);
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to elevate: {ex.Message}");
            }
        }
    }
}
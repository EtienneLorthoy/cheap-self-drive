using Newtonsoft.Json;

namespace SelfDriveInstaller.Models;

public class Config
{
    public string MountName { get; set; } = "UNVALID";
    public string DriveLetter { get; set; } = "X:";
    public string NASAddress { get; set; } = "Unvalid";
    public string NASUsername { get; set; } = "";
    public int NASPort { get; set; } = 22;
    public string NASAbsolutePath { get; set; } = "/";
    public string ShellType { get; set; } = "unix";
    public string InstallDirectory { get; set; } = "C:\\SelfDrive";
    public string VFSCacheDir { get; set; } = "C:\\SelfDrive\\VFS\\{MountName}";
    public string RcloneLogs { get; set; } = "C:\\SelfDrive\\VFS\\{MountName}.log";
    public string RcloneConfigDir { get; set; } = "{APPDATA}\\rclone";
    public AdvancedSettings AdvancedSettings { get; set; } = new();

    public string GetExpandedVFSCacheDir()
    {
        return VFSCacheDir.Replace("{MountName}", MountName)
                          .Replace("{APPDATA}", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
    }

    public string GetExpandedRcloneLogs()
    {
        return RcloneLogs.Replace("{MountName}", MountName)
                        .Replace("{APPDATA}", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
    }

    public string GetExpandedRcloneConfigDir()
    {
        return RcloneConfigDir.Replace("{MountName}", MountName)
                             .Replace("{APPDATA}", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
    }
}

public class AdvancedSettings
{
    public string CacheMode { get; set; } = "full";
    public string CacheMaxSize { get; set; } = "20G";
    public string CacheMaxAge { get; set; } = "168h";
    public string DirCacheTime { get; set; } = "30s";
    public string PollInterval { get; set; } = "15s";
    public string BufferSize { get; set; } = "16M";
    public string CacheMinFreeSpace { get; set; } = "20G";
}

public enum MountStatus
{
    NotConfigured,
    Configured,
    Installed,
    Running,
    Error
}

public class ExecutionResult
{
    public bool Success { get; set; }
    public string Output { get; set; } = "";
    public string Error { get; set; } = "";
    public int ExitCode { get; set; }
}

public enum LogLevel
{
    Info,
    Warning,
    Error,
    Success
}
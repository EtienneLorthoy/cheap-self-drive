using Newtonsoft.Json;

namespace CheapSelfDriveUI.Models;

public class Config
{
    public string MountName { get; set; } = "MyNAS";
    public string DriveLetter { get; set; } = "X:";
    public string NASAddress { get; set; } = "192.168.1.100";
    public string NASUsername { get; set; } = "";
    public int NASPort { get; set; } = 22;
    public string NASAbsolutePath { get; set; } = "/";
    public string ShellType { get; set; } = "unix";
    public string VFSCacheDir { get; set; } = "C:\\VFS\\{MountName}";
    public string RcloneLogs { get; set; } = "C:\\VFS\\{MountName}.log";
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

public class BannerConfig
{
    public string Title { get; set; } = "CheapSelfDrive Manager";
    public string BackgroundColor { get; set; } = "#f0f0f0";
    public string TitleColor { get; set; } = "#2c3e50";
    public string TitleFont { get; set; } = "Segoe UI";
    public int TitleSize { get; set; } = 18;
    public string LogoPath { get; set; } = "";
    public int LogoWidth { get; set; } = 64;
    public int LogoHeight { get; set; } = 64;
    public string Alignment { get; set; } = "center";
    public int Padding { get; set; } = 10;
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
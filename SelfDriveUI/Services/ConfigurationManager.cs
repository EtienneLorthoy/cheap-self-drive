using SelfDriveInstaller.Models;
using Newtonsoft.Json;

namespace SelfDriveInstaller.Services;

internal class ConfigurationManager
{
    internal Config LoadConfiguration(string path)
    {
        try
        {
            if (!File.Exists(path)) new Config();

            var json = File.ReadAllText(path);
            var config = JsonConvert.DeserializeObject<Config>(json);
            return config ?? new Config();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to load configuration from {path}: {ex.Message}", ex);
        }
    }

    internal void SaveConfiguration(Config config, string path)
    {
        try
        {
            ValidateConfiguration(config);
            
            var json = JsonConvert.SerializeObject(config, Formatting.Indented);
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            File.WriteAllText(path, json);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to save configuration to {path}: {ex.Message}", ex);
        }
    }

    internal void ValidateConfiguration(Config config)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(config.MountName))
            errors.Add("Mount Name is required");

        if (string.IsNullOrWhiteSpace(config.DriveLetter) || !IsValidDriveLetter(config.DriveLetter))
            errors.Add("Valid Drive Letter is required (e.g., X:)");

        if (string.IsNullOrWhiteSpace(config.NASAddress))
            errors.Add("NAS Address is required");

        if (string.IsNullOrWhiteSpace(config.NASUsername))
            errors.Add("NAS Username is required");

        if (config.NASPort <= 0 || config.NASPort > 65535)
            errors.Add("NAS Port must be between 1 and 65535");

        if (string.IsNullOrWhiteSpace(config.NASAbsolutePath))
            errors.Add("Remote Path is required");

        if (string.IsNullOrWhiteSpace(config.VFSCacheDir))
            errors.Add("VFS Cache Directory is required");

        if (string.IsNullOrWhiteSpace(config.RcloneLogs))
            errors.Add("Log File path is required");

        if (string.IsNullOrWhiteSpace(config.RcloneConfigDir))
            errors.Add("Rclone Config Directory is required");

        // Validate advanced settings
        if (config.AdvancedSettings != null)
        {
            var validCacheModes = new[] { "off", "minimal", "writes", "full" };
            if (!validCacheModes.Contains(config.AdvancedSettings.CacheMode))
                errors.Add($"Cache Mode must be one of: {string.Join(", ", validCacheModes)}");

            if (!IsValidSize(config.AdvancedSettings.CacheMaxSize))
                errors.Add("Cache Max Size must be valid (e.g., 20G, 1024M)");

            if (!IsValidDuration(config.AdvancedSettings.CacheMaxAge))
                errors.Add("Cache Max Age must be valid (e.g., 168h, 30m)");
        }

        if (errors.Any())
        {
            throw new ArgumentException($"Configuration validation failed:\n{string.Join("\n", errors)}");
        }
    }

    private static bool IsValidDriveLetter(string driveLetter)
    {
        if (string.IsNullOrWhiteSpace(driveLetter) || driveLetter.Length != 2)
            return false;

        return char.IsLetter(driveLetter[0]) && driveLetter[1] == ':';
    }

    private static bool IsValidSize(string size)
    {
        if (string.IsNullOrWhiteSpace(size))
            return false;

        var units = new[] { "B", "K", "M", "G", "T" };
        var lastChar = size[^1];
        
        if (char.IsDigit(lastChar))
            return long.TryParse(size, out _);

        if (units.Contains(lastChar.ToString().ToUpper()))
        {
            var numberPart = size[..^1];
            return long.TryParse(numberPart, out _);
        }

        return false;
    }

    private static bool IsValidDuration(string duration)
    {
        if (string.IsNullOrWhiteSpace(duration))
            return false;

        var units = new[] { "s", "m", "h", "d" };
        var lastChar = duration[^1];

        if (units.Contains(lastChar.ToString().ToLower()))
        {
            var numberPart = duration[..^1];
            return int.TryParse(numberPart, out _);
        }

        return false;
    }
}
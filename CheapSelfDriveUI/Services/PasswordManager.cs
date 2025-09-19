using System.Security.Cryptography;
using System.Text;

namespace CheapSelfDriveUI.Services;

public class PasswordManager
{
    private const string KeyPrefix = "CheapSelfDrive_";

    public void StorePassword(string identifier, string password)
    {
        try
        {
            var keyName = KeyPrefix + identifier;
            var passwordBytes = Encoding.UTF8.GetBytes(password);
            var encryptedBytes = ProtectedData.Protect(passwordBytes, null, DataProtectionScope.CurrentUser);
            var encryptedPassword = Convert.ToBase64String(encryptedBytes);
            
            // Store in Windows Credential Manager would be ideal, but for simplicity using registry
            Microsoft.Win32.Registry.SetValue(
                $@"HKEY_CURRENT_USER\Software\CheapSelfDrive\Passwords",
                keyName,
                encryptedPassword);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to store password for {identifier}: {ex.Message}", ex);
        }
    }

    public string RetrievePassword(string identifier)
    {
        try
        {
            var keyName = KeyPrefix + identifier;
            var encryptedPassword = Microsoft.Win32.Registry.GetValue(
                $@"HKEY_CURRENT_USER\Software\CheapSelfDrive\Passwords",
                keyName,
                null) as string;

            if (string.IsNullOrEmpty(encryptedPassword))
                return string.Empty;

            var encryptedBytes = Convert.FromBase64String(encryptedPassword);
            var passwordBytes = ProtectedData.Unprotect(encryptedBytes, null, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(passwordBytes);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to retrieve password for {identifier}: {ex.Message}", ex);
        }
    }

    public bool HasStoredPassword(string identifier)
    {
        try
        {
            var keyName = KeyPrefix + identifier;
            var value = Microsoft.Win32.Registry.GetValue(
                $@"HKEY_CURRENT_USER\Software\CheapSelfDrive\Passwords",
                keyName,
                null);
            return value != null;
        }
        catch
        {
            return false;
        }
    }

    public void DeletePassword(string identifier)
    {
        try
        {
            var keyName = KeyPrefix + identifier;
            using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                @"Software\CheapSelfDrive\Passwords", true);
            key?.DeleteValue(keyName, false);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to delete password for {identifier}: {ex.Message}", ex);
        }
    }

    public string GetPasswordIdentifier(string mountName, string username, string nasAddress)
    {
        return $"{mountName}_{username}_{nasAddress}".Replace(" ", "_");
    }
}
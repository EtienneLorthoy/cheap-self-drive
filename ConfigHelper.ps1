<#
.SYNOPSIS
    Shared configuration and password management functions for CheapSelfDrive SFTP mount scripts.

.DESCRIPTION
    This module provides common functionality used across all CheapSelfDrive scripts:
    - Secure password storage and retrieval using Windows Credential Manager
    - Configuration file loading and validation with template variable expansion
    - Windows Credential Manager API integration for secure password handling
    - Configuration summary display utilities
    
    All passwords are stored securely using Windows DPAPI and never appear in plain text
    in configuration files. The module handles automatic template variable expansion
    for paths containing placeholders like {MountName} and {APPDATA}.

.NOTES
    - Passwords are encrypted per-user using Windows DPAPI in Credential Manager
    - Configuration files support template variables for dynamic path generation
    - All functions include proper error handling and validation
    - This module is dot-sourced by other scripts in the CheapSelfDrive system
    - Supports Windows Credential Manager API for secure password operations

.FUNCTIONS
    Set-SecurePassword - Stores a password securely in Windows Credential Manager
    Get-SecurePassword - Retrieves a stored password as a SecureString
    Remove-SecurePassword - Removes a stored password from Credential Manager
    Test-SecurePasswordExists - Checks if a password is stored for a mount
    Get-SFTPDiskConfig - Loads and validates configuration from JSON file
    Show-ConfigSummary - Displays a formatted summary of configuration settings
#>

# ConfigHelper.ps1 - Common configuration loading for SFTP disk scripts

# Add Windows Credential Manager API types
Add-Type -TypeDefinition @"
using System;
using System.Runtime.InteropServices;
using System.Text;

public class CredentialManager
{
    [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern bool CredWrite(ref CREDENTIAL userCredential, uint flags);

    [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern bool CredRead(string target, uint type, uint reservedFlag, out IntPtr credentialPtr);

    [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern bool CredDelete(string target, uint type, uint reservedFlag);

    [DllImport("advapi32.dll")]
    public static extern void CredFree(IntPtr cred);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct CREDENTIAL
    {
        public uint Flags;
        public uint Type;
        public string TargetName;
        public string Comment;
        public System.Runtime.InteropServices.ComTypes.FILETIME LastWritten;
        public uint CredentialBlobSize;
        public IntPtr CredentialBlob;
        public uint Persist;
        public uint AttributeCount;
        public IntPtr Attributes;
        public string TargetAlias;
        public string UserName;
    }

    public const uint CRED_TYPE_GENERIC = 1;
    public const uint CRED_PERSIST_LOCAL_MACHINE = 2;
}
"@

function Get-CredentialTarget {
    param([string]$MountName)
    return "CheapSelfDrive:$MountName"
}

function Set-SecurePassword {
    param(
        [string]$MountName,
        [SecureString]$Password
    )
    
    if (-not $Password -or $Password.Length -eq 0) {
        Write-Warning "Empty password provided for mount '$MountName'"
        return
    }
    
    try {
        $target = Get-CredentialTarget -MountName $MountName
        # Convert SecureString to plain text for Windows Credential Manager
        $plainPassword = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto([System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($Password))
        
        # Create credential structure
        $credential = New-Object CredentialManager+CREDENTIAL
        $credential.Type = [CredentialManager]::CRED_TYPE_GENERIC
        $credential.TargetName = $target
        $credential.UserName = $MountName
        $credential.Persist = [CredentialManager]::CRED_PERSIST_LOCAL_MACHINE
        
        # Convert password to byte array
        $passwordBytes = [System.Text.Encoding]::Unicode.GetBytes($plainPassword)
        $credential.CredentialBlobSize = $passwordBytes.Length
        $credential.CredentialBlob = [System.Runtime.InteropServices.Marshal]::AllocHGlobal($passwordBytes.Length)
        [System.Runtime.InteropServices.Marshal]::Copy($passwordBytes, 0, $credential.CredentialBlob, $passwordBytes.Length)
        
        # Write credential
        $result = [CredentialManager]::CredWrite([ref]$credential, 0)
        
        # Free allocated memory
        [System.Runtime.InteropServices.Marshal]::FreeHGlobal($credential.CredentialBlob)
        
        if (-not $result) {
            $lastError = [System.Runtime.InteropServices.Marshal]::GetLastWin32Error()
            throw "Failed to write credential. Win32 Error: $lastError"
        }
        
        Write-Verbose "Password securely stored in Windows Credential Manager for mount '$MountName'"
    }
    catch {
        Write-Error "Failed to store password for mount '$MountName': $($_.Exception.Message)"
        throw
    }
}

function Get-SecurePassword {
    param([string]$MountName)
    
    try {
        $target = Get-CredentialTarget -MountName $MountName
        $credentialPtr = [IntPtr]::Zero
        
        $result = [CredentialManager]::CredRead($target, [CredentialManager]::CRED_TYPE_GENERIC, 0, [ref]$credentialPtr)
        
        if (-not $result) {
            Write-Verbose "No stored password found for mount '$MountName'"
            return $null
        }
        
        try {
            # Marshal the credential structure
            $credential = [System.Runtime.InteropServices.Marshal]::PtrToStructure($credentialPtr, [Type][CredentialManager+CREDENTIAL])
            
            # Extract password from credential blob
            $passwordBytes = New-Object byte[] $credential.CredentialBlobSize
            [System.Runtime.InteropServices.Marshal]::Copy($credential.CredentialBlob, $passwordBytes, 0, $credential.CredentialBlobSize)
            $plainPassword = [System.Text.Encoding]::Unicode.GetString($passwordBytes)
            
            # Convert plain text password to SecureString
            $securePassword = ConvertTo-SecureString $plainPassword -AsPlainText -Force
            
            # Clear the plain text password from memory
            $plainPassword = $null
            
            return $securePassword
        }
        finally {
            # Free the credential memory
            [CredentialManager]::CredFree($credentialPtr)
        }
    }
    catch {
        Write-Error "Failed to retrieve password for mount '$MountName': $($_.Exception.Message)"
        throw
    }
}

function Remove-SecurePassword {
    param([string]$MountName)
    
    try {
        $target = Get-CredentialTarget -MountName $MountName
        $result = [CredentialManager]::CredDelete($target, [CredentialManager]::CRED_TYPE_GENERIC, 0)
        
        if ($result) {
            Write-Verbose "Password removed from Windows Credential Manager for mount '$MountName'"
        } else {
            $lastError = [System.Runtime.InteropServices.Marshal]::GetLastWin32Error()
            # Error 1168 = Element not found, which is OK if credential doesn't exist
            if ($lastError -ne 1168) {
                Write-Warning "Failed to remove password for mount '$MountName'. Win32 Error: $lastError"
            }
        }
    }
    catch {
        Write-Warning "Failed to remove password for mount '$MountName': $($_.Exception.Message)"
    }
}

function Test-SecurePasswordExists {
    param([string]$MountName)
    
    try {
        $target = Get-CredentialTarget -MountName $MountName
        $credentialPtr = [IntPtr]::Zero
        
        $result = [CredentialManager]::CredRead($target, [CredentialManager]::CRED_TYPE_GENERIC, 0, [ref]$credentialPtr)
        
        if ($result) {
            [CredentialManager]::CredFree($credentialPtr)
            return $true
        } else {
            return $false
        }
    }
    catch {
        return $false
    }
}

function Get-SFTPDiskConfig {
    param(
        [string]$ConfigPath = (Join-Path $PSScriptRoot 'config.json')
    )
    
    if (-not (Test-Path $ConfigPath)) {
        throw "Configuration file not found at: $ConfigPath"
    }
    
    try {
        $config = Get-Content -Path $ConfigPath -Raw | ConvertFrom-Json
        
        # Expand template variables
        $mountName = $config.MountName
        $config.VFSCacheDir = $config.VFSCacheDir -replace '\{MountName\}', $mountName
        $config.RcloneLogs = $config.RcloneLogs -replace '\{MountName\}', $mountName
        $config.RcloneConfigDir = $config.RcloneConfigDir -replace '\{APPDATA\}', $env:APPDATA
        
        # Add computed properties
        $config | Add-Member -NotePropertyName 'RcloneConfig' -NotePropertyValue (Join-Path $config.RcloneConfigDir 'rclone.conf')
        $config | Add-Member -NotePropertyName 'MountScriptPath' -NotePropertyValue "C:\VFS\Mount-$($config.MountName)-SFTPDisk.ps1"
        $config | Add-Member -NotePropertyName 'TaskName' -NotePropertyValue "Rclone Mount $($config.MountName)"
        
        # Remove any password field from config if it exists (ignore it completely)
        if ($config.PSObject.Properties.Name -contains 'NASPassword') {
            $config.PSObject.Properties.Remove('NASPassword')
        }
        
        return $config
    }
    catch {
        throw "Failed to parse configuration file: $($_.Exception.Message)"
    }
}

function Show-ConfigSummary {
    param($Config)

    [pscustomobject]@{
        MountName            = $Config.MountName
        DriveLetter          = $Config.DriveLetter
        NASAddress           = $Config.NASAddress
        NASUsername          = $Config.NASUsername
        NASPort              = $Config.NASPort
        NASAbsolutePath      = $Config.NASAbsolutePath
        RcloneLogs           = $Config.RcloneLogs
        RcloneConfig         = $Config.RcloneConfig
    } | Format-List | Out-String | Write-Host
}

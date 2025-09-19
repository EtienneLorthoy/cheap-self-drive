<#
.SYNOPSIS
    Sets a secure password for a CheapSelfDrive mount configuration.

.DESCRIPTION
    This script allows you to securely store a password for an SFTP mount using Windows Credential Manager.
    The password is stored securely and will be automatically retrieved when mounting the SFTP drive.

.PARAMETER MountName
    The name of the mount configuration to set the password for. This should match the MountName
    in your config.json file.

.PARAMETER Password
    Optional SecureString containing the password. If not provided, you will be prompted to enter it securely.

.EXAMPLE
    .\Set-Password.ps1 -MountName "MyNAS"
    Prompts for and securely stores a password for the MyNAS mount configuration.

.NOTES
    - The password is stored in Windows Credential Manager under the target "CheapSelfDrive:MountName"
    - The stored password will be automatically retrieved by other CheapSelfDrive scripts
#>

param(
    [Parameter(Mandatory)]
    [string]$MountName,
    [SecureString]$Password
)

# Load configuration helper
. (Join-Path $PSScriptRoot 'ConfigHelper.ps1')

function Write-Info { param([string]$m) Write-Host $m -ForegroundColor Cyan }
function Write-Warn { param([string]$m) Write-Host $m -ForegroundColor Yellow }
function Write-Err { param([string]$m) Write-Host $m -ForegroundColor Red }
function Write-Ok { param([string]$m) Write-Host $m -ForegroundColor Green }

try {
    # If no password provided, prompt for it
    if (-not $Password) {
        Write-Info "Enter password for mount '$MountName'"
        $Password = Read-Host -Prompt "Password" -AsSecureString
    }
    
    # Validate that a password was provided
    if (-not $Password -or $Password.Length -eq 0) {
        Write-Err "No password provided. Exiting."
        exit 1
    }
    
    # Set the password using ConfigHelper function
    Set-SecurePassword -MountName $MountName -Password $Password
    
    Write-Ok "Password successfully set for mount '$MountName'"
    Write-Info "The password is now stored securely in Windows Credential Manager."
    Write-Info "It will be automatically retrieved when mounting the SFTP drive."
    
    # Test that the password was stored correctly
    if (Test-SecurePasswordExists -MountName $MountName) {
        Write-Ok "Verification: Password storage confirmed."
    } else {
        Write-Warn "Warning: Could not verify password storage."
    }
}
catch {
    Write-Err "Failed to set password for mount '$MountName': $($_.Exception.Message)"
    exit 1
}
finally {
    # Clear the password from memory if we created it
    if ($Password) {
        $Password.Dispose()
    }
}
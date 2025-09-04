# ConfigHelper.ps1 - Common configuration loading for SFTP disk scripts

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
        
        return $config
    }
    catch {
        throw "Failed to parse configuration file: $($_.Exception.Message)"
    }
}

function Show-ConfigSummary {
    param($Config)
    
    $maskedPassword = if ($Config.NASPassword) {
        if ($Config.NASPassword.Length -gt 2) { 
            $Config.NASPassword.Substring(0,1) + ('*' * ($Config.NASPassword.Length -2)) + $Config.NASPassword[-1] 
        } else { 
            '*' * $Config.NASPassword.Length 
        }
    } else { 
        '<empty>' 
    }

    [pscustomobject]@{
        MountName            = $Config.MountName
        DriveLetter          = $Config.DriveLetter
        NASAddress           = $Config.NASAddress
        NASUsername          = $Config.NASUsername
        NASPassword          = $maskedPassword
        NASPort              = $Config.NASPort
        NASAbsolutePath      = $Config.NASAbsolutePath
        RcloneLogs           = $Config.RcloneLogs
        RcloneConfig         = $Config.RcloneConfig
    } | Format-List | Out-String | Write-Host
}

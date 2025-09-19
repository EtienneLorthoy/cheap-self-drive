<#
.SYNOPSIS
    Template script for mounting SFTP drives using rclone with VFS caching.

.DESCRIPTION
    This is a template script that gets processed by the installer to create the actual
    mount script with concrete configuration values. The installer replaces placeholder
    variables with real values from the configuration file.
    
    The generated script:
    - Mounts an SFTP remote as a Windows drive letter using rclone
    - Uses VFS caching for improved performance and offline access
    - Includes comprehensive error handling and logging
    - Checks for existing mounts to avoid conflicts
    - Runs rclone in network mode with optimized cache settings
    
    This template can also be used standalone by manually replacing the placeholder
    values, but it's typically processed automatically during installation.

.NOTES
    - Placeholder values are replaced by the installer with actual configuration
    - Uses rclone VFS cache mode 'full' for best performance
    - Cache settings are optimized for typical NAS usage patterns
    - Runs rclone in hidden mode to avoid console windows
    - Includes file permissions and link handling for Windows compatibility
    - Generated scripts are placed in C:\VFS\ directory
    
.PLACEHOLDERS
    REPLACED_BY_INSTALLER - All configuration values are replaced during installation
    
.GENERATED_LOCATION
    C:\VFS\Mount-{MountName}-SFTPDisk.ps1
#>

# NOTE: This file can be used as a standalone template, but the installer script
# will also generate a final version in C:\VFS with concrete values.

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# These placeholder values will be replaced by the installer-generated copy
$MountName    = 'REPLACED_BY_INSTALLER'
$DriveLetter  = 'REPLACED_BY_INSTALLER'  # e.g. 'v:'
$VFSCacheDir  = 'REPLACED_BY_INSTALLER'
$RcloneLogs   = 'REPLACED_BY_INSTALLER'
$RcloneConfig = 'REPLACED_BY_INSTALLER'

try {
    Write-Host "Mount script starting for $MountName -> $DriveLetter"
    if (-not (Test-Path $RcloneConfig)) { throw "rclone config not found at $RcloneConfig" }
    New-Item -ItemType Directory -Path $VFSCacheDir -Force | Out-Null

    # If drive already present, exit early (assumes already mounted)
    $driveLetterOnly = $DriveLetter.TrimEnd(':')
    if (Get-PSDrive -Name $driveLetterOnly -ErrorAction SilentlyContinue) {
        Write-Host "Drive $DriveLetter already exists; skipping new mount." -ForegroundColor Yellow
        exit 0
    }

    $arguments = @(
        'mount'
        ("${MountName}:")
        $DriveLetter
        '--no-console'
        '--log-file'; $RcloneLogs
        '--config'; $RcloneConfig
        '--vfs-cache-mode'; 'full'
        '--vfs-cache-max-size'; '20G'
        '--vfs-cache-max-age'; '168h'
        '--dir-cache-time'; '30s'
        '--poll-interval'; '15s'
        '--buffer-size'; '16M'
        '--vfs-cache-min-free-space'; '20G'
        '--cache-dir'; $VFSCacheDir
        '--network-mode'
        '--file-perms=0777'
        '--links'
    )

    Write-Host "Executing rclone mount ..."
    Start-Process -FilePath "rclone" -ArgumentList $arguments  -WindowStyle Hidden
    $exit = $LASTEXITCODE
    $level = if ($exit -eq 0) { 'INFO' } else { 'ERROR' }
    $color = if ($exit -eq 0) { 'Green' } else { 'Red' }
    Write-Host "rclone exited with code $exit" -ForegroundColor $color
    exit $exit
}
catch {
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}
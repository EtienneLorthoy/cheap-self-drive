# Cheap Self-Drive - SFTP Auto Mount System

This project provides a complete solution for mounting SFTP shares as Windows drives using rclone and VFS caching. It operates similarly to Google Drive or OneDrive desktop apps - you can browse your remote folders as a Windows drive without downloading anything. Files are downloaded on-demand when opened and cached for faster access.

## Features

- **Automated Installation**: PowerShell scripts handle complete setup including dependencies
- **Secure Password Management**: Credentials stored in Windows Credential Manager using DPAPI encryption
- **GUI Installer**: Windows Forms-based installer with visual status feedback
- **System Tray Monitor**: Real-time drive status monitoring with quick reconnection options
- **Windows Integration**: System Tray Monitor runs as at system startup 
- **VFS Caching**: Caching for optimal performance and disk usage (all rclone!)
- **Multiple Mount Support**: Configure and manage multiple SFTP mounts simultaneously

## Components

### PowerShell Scripts
- **Install-CheapSelfDrive.ps1** - Main installation script with automatic dependency management
- **Uninstall-CheapSelfDrive.ps1** - Complete cleanup including tasks, cache, and credentials
- **Set-Password.ps1** - Secure password storage utility
- **ConfigHelper.ps1** - Shared configuration and credential management functions
- **Mount-TEMPLATE-SFTPDisk.ps1** - Template for generating mount scripts

### GUI Applications
- **SelfDrive.UI** - Windows Forms installer with visual configuration and testing
- **SelfDrive.Systray** - System tray application for monitoring drive status and reconnection

## Prerequisites

The install script handles all dependencies automatically. You only need:
- Windows 10 or Windows 11
- PowerShell 5.1 or later
- Internet connection (for downloading dependencies)

## Quick Start

1. Build and run the SelfDrive applications
2. Edit `config.json` with your SFTP server details (see Configuration section below for details)
3. Test connection and install with one click
4. Monitor status in real-time through the system tray app

### System Tray Monitor (Optional)

The system tray application provides:
- Real-time drive status monitoring
- Visual indicators (green = OK, red = disconnected)
- Quick reconnection via right-click menu
- Hover for detailed status information

Configure drive mappings in `drive-mappings.txt` next to the executable.

## Configuration

All scripts use a centralized JSON configuration file (default: `config.json`).

### Configuration File Structure

```json
{
  "MountName": "MyNAS",
  "DriveLetter": "X:",
  "NASAddress": "your_server_ip_or_hostname",
  "NASUsername": "default_username_to_show_in_the installer",
  "NASPort": 22,
  "NASAbsolutePath": "/path/to/remote/folder",
  "ShellType": "unix",
  "InstallDirectory": "C:\\SelfDrive",
  "VFSCacheDir": "C:\\SelfDrive\\VFS\\{MountName}",
  "RcloneLogs": "C:\\SelfDrive\\{MountName}.log",
  "RcloneConfigDir": "{APPDATA}\\rclone\\rclone.conf",
  "AdvancedSettings": {
    "CacheMode": "full",
    "CacheMaxSize": "20G",
    "CacheMaxAge": "168h",
    "DirCacheTime": "30s",
    "PollInterval": "15s",
    "BufferSize": "16M",
    "CacheMinFreeSpace": "20G"
  }
}
```

### Template Variables

Configuration paths support the following variables:
- `{MountName}` - Replaced with the value of MountName
- `{APPDATA}` - Replaced with user's AppData\Roaming directory

### Multiple Mounts

To configure multiple SFTP mounts:
1. Create separate configuration files (e.g., `config.fastnas.json`, `config.slownas.json`)
2. Load them in the Installer

## Advanced Configuration

### SSH Key Authentication

While the scripts use password authentication by default, you can configure SSH key authentication:

1. Run the installer to create the base configuration
2. Edit `%AppData%\rclone\rclone.conf`
3. Remove the `pass` line from your mount configuration
4. Add SSH key configuration:
   ```
   key_pem = -----BEGIN OPENSSH PRIVATE KEY-----your_private_key-----END OPENSSH PRIVATE KEY-----
   key_file_pass = your_key_pass
   ```

Alternatively, use rclone's command-line tools or a GUI to edit the configuration.

### Custom Rclone Options

The scripts create rclone remotes with sensible defaults. To customize:
- Modify the `rclone config create` commands in `Install-CheapSelfDrive.ps1`
- Edit `%AppData%\rclone\rclone.conf` directly after installation
- Use advanced settings in the configuration JSON file

## Automated Dependencies

The installation script automatically installs via winget:
- **rclone** - SFTP mounting and VFS caching
- **WinFsp** - Windows File System Proxy (required for drive mounting)

## Security

- **No passwords in configuration files**: Credentials are stored securely in Windows Credential Manager
- **DPAPI encryption**: Passwords are encrypted per-user using Windows Data Protection API  
- **Secure password entry**: Use `Set-Password.ps1` to avoid exposing credentials (optional if you want to bypass installer)

## Troubleshooting

Your own you own, but contact me. Depending on circumstances, I might help you, maybe!

## Uninstallation

To completely remove a mount configuration:

```powershell
.\Uninstall-CheapSelfDrive.ps1
```

Or specify a custom config:
```powershell
.\Uninstall-CheapSelfDrive.ps1 -ConfigPath "config.custom.json"
```

This will:
- Terminate rclone processes for that mount
- Remove cached files
- Delete stored credentials
- Clean up rclone remote configuration
- You might want to delete the entire folder after the cleaning

## Development

### Building the GUI Applications

Prerequisites:
- .NET 9.0 SDK
- Visual Studio 2022 or VS Code with C# extension

Build commands:
```powershell
# Build the entire solution
dotnet build SelfDrive.sln

# Build specific projects
dotnet build SelfDriveUI/SelfDrive.Installer.csproj
dotnet build SelfDrive.Systray/SelfDrive.Systray.csproj

# Release builds
dotnet build -c Release
```

### Contributing

This project is designed to be user-friendly for the specific scenario of Windows clients connecting to SFTP shares on Linux servers. While rclone supports many more features, the scripts focus on simplicity and reliability for this common use case.

Feel free to extend the scripts for additional protocols or configurations by modifying the `rclone config create` commands in the installation scripts.

## License

MIT License

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

---
> [etiennelorthoy.com](https://etiennelorthoy.com) &nbsp;&middot;&nbsp;
> LinkedIn [@etiennelorthoy](https://www.linkedin.com/in/etienne-lorthoy/) &nbsp;&middot;&nbsp;
> GitHub [@etiennelorthoy](https://github.com/EtienneLorthoy) &nbsp;&middot;&nbsp;
> Twitter [@ELorthoy](https://twitter.com/ELorthoy)
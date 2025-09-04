# SFTP Auto Mount Disk - Configuration Guide

This project provides PowerShell scripts to mount SFTP drives using rclone and Windows Scheduled Tasks. This is meant to use rclone with VFS caching which operates in a similar way GoogleDrive or OneDrive desktop apps do. You can list the entirety of your online folder as a Windows Drive without downloading anything, and when you open a file it downloads that files live and keep in a cache for a while for faster access. 

    1. Loads configuration from a JSON file (config.json by default)
    2. Installs required dependencies (rclone and WinFsp via winget)
    3. Creates necessary directories for VFS caching and logs
    4. Configures rclone remotes for SFTP connection
    5. Generates a mount script from template with concrete configuration values
    6. Creates a Windows Scheduled Task that runs at user logon
    7. Sets up retry policies and execution limits for reliability

## Prerequisites

The install script should take care of every dependencies. You just need Windows and Powershell.

## Usage

1. Edit your config.json with your values.
2. Just right click and 'Run with Powershell'
 - `Install-SFTPDiskScheduledTask.ps1` for installing and prepare the mount.
 - `Start-SFTPDiskScheduledTask.ps1` to start it manually (otherwise the mount will be started at each user logon).
3. In a manner of seconds the new drive should be mounted.

### Limitation

This scripts is made to be user friendly first with this specific scenario only: Windows Client connecting to a SFTP share on a Linux server using user/password creds.

Though rclone is entirely capable of much more, you can change the remote creation lines in `Install-SFTPDiskScheduledTask.ps1` (rclone config create ...).

Or for example if you use SSH keys instead of password, run the install and then edit `%AppData%\rclone\rclone.conf`:
remove the pass = xxxxxxx lines and add instead:

```
key_pem = -----BEGIN OPENSSH PRIVATE KEY-----your_private_key-----END OPENSSH PRIVATE KEY-----
key_file_pass = your_key_pass
```

Alternatively and because those script use default rclone.conf, you can use the rclone command line to edit remote directly or any software that offers a GUI to edit rclone's config.
 
## Configuration

All scripts use a centralized JSON configuration file (`config.json`).

### Configuration File Structure

```json
{
  "MountName": "MyNAS",
  "DriveLetter": "X:",
  "NASAddress": "your_linux_server_ip_address",
  "NASUsername": "your_username", 
  "NASPassword": "your_password",
  "NASPort": 22,
  "NASAbsolutePath": "/home/etienne/lmnt",
  "ShellType": "unix",
  "VFSCacheDir": "C:\\VFS\\{MountName}",
  "RcloneLogs": "C:\\VFS\\{MountName}.log",
  "RcloneConfigDir": "{APPDATA}\\rclone"
}
```

### Configuration Overrides

All scripts support specifying a custom configuration file, if not provided scripts will use the default config.json.

- `-ConfigPath`: Use a different configuration file


## Files
- `Install-SFTPDiskScheduledTask.ps1` - Installation script
- `Start-SFTPDiskScheduledTask.ps1` - Start scheduled task
- `Uninstall-SFTPDiskScheduledTask.ps1` - Cleanup everything script (mount, task and cache)
- `config.json` - Configuration file
- `ConfigHelper.ps1` - Shared configuration loading module
- `Mount-TEMPLATE-SFTPDisk.ps1` - Template for generated mount script

## Security Note

Remember to secure your `config.json` file as it contains credentials.

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
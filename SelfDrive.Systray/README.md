# Drive Tray Monitor

A Windows system tray application that monitors network drive availability and provides quick reconnection functionality.

## Features

- 🟢 **Real-time Drive Monitoring**: Continuously checks if F:/ and S:/ drives are accessible
- 🔴 **Visual Status Indicator**: Green icon when all drives are OK, red icon when any drive is missing
- 🔄 **Quick Reconnection**: Right-click menu to execute PowerShell reconnection scripts
- 💬 **Status Tooltips**: Hover over the icon to see detailed drive status
- ⚡ **Lightweight**: Minimal resource usage, runs silently in the background

## Requirements

- Windows 10 or Windows 11
- .NET 9.0 Windows Desktop Runtime
- PowerShell 5.1 or later
- PowerShell reconnection scripts:
  - `C:\VFS\Mount-FastNas-SFTPDisk.ps1` (for F:/ drive)
  - `C:\VFS\Mount-SlowNas-SFTPDisk.ps1` (for S:/ drive)

## Installation

1. **Install .NET 9.0 Runtime** (if not already installed):
   - Download from: https://dotnet.microsoft.com/download/dotnet/9.0

2. **Build the application**:
   ```bash
   dotnet build -c Release
   ```

3. **Run the application**:
   ```bash
   dotnet run
   ```
   Or navigate to `bin\Release\net9.0-windows\` and run `DriveTrayMonitor.exe`

## Usage

### Starting the Application
- Run `DriveTrayMonitor.exe`
- The application will appear as an icon in the system tray
- No main window will be displayed

### Icon Colors
- **🟢 Green**: Both F:/ and S:/ drives are accessible
- **🔴 Red**: One or both drives are not accessible

### Reconnecting Drives
1. Right-click the system tray icon
2. Select "Reconnect F:/" or "Reconnect S:/" from the menu
3. The corresponding PowerShell script will execute
4. A balloon notification will appear
5. Drive status will be re-checked after 2 seconds

### Viewing Status
- Hover over the system tray icon to see detailed status:
  ```
  Drive Monitor
  F:\ - OK
  S:\ - Missing
  ```

### Exiting the Application
- Right-click the system tray icon
- Select "Exit"

## Auto-Start on Windows Login

To run the application automatically when you log in:

1. Press `Win + R` and type `shell:startup`, press Enter
2. Create a shortcut to `DriveTrayMonitor.exe` in the Startup folder
3. The application will now start automatically on login

## Configuration

The application loads drive/script mappings from a simple text file at startup.

### drive-mappings.txt

- File name: `drive-mappings.txt`
- Location: next to the app executable (copied automatically on build/publish)
- Format: one mapping per line
  - `<DriveRoot>|<PowerShellScriptPath>`
  - Example:
    - `F:\|C:\VFS\Mount-FastNas-SFTPDisk.ps1`

Lines starting with `#` and blank lines are ignored.

### Defaults

If `drive-mappings.txt` is missing, the application falls back to these defaults:

| Setting | Value |
|---------|-------|
| F:/ Drive Path | `F:\` |
| S:/ Drive Path | `S:\` |
| F:/ Reconnect Script | `C:\VFS\Mount-FastNas-SFTPDisk.ps1` |
| S:/ Reconnect Script | `C:\VFS\Mount-SlowNas-SFTPDisk.ps1` |
| Check Interval | 5 seconds |

To modify these settings, edit `drive-mappings.txt`.

## Building from Source

### Build for Development
```bash
dotnet build
```

### Build for Release
```bash
dotnet build -c Release
```

### Publish Self-Contained Executable
```bash
dotnet publish -c Release -r win-x64 --self-contained true
```

This creates a standalone executable that doesn't require .NET runtime to be installed separately.

## Project Structure

```
systray-test/
├── DriveTrayMonitor.csproj    # Project file
├── Program.cs                  # Application entry point
├── TrayApplicationContext.cs   # Main application logic
├── Resources/                  # Resource files directory
├── SPECIFICATION.md            # Technical specification
└── README.md                   # This file
```

## Troubleshooting

### Icon doesn't appear in system tray
- Ensure the application is running (check Task Manager)
- Check Windows system tray settings (Settings > Personalization > Taskbar)

### Scripts don't execute
- Verify script paths exist:
  - `C:\VFS\Mount-FastNas-SFTPDisk.ps1`
  - `C:\VFS\Mount-SlowNas-SFTPDisk.ps1`
- Ensure PowerShell execution policy allows scripts
- Check script permissions

### Drives always show as missing
- Verify drives are mapped correctly in Windows
- Check if drives F:/ and S:/ are accessible in File Explorer
- Review network connection

## Technical Details

- **Framework**: .NET 9.0
- **UI**: Windows Forms
- **Icon Generation**: Programmatic (GDI+)
- **Monitoring Interval**: 5 seconds
- **Script Execution**: PowerShell with `-ExecutionPolicy Bypass`

For detailed technical specifications, see [SPECIFICATION.md](SPECIFICATION.md).

## License

This project is provided as-is for internal use.

## Support

For issues or questions, please refer to the specification document or contact your system administrator.

using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinFormsTimer = System.Windows.Forms.Timer;

namespace DriveTrayMonitor
{
    public class TrayApplicationContext : ApplicationContext
    {
        private static readonly Dictionary<string, ProbeInfo> _activeProbeProcesses = new(StringComparer.OrdinalIgnoreCase);

        private static readonly TimeSpan ReconnectYellowDuration = TimeSpan.FromSeconds(20);

        private readonly NotifyIcon _trayIcon;
        private readonly WinFormsTimer _checkTimer;
        private readonly WinFormsTimer _autoReconnectTimer;
        private readonly ContextMenuStrip _contextMenu;

        private readonly ToolStripMenuItem _autoReconnectMenuItem;

        private WinFormsTimer? _forceYellowTimer;
        private DateTimeOffset? _forceYellowUntilUtc;
        private List<(string DriveRoot, bool Exists)> _lastStatuses = new();

        private readonly SynchronizationContext _uiContext;
        private volatile bool _updateInProgress;
        private readonly CancellationTokenSource _shutdownCts = new();

        private readonly Icon[] _icons = new Icon[3]; // 0=Red, 1=Yellow, 2=Green

        private readonly Dictionary<string, string> _driveToScript;

        private const string CONFIG_FILE_NAME = "drive-mappings.txt";
        private const int DRIVE_CHECK_TIMEOUT_MS = 1500;

        public TrayApplicationContext()
        {
            _uiContext = SynchronizationContext.Current ?? new WindowsFormsSynchronizationContext();
            _driveToScript = LoadDriveMappings();

            // Create context menu
            _contextMenu = new ContextMenuStrip();

            // Icons
            _icons[0] = CreateIcon(Color.Red);
            _icons[1] = CreateIcon(Color.Gold);
            _icons[2] = CreateIcon(Color.Green);

            _autoReconnectTimer = new WinFormsTimer()
            {
                Interval = (int)TimeSpan.FromMinutes(1).TotalMilliseconds
            };
            _autoReconnectTimer.Tick += (s, e) =>
            {
                if (_shutdownCts.IsCancellationRequested)
                {
                    return;
                }

                ReconnectAllFailedDrives(showBalloonTip: false);
            };

            _autoReconnectMenuItem = new ToolStripMenuItem("Auto Reconnect")
            {
                CheckOnClick = true,
                Checked = false
            };
            _autoReconnectMenuItem.CheckedChanged += (s, e) =>
            {
                if (_autoReconnectMenuItem.Checked)
                {
                    _autoReconnectTimer.Start();
                }
                else
                {
                    _autoReconnectTimer.Stop();
                }
            };

            _contextMenu.Items.Add(_autoReconnectMenuItem);

            _contextMenu.Items.Add($"Reconnect all failed now", null, (s, e) =>
            {
                ReconnectAllFailedDrives(showBalloonTip: true);
            });

            _contextMenu.Items.Add(new ToolStripSeparator());
            _contextMenu.Items.Add("Exit", null, OnExit);

            // Create tray icon
            _trayIcon = new NotifyIcon()
            {
                ContextMenuStrip = _contextMenu,
                Text = "Drive Monitor",
                Visible = true
            };

            // Startup state: show "checking" before the first validation completes
            _trayIcon.Icon = _icons[1];
            _trayIcon.Text = SafeNotifyText("Drive Monitor: Checking...");

            // Set up timer to check drives periodically
            _checkTimer = new WinFormsTimer()
            {
                Interval = 5000 // Check every 5 seconds
            };
            _checkTimer.Tick += (s, e) => _ = BeginUpdateDriveStatusAsync(_shutdownCts.Token);
            _checkTimer.Start();

            // Initial check
            _ = BeginUpdateDriveStatusAsync(_shutdownCts.Token);
        }

        private async Task BeginUpdateDriveStatusAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            if (_updateInProgress)
            {
                return;
            }

            _updateInProgress = true;
            try
            {
                var statuses = await ComputeStatusesAsync(cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                _uiContext.Post(_ =>
                {
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        ApplyStatuses(statuses);
                    }
                }, null);
            }
            finally
            {
                _updateInProgress = false;
            }
        }

        private async Task<List<(string DriveRoot, bool Exists)>> ComputeStatusesAsync(CancellationToken cancellationToken)
        {
            if (_driveToScript.Count == 0)
            {
                return new List<(string DriveRoot, bool Exists)>();
            }

            var statuses = new List<(string DriveRoot, bool Exists)>();
            foreach (string driveRoot in _driveToScript.Keys.OrderBy(k => k, StringComparer.OrdinalIgnoreCase))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                bool ok = await IsDriveReadableAsync(driveRoot, DRIVE_CHECK_TIMEOUT_MS, cancellationToken);
                statuses.Add((driveRoot, ok));
            }

            return statuses;
        }

        private void ApplyStatuses(List<(string DriveRoot, bool Exists)> statuses)
        {
            _lastStatuses = statuses;

            if (_driveToScript.Count == 0)
            {
                _trayIcon.Text = SafeNotifyText("Drive Monitor (no drives configured)");
                return;
            }

            bool allDrivesOk = statuses.Count > 0 && statuses.All(s => s.Exists);

            // Update icon
            if (!IsForceYellowActive() || allDrivesOk) _trayIcon.Icon = allDrivesOk ? _icons[2] : _icons[0];
            else _trayIcon.Icon = _icons[1];

            string statusText = string.Join("\n", statuses.Select(s => $"{ToDriveLabel(s.DriveRoot)} {(s.Exists ? "✅" : "🟥")}"));
            _trayIcon.Text = SafeNotifyText($"Drives Monitor\n{statusText}");
        }

        private bool IsForceYellowActive()
        {
            return _forceYellowUntilUtc is DateTimeOffset untilUtc && DateTimeOffset.UtcNow < untilUtc;
        }

        private void BeginForceYellowForReconnect()
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;
            DateTimeOffset newUntil = now.Add(ReconnectYellowDuration);

            if (_forceYellowUntilUtc is DateTimeOffset existingUntil && existingUntil > newUntil)
            {
                newUntil = existingUntil;
            }

            _forceYellowUntilUtc = newUntil;
            _trayIcon.Icon = _icons[1];
        }

        private static async Task<bool> IsDriveReadableAsync(string driveRoot, int timeoutMs, CancellationToken cancellationToken)
        {
            // Some network/VFS mounts (e.g., rclone) can hang indefinitely on IO.
            // Use a separate process for the probe so we can time out and kill it without
            // ever blocking this process.
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            linkedCts.CancelAfter(timeoutMs);

            ProbeInfo? probeInfo = null;

            // Check for existing probe process for this drive.
            if (_activeProbeProcesses.TryGetValue(driveRoot, out probeInfo))
            {                
                if (IsProcessAlive(probeInfo.Process, probeInfo.StartTime)) return false; // It is running
                else // It is not running
                {
                    // Clean up exited process.
                    _activeProbeProcesses.Remove(driveRoot);
                    probeInfo.Process.Dispose();
                    probeInfo = null;
                }
            }

            // At this point, no active probe exists for this drive.
            try
            {
                var startInfo = new ProcessStartInfo()
                {
                    FileName = "cmd.exe",
                    // "dir" forces the OS/VFS to actually touch the mount.
                    // Redirect to nul so we don't buffer output.
                    Arguments = $"/c dir /b \"{driveRoot}\" >nul 2>nul",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                };

                var process = Process.Start(startInfo);
                probeInfo = process is not null ? new ProbeInfo(process) : null;

                if (probeInfo is null) return false;

                _activeProbeProcesses[driveRoot] = probeInfo;

                await probeInfo.Process.WaitForExitAsync(linkedCts.Token);

                // Exit code 0 is success. Any error (missing/unreachable) usually returns non-zero.
                return probeInfo.Process.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }

        private void ReconnectAllFailedDrives(bool showBalloonTip)
        {
            var failed = _lastStatuses.Where(s => !s.Exists).Select(s => s.DriveRoot).ToList();
            if (failed.Count == 0)
            {
                return;
            }

            if (showBalloonTip)
            {
                _trayIcon.ShowBalloonTip(
                    3000,
                    "Drive Reconnect",
                    $"Reconnecting {failed.Count} failed drive(s)...",
                    ToolTipIcon.Info);
            }

            // Force Yellow for a fixed period during reconnection attempts.
            BeginForceYellowForReconnect();

            foreach (string driveRoot in failed)
            {
                if (_driveToScript.TryGetValue(driveRoot, out string? scriptPath) && !string.IsNullOrWhiteSpace(scriptPath))
                {
                    ExecutePowerShellScript(scriptPath, ToDriveLabel(driveRoot));
                }
            }
        }

        private static bool IsProcessAlive(Process process, DateTime startTime)
        {
            try
            {
                using var processInfo = Process.GetProcessById(process.Id);
                return processInfo.StartTime == startTime;
            }
            catch
            {
                return false;
            }
        }

        private static string SafeNotifyText(string text)
        {
            // NotifyIcon.Text is limited (typically 63 chars)
            const int maxLen = 63;
            return text.Length <= maxLen ? text : text.Substring(0, maxLen);
        }

        private static string ToDriveLabel(string driveRoot)
        {
            // "F:\" -> "F:/"
            string trimmed = driveRoot.Trim();
            if (trimmed.EndsWith("\\", StringComparison.Ordinal))
            {
                trimmed = trimmed.Substring(0, trimmed.Length - 1);
            }
            return trimmed.Replace("\\", "/") + "/";
        }

        private static string NormalizeDriveRoot(string driveRoot)
        {
            string trimmed = driveRoot.Trim();
            if (trimmed.Length == 2 && trimmed[1] == ':')
            {
                trimmed += "\\";
            }
            if (trimmed.Length == 3 && trimmed[1] == ':' && trimmed[2] == '\\')
            {
                return trimmed;
            }
            if (trimmed.EndsWith("\\", StringComparison.Ordinal) && trimmed.Length >= 3)
            {
                return trimmed;
            }

            // Best-effort fallback; still allow UNC or other roots if provided.
            return trimmed;
        }

        private static Dictionary<string, string> LoadDriveMappings()
        {
            // Reads config from the app folder so it works when published.
            string configPath = Path.Combine(AppContext.BaseDirectory, CONFIG_FILE_NAME);

            var mappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (!File.Exists(configPath)) return mappings;

            foreach (string rawLine in File.ReadAllLines(configPath))
            {
                string line = rawLine.Trim();
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#", StringComparison.Ordinal))
                {
                    continue;
                }

                // Format: <DriveRoot>|<ScriptPath>
                string[] parts = line.Split('|', 2);
                if (parts.Length != 2)
                {
                    continue;
                }

                string driveRoot = NormalizeDriveRoot(parts[0]);
                string scriptPath = parts[1].Trim();

                if (string.IsNullOrWhiteSpace(driveRoot) || string.IsNullOrWhiteSpace(scriptPath))
                {
                    continue;
                }

                mappings[driveRoot] = scriptPath;
            }

            return mappings;
        }

        private Icon CreateIcon(Color color)
        {
            // Create a simple 16x16 icon with the specified color
            Bitmap bitmap = new Bitmap(16, 16);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);
                
                // Draw filled circle
                using (SolidBrush brush = new SolidBrush(color))
                {
                    g.FillEllipse(brush, 2, 2, 12, 12);
                }
                
                // Draw border
                using (Pen pen = new Pen(Color.Black, 1))
                {
                    g.DrawEllipse(pen, 2, 2, 12, 12);
                }
            }

            IntPtr hIcon = bitmap.GetHicon();
            Icon icon = Icon.FromHandle(hIcon);
            return icon;
        }

        private void ExecutePowerShellScript(string scriptPath, string driveName)
        {
            try
            {
                if (!File.Exists(scriptPath))
                {
                    MessageBox.Show($"Script not found: {scriptPath}", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                ProcessStartInfo startInfo = new ProcessStartInfo()
                {
                    FileName = "powershell.exe",
                    Arguments = $"-ExecutionPolicy Bypass -File \"{scriptPath}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                Process? process = Process.Start(startInfo);
                
                _trayIcon.ShowBalloonTip(3000, "Drive Reconnect", 
                    $"Reconnecting {driveName}...", ToolTipIcon.Info);

                // Force Yellow for a fixed period during reconnection attempts.
                BeginForceYellowForReconnect();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error executing script: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnExit(object? sender, EventArgs e)
        {
            _shutdownCts.Cancel();
            _checkTimer.Stop();
            _autoReconnectTimer.Stop();
            _trayIcon.Visible = false;
            Application.Exit();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try { _shutdownCts.Cancel(); } catch { }
                _shutdownCts.Dispose();
                _checkTimer?.Dispose();
                _autoReconnectTimer?.Dispose();
                _forceYellowTimer?.Dispose();
                _trayIcon?.Dispose();
                _contextMenu?.Dispose();
            }
            base.Dispose(disposing);
        }

        private class ProbeInfo
        {
            public Process Process { get; }
            public DateTime StartTime { get; }

            public ProbeInfo(Process process)
            {
                Process = process;
                StartTime = process.StartTime;
            }
        }
    }
}

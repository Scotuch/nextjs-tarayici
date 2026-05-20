using System.Windows.Forms;
using Microsoft.Win32;

namespace NextJS_Tarayici
{
    public class AppSettings
    {
        private const string RegKey = @"Software\NextJSTarayici\Settings";
        private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string AppName = "NextJSTarayici";

        // Başlangıç
        public bool StartWithWindows { get; set; }
        public bool StartMinimized { get; set; }
        public bool StartFullscreen { get; set; }
        public string StartupUrl { get; set; } = "http://localhost:3000";

        // Davranış
        public bool CloseToTray { get; set; } = true;    // X → tray, sağ tık → gerçek kapat
        public bool ConfirmOnClose { get; set; }             // gerçek kapanışta onay
        public bool AllowExternalDrop { get; set; } = true;
        public bool AutoReload { get; set; }
        public int AutoReloadDelaySec { get; set; } = 3;
        public bool ClearCacheOnExit { get; set; }   // ← YENİ

        // Görünüm
        public double DefaultZoom { get; set; } = 1.0;

        public static AppSettings Load()
        {
            var s = new AppSettings();
            try
            {
                using var k = Registry.CurrentUser.OpenSubKey(RegKey);
                if (k != null)
                {
                    s.StartMinimized = Int(k, "StartMinimized") == 1;
                    s.StartFullscreen = Int(k, "StartFullscreen") == 1;
                    s.StartupUrl = (k.GetValue("StartupUrl") as string) ?? s.StartupUrl;
                    s.CloseToTray = Int(k, "CloseToTray", 1) == 1;
                    s.ConfirmOnClose = Int(k, "ConfirmOnClose") == 1;
                    s.AllowExternalDrop = Int(k, "AllowExternalDrop", 1) == 1;
                    s.AutoReload = Int(k, "AutoReload") == 1;
                    s.AutoReloadDelaySec = Int(k, "AutoReloadDelay", 3);
                    s.ClearCacheOnExit = Int(k, "ClearCacheOnExit") == 1;
                    if (double.TryParse(k.GetValue("DefaultZoom") as string,
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out var z))
                        s.DefaultZoom = z;
                }
                using var run = Registry.CurrentUser.OpenSubKey(RunKey);
                s.StartWithWindows = run?.GetValue(AppName) != null;
            }
            catch { }
            return s;
        }

        public void Save()
        {
            try
            {
                using var k = Registry.CurrentUser.CreateSubKey(RegKey);
                k.SetValue("StartMinimized", StartMinimized ? 1 : 0);
                k.SetValue("StartFullscreen", StartFullscreen ? 1 : 0);
                k.SetValue("StartupUrl", StartupUrl);
                k.SetValue("CloseToTray", CloseToTray ? 1 : 0);
                k.SetValue("ConfirmOnClose", ConfirmOnClose ? 1 : 0);
                k.SetValue("AllowExternalDrop", AllowExternalDrop ? 1 : 0);
                k.SetValue("AutoReload", AutoReload ? 1 : 0);
                k.SetValue("AutoReloadDelay", AutoReloadDelaySec);
                k.SetValue("ClearCacheOnExit", ClearCacheOnExit ? 1 : 0);
                k.SetValue("DefaultZoom",
                    DefaultZoom.ToString("F2", System.Globalization.CultureInfo.InvariantCulture));
            }
            catch { }

            try
            {
                using var run = Registry.CurrentUser.OpenSubKey(RunKey, writable: true);
                if (run == null) return;
                if (StartWithWindows)
                    run.SetValue(AppName, $"\"{Application.ExecutablePath}\" -tray");
                else
                    run.DeleteValue(AppName, throwOnMissingValue: false);
            }
            catch { }
        }

        private static int Int(RegistryKey k, string n, int def = 0)
            => k.GetValue(n) is int v ? v : def;
    }
}
#nullable enable
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Win32;

namespace NextJS_Tarayici
{
    public partial class Form1 : Form
    {
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int v, int size);
        private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
        private const int DWMWCP_ROUND = 2;

        private static class T
        {
            public static readonly Color FormBg = Color.FromArgb(18, 18, 18);
            public static readonly Color Toolbar = Color.FromArgb(26, 26, 28);
            public static readonly Color Group = Color.FromArgb(38, 38, 40);
            public static readonly Color BtnHover = Color.FromArgb(55, 55, 58);
            public static readonly Color BtnPressed = Color.FromArgb(68, 68, 72);
            public static readonly Color UrlBg = Color.FromArgb(38, 38, 40);
            public static readonly Color UrlFocus = Color.FromArgb(44, 44, 48);
            public static readonly Color Accent = Color.FromArgb(0, 120, 215);
            public static readonly Color TextSub = Color.FromArgb(130, 130, 135);
            public static readonly Color ToolbarLine = Color.FromArgb(40, 40, 43);
            public static readonly Color SecureGreen = Color.FromArgb(52, 199, 89);
        }

        private AppSettings appSettings = null!;
        private string targetUrl = "http://localhost:3000";
        private bool isFullscreen = false;
        private bool startInTray = false;
        private bool startFull = false;
        private bool urlFocused = false;

        // trayIcon field seviyesinde başlatılıyor — SetVisibleCore'da null olmaz
        private NotifyIcon trayIcon = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            Text = "NextJS Tarayıcı",
            Visible = false
        };

        private readonly double[] zoomLevels = { 0.5, 0.67, 0.75, 0.9, 1.0, 1.1, 1.25, 1.5, 1.75, 2.0 };
        private int currentZoomIdx = 4;

        private System.Windows.Forms.Timer? autoReloadTimer;
        private bool _trayNotifiedOnce = false;

        // ── Kurucu ────────────────────────────────────────────────────────────
        public Form1()
        {
            InitializeComponent();
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            this.KeyPreview = true;
            this.KeyDown += Form1_KeyDown;

            appSettings = AppSettings.Load();
            targetUrl = appSettings.StartupUrl;
            startInTray = appSettings.StartMinimized;
            startFull = appSettings.StartFullscreen;

            ParseCommandLineArguments();
            SetupTrayIcon();
            RestoreWindowBounds();
            SetupInteractions();

            this.Load += Form1_Load;
            this.FormClosing += Form1_FormClosing;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            try { int v = DWMWCP_ROUND; DwmSetWindowAttribute(Handle, DWMWA_WINDOW_CORNER_PREFERENCE, ref v, 4); }
            catch { }
        }

        // Form hiç görünmeden tray'e gider — ekranda anlık flaş olmaz
        protected override void SetVisibleCore(bool value)
        {
            if (startInTray && !IsHandleCreated)
            {
                CreateHandle();
                trayIcon.Visible = true;   // artık asla null değil
                base.SetVisibleCore(false);
                return;
            }
            base.SetVisibleCore(value);
        }

        private void SetupInteractions()
        {
            foreach (var btn in new[] { btnBack, btnForward, btnRefresh, btnSettings, btnDevTools, btnFullscreen, btnTray })
            {
                btn.MouseEnter += (s, _) => ((Button)s!).BackColor = T.BtnHover;
                btn.MouseLeave += (s, _) => ((Button)s!).BackColor = Color.Transparent;
                btn.MouseDown += (s, _) => ((Button)s!).BackColor = T.BtnPressed;
                btn.MouseUp += (s, _) => ((Button)s!).BackColor = T.BtnHover;
            }
            txtUrl.GotFocus += (_, _) => { urlFocused = true; pnlUrl.Invalidate(); txtUrl.SelectAll(); };
            txtUrl.LostFocus += (_, _) => { urlFocused = false; pnlUrl.Invalidate(); };
            pnlUrl.Click += (_, _) => txtUrl.Focus();
            lblProtocol.Click += (_, _) => txtUrl.Focus();
        }

        private void pnlUrl_Paint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = new Rectangle(0, 0, pnlUrl.Width - 1, pnlUrl.Height - 1);
            using var path = Rounded(rect, 7);
            using var brush = new SolidBrush(urlFocused ? T.UrlFocus : T.UrlBg);
            g.FillPath(brush, path);
            if (urlFocused)
            {
                using var pen = new Pen(T.Accent, 1.5f);
                g.DrawPath(pen, path);
            }
        }

        private void pnlNavGroup_Paint(object? sender, PaintEventArgs e) => DrawGroup(e, pnlNavGroup);
        private void pnlActionGroup_Paint(object? sender, PaintEventArgs e) => DrawGroup(e, pnlActionGroup);

        private void DrawGroup(PaintEventArgs e, Panel p)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = new Rectangle(0, 0, p.Width - 1, p.Height - 1);
            using var path = Rounded(rect, 8);
            using var brush = new SolidBrush(T.Group);
            g.FillPath(brush, path);
        }

        private void pnlToolbar_Paint(object? sender, PaintEventArgs e)
        {
            using var pen = new Pen(T.ToolbarLine, 1);
            e.Graphics.DrawLine(pen, 0, pnlToolbar.Height - 1, pnlToolbar.Width, pnlToolbar.Height - 1);
        }

        private static GraphicsPath Rounded(Rectangle b, int r)
        {
            var p = new GraphicsPath(); int d = r * 2;
            p.AddArc(b.X, b.Y, d, d, 180, 90);
            p.AddArc(b.Right - d, b.Y, d, d, 270, 90);
            p.AddArc(b.Right - d, b.Bottom - d, d, d, 0, 90);
            p.AddArc(b.X, b.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }

        private void SetupTrayIcon()
        {
            // trayIcon field'da zaten oluşturuldu, sadece menüyü bağla
            var menu = new ContextMenuStrip();
            menu.Items.Add("Göster", null, (_, _) => GeriYukle());
            menu.Items.Add("Yenile", null, (_, _) => webView21.Reload());
            menu.Items.Add("Ayarlar", null, (_, _) => AyarlariAc());
            menu.Items.Add("-");
            menu.Items.Add("Kapat", null, (_, _) => ExitApp());
            trayIcon.ContextMenuStrip = menu;
            trayIcon.MouseDoubleClick += (_, _) => GeriYukle();
        }

        private void RestoreWindowBounds()
        {
            try
            {
                using var k = Registry.CurrentUser.OpenSubKey(@"Software\NextJSTarayici");
                if (k == null) return;
                int x = (int)(k.GetValue("X", 80) ?? 80), y = (int)(k.GetValue("Y", 80) ?? 80);
                int w = (int)(k.GetValue("W", 1280) ?? 1280), h = (int)(k.GetValue("H", 800) ?? 800);
                if (Screen.GetWorkingArea(new Point(x, y)).Contains(x, y))
                {
                    StartPosition = FormStartPosition.Manual;
                    Location = new Point(x, y);
                    Size = new Size(Math.Max(w, 700), Math.Max(h, 450));
                }
            }
            catch { }
        }

        private void SaveWindowBounds()
        {
            if (isFullscreen || WindowState != FormWindowState.Normal) return;
            try
            {
                using var k = Registry.CurrentUser.CreateSubKey(@"Software\NextJSTarayici");
                k.SetValue("X", Location.X); k.SetValue("Y", Location.Y);
                k.SetValue("W", Width); k.SetValue("H", Height);
            }
            catch { }
        }

        private void ParseCommandLineArguments()
        {
            var args = Environment.GetCommandLineArgs();
            for (int i = 1; i < args.Length; i++)
            {
                var a = args[i].ToLower();
                if ((a == "-url" || a == "--url") && i + 1 < args.Length)
                { var u = args[++i]; targetUrl = u.StartsWith("http") ? u : "http://" + u; }
                else if (a is "-fullscreen" or "-f") startFull = true;
                else if (a is "-tray" or "-min") startInTray = true;
            }
        }

        private async void Form1_Load(object? sender, EventArgs e)
        {
            // startInTray → SetVisibleCore handle ediyor, burada gerekmez
            if (startFull) ToggleFullscreen();

            for (int i = 0; i < zoomLevels.Length; i++)
                if (Math.Abs(zoomLevels[i] - appSettings.DefaultZoom) < 0.01)
                { currentZoomIdx = i; break; }

            autoReloadTimer = new System.Windows.Forms.Timer { Interval = Math.Max(1, appSettings.AutoReloadDelaySec) * 1000 };
            autoReloadTimer.Tick += (_, _) => { autoReloadTimer.Stop(); webView21.Reload(); };

            try
            {
                await webView21.EnsureCoreWebView2Async(null);
                webView21.AllowExternalDrop = appSettings.AllowExternalDrop;

                webView21.CoreWebView2.DocumentTitleChanged += (_, _) =>
                {
                    var t = webView21.CoreWebView2.DocumentTitle;
                    Text = string.IsNullOrEmpty(t) ? "NextJS Tarayıcı" : $"{t} — NextJS Tarayıcı";
                    trayIcon.Text = Text.Length > 63 ? Text[..63] : Text;
                };

                webView21.CoreWebView2.NavigationStarting += (_, ev) =>
                { progressBar.Visible = true; progressBar.Value = 15; SetProtocol(ev.Uri); };
                webView21.CoreWebView2.ContentLoading += (_, _) =>
                { if (progressBar.Value < 65) progressBar.Value = 65; };
                webView21.CoreWebView2.NavigationCompleted += (_, ev) =>
                {
                    progressBar.Visible = false; progressBar.Value = 0;
                    if (!ev.IsSuccess && appSettings.AutoReload)
                    {
                        autoReloadTimer.Interval = Math.Max(1, appSettings.AutoReloadDelaySec) * 1000;
                        autoReloadTimer.Start();
                    }
                };

                webView21.SourceChanged += WebView21_SourceChanged;
                webView21.Source = new Uri(targetUrl);
                txtUrl.Text = targetUrl;
                ApplyZoom();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Tarayıcı başlatılamadı:\n{ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetProtocol(string uri)
        {
            bool s = uri.StartsWith("https://");
            lblProtocol.Text = s ? "🔒" : "🌐";
            lblProtocol.ForeColor = s ? T.SecureGreen : T.TextSub;
        }

        // ── Ayarlar ───────────────────────────────────────────────────────────
        private async void AyarlariAc()
        {
            using var form = new SettingsForm(appSettings);
            if (form.ShowDialog(this) != DialogResult.OK) return;

            webView21.AllowExternalDrop = appSettings.AllowExternalDrop;
            for (int i = 0; i < zoomLevels.Length; i++)
                if (Math.Abs(zoomLevels[i] - appSettings.DefaultZoom) < 0.01)
                { currentZoomIdx = i; break; }
            ApplyZoom();
            if (autoReloadTimer != null)
                autoReloadTimer.Interval = Math.Max(1, appSettings.AutoReloadDelaySec) * 1000;

            // Önbellek / çerez temizle
            if ((form.ShouldClearCache || form.ShouldClearCookies) && webView21.CoreWebView2 != null)
            {
                try
                {
                    if (form.ShouldClearCookies)
                        webView21.CoreWebView2.CookieManager.DeleteAllCookies();

                    if (form.ShouldClearCache)
                    {
                        // AllCache yok — DiskCache + CacheStorage kullan
                        await webView21.CoreWebView2.Profile.ClearBrowsingDataAsync(
                            CoreWebView2BrowsingDataKinds.DiskCache |
                            CoreWebView2BrowsingDataKinds.CacheStorage);
                    }

                    MessageBox.Show("Temizleme tamamlandı.", "Bitti",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    webView21.Reload();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Temizleme hatası:\n{ex.Message}", "Hata",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        // ── Klavye Kısayolları ────────────────────────────────────────────────
        private void Form1_KeyDown(object? sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.F1: AyarlariAc(); e.Handled = true; break;
                case Keys.F11: ToggleFullscreen(); e.Handled = true; break;
                case Keys.F12: webView21.CoreWebView2?.OpenDevToolsWindow(); e.Handled = true; break;
                case Keys.R when e.Control: webView21.Reload(); e.Handled = e.SuppressKeyPress = true; break;
                case Keys.L when e.Control: txtUrl.Focus(); txtUrl.SelectAll(); e.Handled = e.SuppressKeyPress = true; break;
                case Keys.Left when e.Alt: if (webView21.CanGoBack) webView21.GoBack(); e.Handled = true; break;
                case Keys.Right when e.Alt: if (webView21.CanGoForward) webView21.GoForward(); e.Handled = true; break;
                case Keys.Oemplus when e.Control:
                case Keys.Add when e.Control: ZoomIn(); e.Handled = e.SuppressKeyPress = true; break;
                case Keys.OemMinus when e.Control:
                case Keys.Subtract when e.Control: ZoomOut(); e.Handled = e.SuppressKeyPress = true; break;
                case Keys.D0 when e.Control:
                case Keys.NumPad0 when e.Control: ResetZoom(); e.Handled = e.SuppressKeyPress = true; break;
            }
        }

        private void ZoomIn() { if (currentZoomIdx < zoomLevels.Length - 1) { currentZoomIdx++; ApplyZoom(); } }
        private void ZoomOut() { if (currentZoomIdx > 0) { currentZoomIdx--; ApplyZoom(); } }
        private void ResetZoom() { currentZoomIdx = 4; ApplyZoom(); }
        private void ApplyZoom()
        {
            webView21.ZoomFactor = zoomLevels[currentZoomIdx];
            lblZoom.Text = $"%{(int)(zoomLevels[currentZoomIdx] * 100)}";
        }

        private void ToggleFullscreen()
        {
            isFullscreen = !isFullscreen;
            FormBorderStyle = isFullscreen ? FormBorderStyle.None : FormBorderStyle.Sizable;
            WindowState = isFullscreen ? FormWindowState.Maximized : FormWindowState.Normal;
            pnlToolbar.Visible = !isFullscreen;
        }

        private void GeriYukle()
        {
            Show(); WindowState = FormWindowState.Normal; Activate();
            trayIcon.Visible = false;
        }

        private void WebView21_SourceChanged(object? sender, CoreWebView2SourceChangedEventArgs e)
        {
            var url = webView21.Source.ToString();
            void Update() { txtUrl.Text = url; SetProtocol(url); }
            if (InvokeRequired) Invoke(Update); else Update();
        }

        private void btnBack_Click(object? sender, EventArgs e) { if (webView21.CanGoBack) webView21.GoBack(); }
        private void btnForward_Click(object? sender, EventArgs e) { if (webView21.CanGoForward) webView21.GoForward(); }
        private void btnRefresh_Click(object? sender, EventArgs e) { webView21.Reload(); }
        private void btnSettings_Click(object? sender, EventArgs e) { AyarlariAc(); }
        private void btnFullscreen_Click(object? sender, EventArgs e) { ToggleFullscreen(); }
        private void btnDevTools_Click(object? sender, EventArgs e) { webView21.CoreWebView2?.OpenDevToolsWindow(); }
        private void btnTray_Click(object? sender, EventArgs e) { Hide(); trayIcon.Visible = true; }

        private void txtUrl_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            var url = txtUrl.Text.Trim();
            if (!url.StartsWith("http")) url = "http://" + url;
            try { webView21.Source = new Uri(url); }
            catch (UriFormatException) { MessageBox.Show("Geçersiz URL.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
            e.SuppressKeyPress = true;
        }

        // Gerçek kapanma — tray menüsünden veya ayardan "X kapat" seçiliyken çağrılır
        private async void ExitApp()
        {
            if (appSettings.ConfirmOnClose)
            {
                var r = MessageBox.Show("Uygulamayı kapatmak istiyor musunuz?",
                    "Kapat", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (r == DialogResult.No) return;
            }

            if (appSettings.ClearCacheOnExit && webView21.CoreWebView2 != null)
            {
                try
                {
                    await webView21.CoreWebView2.Profile.ClearBrowsingDataAsync(
                        CoreWebView2BrowsingDataKinds.DiskCache |
                        CoreWebView2BrowsingDataKinds.CacheStorage);
                }
                catch { }
            }

            autoReloadTimer?.Stop();
            SaveWindowBounds();
            trayIcon.Visible = false;
            Application.Exit();
        }

        private async void Form1_FormClosing(object? sender, FormClosingEventArgs e)
        {
            // Yalnızca kullanıcı X'e bastıysa tray'e küçült
            if (e.CloseReason == CloseReason.UserClosing && appSettings.CloseToTray)
            {
                e.Cancel = true;          // kapanmayı iptal et
                Hide();                   // pencereyi gizle
                trayIcon.Visible = true; // tray ikonunu göster

                // İlk kez tray'e gidince bildirim göster
                if (!_trayNotifiedOnce)
                {
                    _trayNotifiedOnce = true;
                    trayIcon.ShowBalloonTip(
                        timeout: 2500,
                        tipTitle: "NextJS Tarayıcı",
                        tipText: "Uygulama arka planda çalışmaya devam ediyor.",
                        tipIcon: ToolTipIcon.Info);
                }
                return;
            }

            // Gerçek kapanma (Application.Exit veya görev yöneticisi)
            e.Cancel = false;
            autoReloadTimer?.Stop();
            SaveWindowBounds();
            trayIcon.Visible = false;

            if (appSettings.ClearCacheOnExit && webView21.CoreWebView2 != null)
            {
                try
                {
                    await webView21.CoreWebView2.Profile.ClearBrowsingDataAsync(
                        CoreWebView2BrowsingDataKinds.DiskCache |
                        CoreWebView2BrowsingDataKinds.CacheStorage);
                }
                catch { }
            }
        }
    }
}
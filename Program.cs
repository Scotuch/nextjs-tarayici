#nullable enable
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;

namespace NextJS_Tarayici
{
    internal static class Program
    {
        private const string WV2_DOWNLOAD = "https://developer.microsoft.com/microsoft-edge/webview2/";

        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            // ── WebView2 Runtime kontrolü ─────────────────────────────────────
            if (!IsWebView2Available(out string wv2Version))
            {
                ShowMissingDialog();
                return; // WebView2 yoksa uygulamayı başlatma
            }

            // ── Normal başlangıç ──────────────────────────────────────────────
            Application.Run(new Form1());
        }

        // ─────────────────────────────────────────────────────────────────────
        // WebView2 kurulu mu?
        // ─────────────────────────────────────────────────────────────────────
        private static bool IsWebView2Available(out string version)
        {
            try
            {
                version = CoreWebView2Environment.GetAvailableBrowserVersionString();
                return !string.IsNullOrWhiteSpace(version);
            }
            catch
            {
                version = string.Empty;
                return false;
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // Hata diyaloğu — karanlık tema, indirme yönlendirmesi
        // ─────────────────────────────────────────────────────────────────────
        private static void ShowMissingDialog()
        {
            Application.Run(new WebView2MissingForm());
        }
    }

    // ═════════════════════════════════════════════════════════════════════════
    // WebView2 bulunamadı — özel hata formu
    // ═════════════════════════════════════════════════════════════════════════
    internal sealed class WebView2MissingForm : Form
    {
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int v, int size);

        private const string DOWNLOAD_URL = "https://developer.microsoft.com/microsoft-edge/webview2/";

        public WebView2MissingForm()
        {
            // Form özellikleri
            FormBorderStyle = FormBorderStyle.None;
            BackColor = Color.FromArgb(20, 20, 23);
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(480, 300);
            ShowInTaskbar = true;
            Text = "NextJS Tarayıcı — Hata";

            BuildUI();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            // Windows 11 yuvarlak köşe
            try { int v = 2; DwmSetWindowAttribute(Handle, 33, ref v, 4); } catch { }
        }

        // Başlık alanından sürükle
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == 0x0084)
            {
                var p = PointToClient(new Point(m.LParam.ToInt32() & 0xFFFF,
                                                (m.LParam.ToInt32() >> 16) & 0xFFFF));
                if (p.Y <= 56) m.Result = new IntPtr(2);
            }
        }

        private void BuildUI()
        {
            // ── Üst renkli çizgi (uyarı rengi) ───────────────────────────────
            var topBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 4,
                BackColor = Color.FromArgb(207, 77, 42)   // turuncu-kırmızı
            };

            // ── İkon alanı ────────────────────────────────────────────────────
            var pnlIcon = new Panel
            {
                Location = new Point(32, 32),
                Size = new Size(56, 56),
                BackColor = Color.FromArgb(207, 77, 42, 30)
            };
            pnlIcon.Paint += (_, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                // Daire arka plan
                using var bgBrush = new SolidBrush(Color.FromArgb(60, 207, 77, 42));
                g.FillEllipse(bgBrush, 0, 0, 55, 55);
                // Uyarı "!" karakteri
                using var font = new Font("Segoe UI", 26F, FontStyle.Bold);
                using var brush = new SolidBrush(Color.FromArgb(207, 77, 42));
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString("!", font, brush, new RectangleF(0, 0, 56, 56), sf);
            };

            // ── Başlık ────────────────────────────────────────────────────────
            var lblTitle = new Label
            {
                Text = "WebView2 Runtime Bulunamadı",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.FromArgb(230, 230, 235),
                BackColor = Color.Transparent,
                Location = new Point(104, 36),
                Size = new Size(350, 28),
                AutoSize = false
            };

            // ── Açıklama ──────────────────────────────────────────────────────
            var lblDesc = new Label
            {
                Text = "Bu uygulama Microsoft WebView2 Runtime gerektirir.\n" +
                       "WebView2, Windows 11'de varsayılan olarak bulunur;\n" +
                       "Windows 10 için ayrıca yüklenmelidir.",
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Color.FromArgb(155, 155, 165),
                BackColor = Color.Transparent,
                Location = new Point(104, 72),
                Size = new Size(352, 64),
                AutoSize = false
            };

            // ── Ayraç ─────────────────────────────────────────────────────────
            var divider = new Panel
            {
                BackColor = Color.FromArgb(40, 40, 50),
                Location = new Point(32, 152),
                Size = new Size(416, 1)
            };

            // ── İndirme linki bilgisi ─────────────────────────────────────────
            var lblUrl = new Label
            {
                Text = DOWNLOAD_URL,
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = Color.FromArgb(0, 122, 204),
                BackColor = Color.Transparent,
                Location = new Point(32, 164),
                Size = new Size(416, 18),
                Cursor = Cursors.Hand
            };
            lblUrl.Click += (_, _) => OpenUrl(DOWNLOAD_URL);

            // ── Butonlar ──────────────────────────────────────────────────────
            var btnDownload = new Button
            {
                Text = "İndir — WebView2 Runtime",
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(0, 122, 204),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Location = new Point(32, 200),
                Size = new Size(220, 40)
            };
            btnDownload.FlatAppearance.BorderSize = 0;
            btnDownload.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 100, 180);
            btnDownload.Click += (_, _) =>
            {
                OpenUrl(DOWNLOAD_URL);
                // İndir butonuna basılınca uygulama açık kalsın,
                // kullanıcı yükledikten sonra yeniden başlatabilir
            };

            var btnRetry = new Button
            {
                Text = "Yeniden Dene",
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(180, 180, 190),
                BackColor = Color.FromArgb(44, 44, 52),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Location = new Point(264, 200),
                Size = new Size(130, 40)
            };
            btnRetry.FlatAppearance.BorderSize = 1;
            btnRetry.FlatAppearance.BorderColor = Color.FromArgb(62, 62, 74);
            btnRetry.FlatAppearance.MouseOverBackColor = Color.FromArgb(58, 58, 68);
            btnRetry.Click += (_, _) =>
            {
                // WebView2 kurulduysa uygulamayı yeniden başlat
                try
                {
                    var ver = CoreWebView2Environment.GetAvailableBrowserVersionString();
                    if (!string.IsNullOrWhiteSpace(ver))
                    {
                        Application.Restart();
                        return;
                    }
                }
                catch { }

                // Hâlâ yüklü değil
                lblTitle.Text = "Henüz Kurulmamış";
                lblTitle.ForeColor = Color.FromArgb(207, 120, 42);
            };

            var btnClose = new Button
            {
                Text = "Kapat",
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(130, 130, 140),
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Location = new Point(406, 200),
                Size = new Size(72, 40)
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.FlatAppearance.MouseOverBackColor = Color.FromArgb(40, 40, 50);
            btnClose.Click += (_, _) => Application.Exit();

            // ── Küçük kapat (X) ───────────────────────────────────────────────
            var btnX = new Button
            {
                Text = "✕",
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(100, 100, 110),
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Location = new Point(444, 8),
                Size = new Size(28, 28)
            };
            btnX.FlatAppearance.BorderSize = 0;
            btnX.FlatAppearance.MouseOverBackColor = Color.FromArgb(196, 43, 28);
            btnX.MouseEnter += (_, _) => btnX.ForeColor = Color.White;
            btnX.MouseLeave += (_, _) => btnX.ForeColor = Color.FromArgb(100, 100, 110);
            btnX.Click += (_, _) => Application.Exit();

            Controls.AddRange(new Control[]
            {
                topBar, pnlIcon, lblTitle, lblDesc,
                divider, lblUrl,
                btnDownload, btnRetry, btnClose, btnX
            });
        }

        private static void OpenUrl(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch { }
        }
    }
}
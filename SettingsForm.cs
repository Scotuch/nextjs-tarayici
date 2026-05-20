#nullable enable
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace NextJS_Tarayici
{
    public class SettingsForm : Form
    {
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int v, int size);

        // ── Tema ──────────────────────────────────────────────────────────────
        private static class T
        {
            public static readonly Color FormBg = Color.FromArgb(20, 20, 23);
            public static readonly Color TitleBg = Color.FromArgb(28, 28, 32);
            public static readonly Color Input = Color.FromArgb(36, 36, 42);
            public static readonly Color KeyBadge = Color.FromArgb(45, 45, 56);
            public static readonly Color Accent = Color.FromArgb(0, 122, 204);
            public static readonly Color AccentHov = Color.FromArgb(0, 100, 180);
            public static readonly Color BtnSec = Color.FromArgb(44, 44, 52);
            public static readonly Color BtnSecBor = Color.FromArgb(62, 62, 74);
            public static readonly Color TextMain = Color.FromArgb(222, 222, 228);
            public static readonly Color TextSub = Color.FromArgb(138, 138, 150);
            public static readonly Color TextHint = Color.FromArgb(80, 80, 94);
            public static readonly Color Divider = Color.FromArgb(40, 40, 50);
            public static readonly Color CloseHov = Color.FromArgb(196, 43, 28);
            public static readonly Color WarnBg = Color.FromArgb(55, 35, 18);
            public static readonly Color WarnBor = Color.FromArgb(180, 90, 20);
            public static readonly Color WarnText = Color.FromArgb(220, 160, 80);

            // Scrollbar
            public static readonly Color ScrollTrack = Color.FromArgb(28, 28, 34);
            public static readonly Color ScrollThumb = Color.FromArgb(65, 65, 78);
            public static readonly Color ScrollHover = Color.FromArgb(88, 88, 105);
            public static readonly Color ScrollDrag = Color.FromArgb(110, 110, 130);
        }

        // ═════════════════════════════════════════════════════════════════════
        // CUSTOM DARK SCROLL CONTAINER
        // İçerik paneli absolute-positioned olarak taşınır.
        // Windows sistem scroll yoktur, tamamen custom çizim yapılır.
        // ═════════════════════════════════════════════════════════════════════
        private sealed class DarkScroll : Panel
        {
            // ── Scrollbar boyutları ───────────────────────────────────────────
            private const int BAR_W = 5;   // scrollbar şerit genişliği
            private const int THUMB_MH = 28;  // minimum thumb yüksekliği
            private const int SCROLL_SPEED = 40; // piksel / wheel notch

            // ── Alt kontroller ────────────────────────────────────────────────
            private readonly Panel _content;  // tüm içerik bu panelde
            private readonly Panel _track;    // scrollbar şerit arka planı
            private readonly Panel _thumb;    // sürüklenebilir thumb

            // ── Sürükleme durumu ──────────────────────────────────────────────
            private bool _dragging;
            private int _dragStartY;
            private int _dragStartScrollY;
            private Color _thumbColor;

            public Panel Content => _content;

            // ─────────────────────────────────────────────────────────────────
            public DarkScroll()
            {
                DoubleBuffered = true;
                _thumbColor = T.ScrollThumb;

                // İçerik paneli — sistem scroll olmadan manuel kaydırılır
                _content = new Panel
                {
                    AutoSize = false,
                    BackColor = T.FormBg,
                    Location = new Point(0, 0)
                };

                // Scrollbar şerit
                _track = new Panel { BackColor = T.ScrollTrack, Width = BAR_W };
                _track.MouseDown += Track_Click;

                // Thumb — yuvarlak köşeli, custom paint
                _thumb = new Panel { BackColor = Color.Transparent };
                _thumb.Paint += Thumb_Paint;
                _thumb.MouseEnter += (_, _) => SetThumbColor(T.ScrollHover);
                _thumb.MouseLeave += (_, _) => { if (!_dragging) SetThumbColor(T.ScrollThumb); };
                _thumb.MouseDown += Thumb_Down;
                _thumb.MouseMove += Thumb_Move;
                _thumb.MouseUp += Thumb_Up;
                _track.Controls.Add(_thumb);

                Controls.Add(_content);
                Controls.Add(_track);

                Resize += (_, _) => UpdateLayout();
                MouseWheel += (_, e) => Scroll(-e.Delta / 120 * SCROLL_SPEED);

                // İçeriğe eklenen tüm alt kontrollerin wheel event'ini yakala
                _content.ControlAdded += (_, e) => HookWheel(e.Control);
            }

            // ── İçerik yüksekliği atandıktan sonra çağırılmalı ───────────────
            public void Refresh() => UpdateLayout();

            // ─────────────────────────────────────────────────────────────────
            // SCROLL HESABI
            // ─────────────────────────────────────────────────────────────────

            private int ContentH => _content.Height;
            private int ViewH => Height;
            private int MaxScroll => Math.Max(0, ContentH - ViewH);
            private int CurrentY => -_content.Top;

            private void Scroll(int delta)
            {
                SetScrollY(CurrentY + delta);
            }

            private void SetScrollY(int y)
            {
                y = Math.Clamp(y, 0, MaxScroll);
                _content.Top = -y;
                UpdateThumb();
            }

            private void UpdateLayout()
            {
                bool show = ContentH > ViewH;
                _track.Visible = show;
                _track.Location = new Point(Width - BAR_W, 0);
                _track.Size = new Size(BAR_W, Height);
                _content.Width = show ? Width - BAR_W - 1 : Width;
                UpdateThumb();
            }

            private void UpdateThumb()
            {
                if (MaxScroll <= 0) { _thumb.Visible = false; return; }
                _thumb.Visible = true;

                int trackH = _track.Height;
                int thumbH = Math.Max(THUMB_MH, (int)((double)ViewH / ContentH * trackH));
                int thumbY = (int)((double)CurrentY / MaxScroll * (trackH - thumbH));

                _thumb.Size = new Size(BAR_W, thumbH);
                _thumb.Location = new Point(0, Math.Clamp(thumbY, 0, trackH - thumbH));
            }

            // ─────────────────────────────────────────────────────────────────
            // THUMB OLAYLARI
            // ─────────────────────────────────────────────────────────────────

            private void Thumb_Down(object? s, MouseEventArgs e)
            {
                if (e.Button != MouseButtons.Left) return;
                _dragging = true;
                _dragStartY = _thumb.PointToScreen(e.Location).Y;
                _dragStartScrollY = CurrentY;
                _thumb.Capture = true;
                SetThumbColor(T.ScrollDrag);
            }

            private void Thumb_Move(object? s, MouseEventArgs e)
            {
                if (!_dragging) return;
                int screenY = _thumb.PointToScreen(e.Location).Y;
                int pixDelta = screenY - _dragStartY;
                int trackH = _track.Height;
                int thumbH = _thumb.Height;
                double ratio = (double)pixDelta / (trackH - thumbH);
                SetScrollY(_dragStartScrollY + (int)(ratio * MaxScroll));
            }

            private void Thumb_Up(object? s, MouseEventArgs e)
            {
                _dragging = false;
                _thumb.Capture = false;
                SetThumbColor(T.ScrollHover); // mouse hâlâ üzerinde olabilir
            }

            // Track'e tıklayınca o konuma zıpla
            private void Track_Click(object? s, MouseEventArgs e)
            {
                if (e.Button != MouseButtons.Left) return;
                int trackH = _track.Height;
                int thumbH = _thumb.Height;
                double ratio = (double)(e.Y - thumbH / 2) / Math.Max(1, trackH - thumbH);
                SetScrollY((int)(ratio * MaxScroll));
            }

            // ─────────────────────────────────────────────────────────────────
            // THUMB ÇİZİMİ — yuvarlak köşeli, renkli
            // ─────────────────────────────────────────────────────────────────
            private void Thumb_Paint(object? s, PaintEventArgs e)
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(T.ScrollTrack); // arka plan = track rengi

                // Kenarlardan 1px boşluk bırak, 2px üstten/alttan padding
                var r = new Rectangle(1, 2, BAR_W - 2, _thumb.Height - 4);
                using var path = RRect(r, 2);
                using var brush = new SolidBrush(_thumbColor);
                g.FillPath(brush, path);
            }

            private void SetThumbColor(Color c)
            {
                _thumbColor = c;
                _thumb.Invalidate();
            }

            // ─────────────────────────────────────────────────────────────────
            // TÜM ALT KONTROLLERİN WHEEL EVENT'İNİ YÖNLENDIR
            // ─────────────────────────────────────────────────────────────────
            private void HookWheel(Control ctrl)
            {
                // TrackBar kendi scroll'unu kullanır — dokunma
                if (ctrl is not TrackBar && ctrl is not NumericUpDown)
                    ctrl.MouseWheel += (_, e) => Scroll(-e.Delta / 120 * SCROLL_SPEED);

                ctrl.ControlAdded += (_, ev) => HookWheel(ev.Control);
                foreach (Control child in ctrl.Controls)
                    HookWheel(child);
            }

            private static GraphicsPath RRect(Rectangle b, int r)
            {
                var p = new GraphicsPath(); int d = r * 2;
                p.AddArc(b.X, b.Y, d, d, 180, 90);
                p.AddArc(b.Right - d, b.Y, d, d, 270, 90);
                p.AddArc(b.Right - d, b.Bottom - d, d, d, 0, 90);
                p.AddArc(b.X, b.Bottom - d, d, d, 90, 90);
                p.CloseFigure(); return p;
            }
        }

        // ═════════════════════════════════════════════════════════════════════
        // SETTINGS FORM
        // ═════════════════════════════════════════════════════════════════════

        private const int FW = 520;
        private const int CW = 462;   // content genişliği (sol 20px + sağ scrollbar payı)
        private const int LEFT = 20;

        private readonly AppSettings _s;
        private DarkScroll _container = null!;
        private Panel _scroll = null!;   // _container.Content kısayolu
        private int _y;

        private CheckBox chkWindows = null!;
        private CheckBox chkMinimized = null!;
        private CheckBox chkFullscreen = null!;
        private TextBox txtUrl = null!;
        private TrackBar trkZoom = null!;
        private Label lblZoomVal = null!;
        private CheckBox chkAutoReload = null!;
        private NumericUpDown numDelay = null!;
        private CheckBox chkCloseToTray = null!;
        private CheckBox chkConfirm = null!;
        private CheckBox chkDrop = null!;
        private CheckBox chkClearExit = null!;

        public bool ShouldClearCache { get; private set; }
        public bool ShouldClearCookies { get; private set; }

        private readonly double[] _zooms = { 0.5, 0.67, 0.75, 0.9, 1.0, 1.1, 1.25, 1.5, 1.75, 2.0 };
        private static readonly Font _def = new Font("Segoe UI", 9F);
        private static readonly Font _bold = new Font("Segoe UI", 9F, FontStyle.Bold);

        // ── Kurucu ────────────────────────────────────────────────────────────
        public SettingsForm(AppSettings settings)
        {
            _s = settings;
            BuildUI();
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == 0x0084) // WM_NCHITTEST — başlıktan sürükle
            {
                var p = PointToClient(new Point(m.LParam.ToInt32() & 0xFFFF,
                                                (m.LParam.ToInt32() >> 16) & 0xFFFF));
                if (p.Y >= 0 && p.Y <= 58) m.Result = new IntPtr(2);
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            try { int v = 2; DwmSetWindowAttribute(Handle, 33, ref v, 4); } catch { }
        }

        // ═════════════════════════════════════════════════════════════════════
        // ARAYÜZ KURULUMU
        // ═════════════════════════════════════════════════════════════════════
        private void BuildUI()
        {
            FormBorderStyle = FormBorderStyle.None;
            BackColor = T.FormBg;
            Font = _def;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;

            // ── Başlık ────────────────────────────────────────────────────────
            var pnlTitle = new Panel { Dock = DockStyle.Top, Height = 58, BackColor = T.TitleBg };
            pnlTitle.Paint += (_, e) =>
            {
                using var pen = new Pen(T.Divider, 1);
                e.Graphics.DrawLine(pen, 0, 57, pnlTitle.Width, 57);
            };
            pnlTitle.Controls.Add(Lbl("\uE713", new Font("Segoe MDL2 Assets", 14F), T.Accent,
                                        18, 14, 30, 30, ContentAlignment.MiddleCenter));
            pnlTitle.Controls.Add(Lbl("Ayarlar", new Font("Segoe UI", 13F), T.TextMain,
                                        54, 16, 280, 26));

            var btnClose = MakeBtn("\uE8BB", Color.Transparent, T.TextSub);
            btnClose.Font = new Font("Segoe MDL2 Assets", 9F);
            btnClose.Size = new Size(44, 44);
            btnClose.FlatAppearance.MouseOverBackColor = T.CloseHov;
            btnClose.MouseEnter += (_, _) => btnClose.ForeColor = Color.White;
            btnClose.MouseLeave += (_, _) => btnClose.ForeColor = T.TextSub;
            btnClose.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
            void PosClose() => btnClose.Location = new Point(pnlTitle.Width - 44, 7);
            PosClose(); pnlTitle.Resize += (_, _) => PosClose();
            pnlTitle.Controls.Add(btnClose);

            // ── Custom Dark Scroll Container ──────────────────────────────────
            _container = new DarkScroll { Dock = DockStyle.Fill };
            _scroll = _container.Content;   // içeriği buraya ekle

            _y = 14;
            Sec_Startup();
            Sec_Appearance();
            Sec_Behavior();
            Sec_Privacy();
            Sec_Shortcuts();
            Sec_About();
            _y += 16;

            // İçerik yüksekliğini sabitle — bu scroll miktarını belirler
            _scroll.Height = _y;
            _container.Refresh();

            // ── Alt Bar ───────────────────────────────────────────────────────
            var pnlBottom = new Panel { Dock = DockStyle.Bottom, Height = 66, BackColor = T.TitleBg };
            pnlBottom.Paint += (_, e) =>
            {
                using var pen = new Pen(T.Divider, 1);
                e.Graphics.DrawLine(pen, 0, 0, pnlBottom.Width, 0);
            };

            var btnClear = MakeBtn("Önbelleği Temizle", T.BtnSec, T.TextSub, T.BtnSecBor);
            var btnCancel = MakeBtn("İptal", T.BtnSec, T.TextSub, T.BtnSecBor);
            var btnSave = MakeBtn("  Kaydet  ", T.Accent, Color.White);
            btnSave.FlatAppearance.MouseOverBackColor = T.AccentHov;
            btnClear.Click += BtnClear_Click;
            btnCancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
            btnSave.Click += BtnSave_Click;

            void LayoutBottom()
            {
                const int bh = 36, pad = 12;
                btnSave.Size = new Size(120, bh); btnSave.Location = new Point(pnlBottom.Width - 120 - pad, 15);
                btnCancel.Size = new Size(110, bh); btnCancel.Location = new Point(pnlBottom.Width - 234 - pad, 15);
                btnClear.Size = new Size(140, bh); btnClear.Location = new Point(pad, 15);
            }
            LayoutBottom(); pnlBottom.Resize += (_, _) => LayoutBottom();
            pnlBottom.Controls.AddRange(new Control[] { btnClear, btnCancel, btnSave });

            ClientSize = new Size(FW, 660);
            Controls.AddRange(new Control[] { _container, pnlTitle, pnlBottom });
        }

        // ═════════════════════════════════════════════════════════════════════
        // BÖLÜMLER (aynı — sadece _scroll.Controls.Add kullanıyoruz)
        // ═════════════════════════════════════════════════════════════════════

        private void Sec_Startup()
        {
            Header("\uE7FC", "Başlangıç");
            chkWindows = Chk("Windows ile birlikte başlat", _s.StartWithWindows,
                                "HKCU\\...\\Run anahtarına eklenir; sistem açılışında tray'de başlar");
            chkMinimized = Chk("Simge durumunda başlat (tray'e küçült)", _s.StartMinimized);
            chkFullscreen = Chk("Tam ekran başlat", _s.StartFullscreen);
            Gap(8);
            _scroll.Controls.Add(Lbl("Açılış sayfası URL'i", new Font("Segoe UI", 8.5F), T.TextSub, LEFT, _y, CW, 16));
            _y += 18;
            txtUrl = new TextBox
            {
                Text = _s.StartupUrl,
                Font = new Font("Segoe UI", 10F),
                BackColor = T.Input,
                ForeColor = T.TextMain,
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(LEFT, _y),
                Size = new Size(CW, 28)
            };
            _scroll.Controls.Add(txtUrl); _y += 36;
        }

        private void Sec_Appearance()
        {
            Header("\uE771", "Görünüm");
            int idx = ZoomIdx(_s.DefaultZoom);

            var rowZ = new Panel { BackColor = Color.Transparent, Location = new Point(LEFT, _y), Size = new Size(CW, 22) };
            rowZ.Controls.Add(Lbl("Varsayılan zoom seviyesi", _def, T.TextSub, 0, 2, 280, 18));
            lblZoomVal = Lbl(ZoomStr(idx), new Font("Segoe UI", 10F, FontStyle.Bold), T.Accent,
                             CW - 54, 0, 54, 22, ContentAlignment.MiddleRight);
            rowZ.Controls.Add(lblZoomVal);
            _scroll.Controls.Add(rowZ); _y += 26;

            trkZoom = new TrackBar
            {
                Minimum = 0,
                Maximum = _zooms.Length - 1,
                Value = idx,
                TickStyle = TickStyle.None,
                Location = new Point(LEFT - 4, _y),
                Size = new Size(CW + 4, 32)
            };
            trkZoom.Scroll += (_, _) => lblZoomVal.Text = ZoomStr(trkZoom.Value);
            _scroll.Controls.Add(trkZoom); _y += 34;

            var hints = new Panel { BackColor = Color.Transparent, Location = new Point(LEFT, _y), Size = new Size(CW, 14) };
            hints.Controls.Add(Lbl("%50", _def, T.TextHint, 0, 0, 36, 14));
            hints.Controls.Add(Lbl("%100", _def, T.TextHint, CW / 2 - 18, 0, 36, 14, ContentAlignment.MiddleCenter));
            hints.Controls.Add(Lbl("%200", _def, T.TextHint, CW - 36, 0, 36, 14, ContentAlignment.MiddleRight));
            _scroll.Controls.Add(hints); _y += 20;
            Gap(8);
        }

        private void Sec_Behavior()
        {
            Header("\uE8B7", "Davranış");
            chkAutoReload = Chk("Sunucu hazır olana kadar otomatik yenile", _s.AutoReload,
                                "Sayfa yüklenemezse belirtilen süre sonra tekrar dener");

            var rowD = new Panel { BackColor = Color.Transparent, Location = new Point(LEFT + 20, _y), Size = new Size(CW - 20, 28) };
            rowD.Controls.Add(Lbl("Yenileme aralığı:", _def, T.TextSub, 0, 5, 180, 18));
            numDelay = new NumericUpDown
            {
                Minimum = 1,
                Maximum = 60,
                Value = Math.Clamp(_s.AutoReloadDelaySec, 1, 60),
                Font = new Font("Segoe UI", 9.5F),
                BackColor = T.Input,
                ForeColor = T.TextMain,
                Location = new Point(188, 2),
                Size = new Size(56, 24),
                BorderStyle = BorderStyle.FixedSingle
            };
            rowD.Controls.Add(numDelay);
            rowD.Controls.Add(Lbl("saniye", _def, T.TextHint, 252, 6, 60, 16));
            _scroll.Controls.Add(rowD); _y += 34;

            Divider();
            chkCloseToTray = Chk("X butonu tray'e küçültsün (kapatmasın)", _s.CloseToTray,
                                  "Gerçek kapanma için tray simgesine sağ tıkla → Kapat");
            chkConfirm = Chk("Kapatmadan önce onay iste", _s.ConfirmOnClose);
            chkDrop = Chk("Harici sürükle-bırak iznine izin ver", _s.AllowExternalDrop);
            chkClearExit = Chk("Çıkışta WebView2 disk önbelleğini temizle", _s.ClearCacheOnExit);
            Gap(8);
        }

        private void Sec_Privacy()
        {
            Header("\uE72E", "Gizlilik & Önbellek");

            var warn = new Panel { BackColor = T.WarnBg, Location = new Point(LEFT, _y), Size = new Size(CW, 44) };
            warn.Paint += (_, e) =>
            {
                using var pen = new Pen(T.WarnBor, 1);
                e.Graphics.DrawRectangle(pen, 0, 0, warn.Width - 1, warn.Height - 1);
            };
            warn.Controls.Add(Lbl("⚠  Önbelleği temizlemek mevcut oturumu etkiler.\n" +
                                   "   Sayfalar yeniden yüklenene kadar yavaşlayabilir.",
                                   new Font("Segoe UI", 8.5F), T.WarnText, 10, 6, CW - 20, 32));
            _scroll.Controls.Add(warn); _y += 52;

            var bCache = MakeBtn("Önbelleği Temizle", T.BtnSec, T.TextSub, T.BtnSecBor);
            var bCook = MakeBtn("Çerezleri Temizle", T.BtnSec, T.TextSub, T.BtnSecBor);
            bCache.Location = new Point(LEFT, _y); bCache.Size = new Size(190, 32);
            bCook.Location = new Point(LEFT + 200, _y); bCook.Size = new Size(190, 32);
            bCache.Click += (_, _) => { ShouldClearCache = true; bCache.Text = "✓ Temizlenecek"; bCache.ForeColor = Color.FromArgb(52, 199, 89); };
            bCook.Click += (_, _) => { ShouldClearCookies = true; bCook.Text = "✓ Temizlenecek"; bCook.ForeColor = Color.FromArgb(52, 199, 89); };
            _scroll.Controls.AddRange(new Control[] { bCache, bCook }); _y += 40;
            Gap(8);
        }

        private void Sec_Shortcuts()
        {
            Header("\uE765", "Klavye Kısayolları");

            (string key, string desc)[] rows =
            {
                ("F1",       "Ayarları aç"),
                ("F11",      "Tam ekran aç / kapat"),
                ("F12",      "Geliştirici araçları"),
                ("Ctrl+R",   "Sayfayı yenile"),
                ("Ctrl+L",   "URL çubuğuna odaklan"),
                ("Ctrl++",   "Yakınlaştır"),
                ("Ctrl+−",   "Uzaklaştır"),
                ("Ctrl+0",   "Zoom sıfırla (%100)"),
                ("Alt+←",    "Önceki sayfa"),
                ("Alt+→",    "Sonraki sayfa"),
            };

            foreach (var (key, desc) in rows)
            {
                var row = new Panel { BackColor = Color.Transparent, Location = new Point(LEFT, _y), Size = new Size(CW, 26) };
                var badge = new Label
                {
                    Text = key,
                    Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                    ForeColor = T.TextMain,
                    BackColor = T.KeyBadge,
                    TextAlign = ContentAlignment.MiddleCenter,
                    BorderStyle = BorderStyle.FixedSingle,
                    Location = new Point(0, 3),
                    Size = new Size(88, 20)
                };
                row.Controls.Add(badge);
                row.Controls.Add(Lbl(desc, _def, T.TextSub, 98, 5, CW - 100, 16));
                _scroll.Controls.Add(row); _y += 28;
            }
            Gap(10);
        }

        private void Sec_About()
        {
            Divider(); Gap(6);
            _scroll.Controls.Add(Lbl("NextJS Tarayıcı",
                new Font("Segoe UI", 11F, FontStyle.Bold), T.TextMain, LEFT, _y, CW, 22)); _y += 26;
            _scroll.Controls.Add(Lbl("Sürüm 1.0.0  •  .NET 10  •  WebView2 " + GetWV2(),
                _def, T.TextSub, LEFT, _y, CW, 18)); _y += 22;
            _scroll.Controls.Add(Lbl("Geliştirici: Samed CIMEN (Scotuch)",
                _def, T.TextHint, LEFT, _y, CW, 16)); _y += 20;
            var lnk = Lbl("github.com/scotuch", _def, T.Accent, LEFT, _y, CW, 16);
            lnk.Cursor = Cursors.Hand;
            lnk.Click += (_, _) =>
            {
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(
                    "https://github.com/scotuch")
                    { UseShellExecute = true });
                }
                catch { }
            };
            _scroll.Controls.Add(lnk); _y += 22;
        }

        // ═════════════════════════════════════════════════════════════════════
        // KAYDET & TEMİZLE
        // ═════════════════════════════════════════════════════════════════════

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            var url = txtUrl.Text.Trim();
            if (!url.StartsWith("http")) url = "http://" + url;
            _s.StartWithWindows = chkWindows.Checked;
            _s.StartMinimized = chkMinimized.Checked;
            _s.StartFullscreen = chkFullscreen.Checked;
            _s.StartupUrl = url;
            _s.AutoReload = chkAutoReload.Checked;
            _s.AutoReloadDelaySec = (int)numDelay.Value;
            _s.CloseToTray = chkCloseToTray.Checked;
            _s.ConfirmOnClose = chkConfirm.Checked;
            _s.AllowExternalDrop = chkDrop.Checked;
            _s.ClearCacheOnExit = chkClearExit.Checked;
            _s.DefaultZoom = _zooms[trkZoom.Value];
            _s.Save();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void BtnClear_Click(object? sender, EventArgs e)
        {
            ShouldClearCache = ShouldClearCookies = true;
            if (sender is Button b) { b.Text = "✓ Temizlenecek"; b.ForeColor = Color.FromArgb(52, 199, 89); }
        }

        // ═════════════════════════════════════════════════════════════════════
        // YARDIMCILAR
        // ═════════════════════════════════════════════════════════════════════

        private void Header(string icon, string title)
        {
            _scroll.Controls.Add(new Label
            {
                Text = icon,
                Font = new Font("Segoe MDL2 Assets", 10F),
                ForeColor = T.Accent,
                BackColor = Color.Transparent,
                Location = new Point(LEFT, _y + 1),
                Size = new Size(20, 20),
                TextAlign = ContentAlignment.MiddleCenter
            });
            _scroll.Controls.Add(new Label
            {
                Text = title.ToUpper(),
                Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                ForeColor = T.Accent,
                BackColor = Color.Transparent,
                Location = new Point(LEFT + 26, _y + 3),
                Size = new Size(CW - 26, 16)
            });
            _scroll.Controls.Add(new Panel { BackColor = T.Divider, Location = new Point(LEFT, _y + 22), Size = new Size(CW, 1) });
            _y += 32;
        }

        private CheckBox Chk(string text, bool val, string? hint = null)
        {
            var c = new CheckBox
            {
                Text = text,
                Checked = val,
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = T.TextMain,
                BackColor = Color.Transparent,
                Location = new Point(LEFT, _y),
                Size = new Size(CW, 24),
                Cursor = Cursors.Hand
            };
            _scroll.Controls.Add(c); _y += 26;
            if (hint != null)
            {
                _scroll.Controls.Add(new Label
                {
                    Text = hint,
                    Font = new Font("Segoe UI", 8F),
                    ForeColor = T.TextHint,
                    BackColor = Color.Transparent,
                    Location = new Point(LEFT + 20, _y),
                    Size = new Size(CW - 20, 15)
                });
                _y += 17;
            }
            return c;
        }

        private void Divider()
        {
            _scroll.Controls.Add(new Panel { BackColor = T.Divider, Location = new Point(LEFT, _y), Size = new Size(CW, 1) });
            _y += 12;
        }

        private void Gap(int h) => _y += h;

        // Overload 1 — font, renk, konum, boyut
        private static Label Lbl(string text, Font font, Color color, int x, int y, int w, int h)
            => new Label
            {
                Text = text,
                Font = font,
                ForeColor = color,
                BackColor = Color.Transparent,
                Location = new Point(x, y),
                Size = new Size(w, h),
                AutoSize = false
            };

        // Overload 2 — + hizalama
        private static Label Lbl(string text, Font font, Color color, int x, int y, int w, int h,
                                  ContentAlignment align)
            => new Label
            {
                Text = text,
                Font = font,
                ForeColor = color,
                BackColor = Color.Transparent,
                Location = new Point(x, y),
                Size = new Size(w, h),
                TextAlign = align,
                AutoSize = false
            };

        private static Button MakeBtn(string text, Color bg, Color fg, Color border = default)
        {
            var b = new Button
            {
                Text = text,
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = fg,
                BackColor = bg,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = border == default ? 0 : 1;
            if (border != default) b.FlatAppearance.BorderColor = border;
            return b;
        }

        private static string GetWV2()
        {
            try { return $"({Microsoft.Web.WebView2.Core.CoreWebView2Environment.GetAvailableBrowserVersionString()})"; }
            catch { return ""; }
        }

        private int ZoomIdx(double v) { for (int i = 0; i < _zooms.Length; i++) if (Math.Abs(_zooms[i] - v) < 0.01) return i; return 4; }
        private string ZoomStr(int i) => $"%{(int)(_zooms[i] * 100)}";
    }
}
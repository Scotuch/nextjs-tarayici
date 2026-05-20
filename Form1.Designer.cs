namespace NextJS_Tarayici
{
    partial class Form1
    {
        private System.ComponentModel.IContainer? components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            pnlToolbar = new Panel();
            pnlNavGroup = new Panel();
            pnlUrl = new Panel();
            lblProtocol = new Label();
            txtUrl = new TextBox();
            lblZoom = new Label();
            btnSettings = new Button();
            btnBack = new Button();
            btnForward = new Button();
            btnRefresh = new Button();
            btnDevTools = new Button();
            btnFullscreen = new Button();
            btnTray = new Button();
            progressBar = new ProgressBar();
            webView21 = new Microsoft.Web.WebView2.WinForms.WebView2();

            // pnlActionGroup artık kullanılmıyor ama field null! olarak tutuluyor
            pnlActionGroup = new Panel { Visible = false };

            pnlToolbar.SuspendLayout();
            pnlNavGroup.SuspendLayout();
            pnlUrl.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)webView21).BeginInit();
            SuspendLayout();

            // ── pnlToolbar ────────────────────────────────────────────────────
            pnlToolbar.BackColor = Color.FromArgb(26, 26, 28);
            pnlToolbar.Controls.AddRange(new Control[] { pnlNavGroup, pnlUrl });
            pnlToolbar.Dock = DockStyle.Top;
            pnlToolbar.Height = 54;
            pnlToolbar.Name = "pnlToolbar";
            pnlToolbar.Paint += pnlToolbar_Paint;

            // ══════════════════════════════════════════════════════════════════
            // pnlNavGroup — TEK GRUP: [⚙ | ‹ › ↻ | </> ⛶ ⤓]
            //
            //  ⚙   ‹   ›   ↻   [sekiz px boşluk]  </>  ⛶   ⤓
            //  2  40  78 116        154           158  196  234
            //
            //  Toplam genişlik: 234 + 34 + 2 (sağ kenar) = 270px
            // ══════════════════════════════════════════════════════════════════
            pnlNavGroup.BackColor = Color.Transparent;
            pnlNavGroup.Controls.AddRange(new Control[]
            {
                btnSettings,
                btnBack, btnForward, btnRefresh,
                btnDevTools, btnFullscreen, btnTray
            });
            pnlNavGroup.Location = new Point(10, 11);
            pnlNavGroup.Name = "pnlNavGroup";
            pnlNavGroup.Size = new Size(270, 32);
            pnlNavGroup.Paint += pnlNavGroup_Paint;

            // ── ⚙ btnSettings (F1) ────────────────────────────────────────────
            btnSettings.BackColor = Color.Transparent;
            btnSettings.FlatAppearance.BorderSize = 0;
            btnSettings.FlatStyle = FlatStyle.Flat;
            btnSettings.Font = new Font("Segoe MDL2 Assets", 9F);
            btnSettings.ForeColor = Color.FromArgb(140, 140, 150);
            btnSettings.Location = new Point(2, 2);
            btnSettings.Name = "btnSettings";
            btnSettings.Size = new Size(34, 28);
            btnSettings.Text = "\uE713";
            btnSettings.Click += btnSettings_Click;

            // ── ‹ btnBack ─────────────────────────────────────────────────────
            btnBack.BackColor = Color.Transparent;
            btnBack.FlatAppearance.BorderSize = 0;
            btnBack.FlatStyle = FlatStyle.Flat;
            btnBack.Font = new Font("Segoe MDL2 Assets", 10F);
            btnBack.ForeColor = Color.FromArgb(170, 170, 175);
            btnBack.Location = new Point(40, 2);
            btnBack.Name = "btnBack";
            btnBack.Size = new Size(34, 28);
            btnBack.Text = "\uE72B";
            btnBack.Click += btnBack_Click;

            // ── › btnForward ──────────────────────────────────────────────────
            btnForward.BackColor = Color.Transparent;
            btnForward.FlatAppearance.BorderSize = 0;
            btnForward.FlatStyle = FlatStyle.Flat;
            btnForward.Font = new Font("Segoe MDL2 Assets", 10F);
            btnForward.ForeColor = Color.FromArgb(170, 170, 175);
            btnForward.Location = new Point(78, 2);
            btnForward.Name = "btnForward";
            btnForward.Size = new Size(34, 28);
            btnForward.Text = "\uE72A";
            btnForward.Click += btnForward_Click;

            // ── ↻ btnRefresh ──────────────────────────────────────────────────
            btnRefresh.BackColor = Color.Transparent;
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.FlatStyle = FlatStyle.Flat;
            btnRefresh.Font = new Font("Segoe MDL2 Assets", 10F);
            btnRefresh.ForeColor = Color.FromArgb(170, 170, 175);
            btnRefresh.Location = new Point(116, 2);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(34, 28);
            btnRefresh.Text = "\uE72C";
            btnRefresh.Click += btnRefresh_Click;

            // ── 8px görsel ayraç boşluğu sonrası aksiyon butonları ────────────

            // ── </> btnDevTools (F12) ─────────────────────────────────────────
            btnDevTools.BackColor = Color.Transparent;
            btnDevTools.FlatAppearance.BorderSize = 0;
            btnDevTools.FlatStyle = FlatStyle.Flat;
            btnDevTools.Font = new Font("Segoe MDL2 Assets", 9F);
            btnDevTools.ForeColor = Color.FromArgb(140, 140, 150);
            btnDevTools.Location = new Point(158, 2);   // 116+34+8
            btnDevTools.Name = "btnDevTools";
            btnDevTools.Size = new Size(34, 28);
            btnDevTools.Text = "\uEBE8";
            btnDevTools.Click += btnDevTools_Click;

            // ── ⛶ btnFullscreen (F11) ─────────────────────────────────────────
            btnFullscreen.BackColor = Color.Transparent;
            btnFullscreen.FlatAppearance.BorderSize = 0;
            btnFullscreen.FlatStyle = FlatStyle.Flat;
            btnFullscreen.Font = new Font("Segoe MDL2 Assets", 9F);
            btnFullscreen.ForeColor = Color.FromArgb(140, 140, 150);
            btnFullscreen.Location = new Point(196, 2);
            btnFullscreen.Name = "btnFullscreen";
            btnFullscreen.Size = new Size(34, 28);
            btnFullscreen.Text = "\uE740";
            btnFullscreen.Click += btnFullscreen_Click;

            // ── ⤓ btnTray ─────────────────────────────────────────────────────
            btnTray.BackColor = Color.Transparent;
            btnTray.FlatAppearance.BorderSize = 0;
            btnTray.FlatStyle = FlatStyle.Flat;
            btnTray.Font = new Font("Segoe MDL2 Assets", 9F);
            btnTray.ForeColor = Color.FromArgb(140, 140, 150);
            btnTray.Location = new Point(234, 2);
            btnTray.Name = "btnTray";
            btnTray.Size = new Size(34, 28);
            btnTray.Text = "\uE921";
            btnTray.Click += btnTray_Click;

            // ══════════════════════════════════════════════════════════════════
            // pnlUrl — URL bar (grup biter, kalan alanı kaplar)
            //   x = 10 + 270 + 10 = 290
            //   w = 1280 - 290 - 10 = 980  (Anchor ile form boyutuyla değişir)
            // ══════════════════════════════════════════════════════════════════
            pnlUrl.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pnlUrl.BackColor = Color.Transparent;
            pnlUrl.Controls.AddRange(new Control[] { lblProtocol, txtUrl, lblZoom });
            pnlUrl.Location = new Point(290, 11);
            pnlUrl.Name = "pnlUrl";
            pnlUrl.Size = new Size(980, 32);
            pnlUrl.Paint += pnlUrl_Paint;

            lblProtocol.Font = new Font("Segoe UI Emoji", 10F);
            lblProtocol.ForeColor = Color.FromArgb(130, 130, 135);
            lblProtocol.Location = new Point(8, 7);
            lblProtocol.Name = "lblProtocol";
            lblProtocol.Size = new Size(22, 18);
            lblProtocol.Text = "🌐";
            lblProtocol.TextAlign = ContentAlignment.MiddleCenter;
            lblProtocol.Cursor = Cursors.IBeam;

            txtUrl.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtUrl.BackColor = Color.FromArgb(38, 38, 40);
            txtUrl.BorderStyle = BorderStyle.None;
            txtUrl.Font = new Font("Segoe UI", 10F);
            txtUrl.ForeColor = Color.FromArgb(210, 210, 215);
            txtUrl.Location = new Point(34, 8);
            txtUrl.Name = "txtUrl";
            txtUrl.Size = new Size(900, 18);
            txtUrl.KeyDown += txtUrl_KeyDown;

            lblZoom.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblZoom.Font = new Font("Segoe UI", 8F);
            lblZoom.ForeColor = Color.FromArgb(90, 90, 98);
            lblZoom.Location = new Point(936, 9);
            lblZoom.Name = "lblZoom";
            lblZoom.Size = new Size(38, 14);
            lblZoom.Text = "%100";
            lblZoom.TextAlign = ContentAlignment.MiddleRight;

            // ── progressBar ───────────────────────────────────────────────────
            progressBar.Dock = DockStyle.Top;
            progressBar.Height = 2;
            progressBar.Maximum = 100;
            progressBar.Style = ProgressBarStyle.Continuous;
            progressBar.ForeColor = Color.FromArgb(0, 120, 215);
            progressBar.BackColor = Color.FromArgb(26, 26, 28);
            progressBar.Visible = false;

            // ── webView21 ─────────────────────────────────────────────────────
            webView21.AllowExternalDrop = true;
            webView21.CreationProperties = null;
            webView21.DefaultBackgroundColor = Color.FromArgb(18, 18, 18);
            webView21.Dock = DockStyle.Fill;
            webView21.ZoomFactor = 1D;

            // ── Form1 ─────────────────────────────────────────────────────────
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(18, 18, 18);
            ClientSize = new Size(1280, 800);
            Controls.Add(webView21);
            Controls.Add(progressBar);
            Controls.Add(pnlToolbar);
            Font = new Font("Segoe UI", 9F);
            MinimumSize = new Size(700, 450);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "NextJS Tarayıcı";

            pnlUrl.ResumeLayout(false);
            pnlUrl.PerformLayout();
            pnlNavGroup.ResumeLayout(false);
            pnlToolbar.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)webView21).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlToolbar = null!;
        private Panel pnlNavGroup = null!;
        private Panel pnlActionGroup = null!;   // artık görünmez, field tutuldu
        private Panel pnlUrl = null!;
        private Label lblProtocol = null!;
        private TextBox txtUrl = null!;
        private Label lblZoom = null!;
        private Button btnSettings = null!;
        private Button btnBack = null!;
        private Button btnForward = null!;
        private Button btnRefresh = null!;
        private Button btnDevTools = null!;
        private Button btnFullscreen = null!;
        private Button btnTray = null!;
        private ProgressBar progressBar = null!;
        private Microsoft.Web.WebView2.WinForms.WebView2 webView21 = null!;
    }
}
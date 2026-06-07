using System;
using System.Drawing;
using System.Windows.Forms;
using AxWMPLib;

namespace MediaPlayerApp
{
    public partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private AxWindowsMediaPlayer axWindowsMediaPlayer;
        private Timer progressTimer;

        private GlassBar titleBar;
        private Label lblAppTitle;
        private VlcIconButton btnMinimize;
        private VlcIconButton btnMaximize;
        private VlcIconButton btnClose;

        private Panel videoPanel;
        private GlassBackgroundPanel ambientBackdrop;
        private Panel albumArtPanel;

        private GlassBar infoPanel;
        private Label lblFileName;
        private Label lblMediaMeta;

        private Panel controlBar;

        private Label lblCurrentTime;
        private Label lblTotalTime;

        private VlcSlider seekSlider;
        private VlcSlider volumeSlider;

        private VlcIconButton btnOpen;
        private VlcIconButton btnPrev;
        private VlcPlayButton btnPlay;
        private VlcIconButton btnNext;
        private VlcIconButton btnStop;
        private VlcIconButton btnShuffle;
        private VlcIconButton btnRepeat;
        private VlcIconButton btnMute;
        private VlcIconButton btnFullscreen;

        private Label lblVolumeIcon;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();

            this.Text = "CRI-Media Player";
            this.Size = new Size(980, 680);
            this.MinimumSize = new Size(820, 560);
            this.BackColor = VlcTheme.BgDeep;
            this.ForeColor = VlcTheme.TextPrimary;
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.DoubleBuffered = true;

            axWindowsMediaPlayer = new AxWindowsMediaPlayer();
            ((System.ComponentModel.ISupportInitialize)axWindowsMediaPlayer).BeginInit();
            axWindowsMediaPlayer.Dock = DockStyle.Fill;
            axWindowsMediaPlayer.Enabled = true;
            ((System.ComponentModel.ISupportInitialize)axWindowsMediaPlayer).EndInit();

            titleBar = new GlassBar
            {
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = VlcTheme.BgTitle,
                GlowEdge = GlassBar.Edge.Bottom,
                Padding = new Padding(0)
            };

            lblAppTitle = new Label
            {
                Text = "CRI-Media Player",
                Font = new Font("Segoe UI Semibold", 10f, FontStyle.Regular),
                ForeColor = VlcTheme.TextPrimary,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(14, 12)
            };

            btnClose = new VlcIconButton("✕", VlcTheme.BgTitle, Color.FromArgb(196, 43, 28))
            {
                Size = new Size(48, 40),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 10f)
            };

            btnMaximize = new VlcIconButton("🗖", VlcTheme.BgTitle, VlcTheme.BtnHover)
            {
                Size = new Size(46, 40),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI Symbol", 9f)
            };

            btnMinimize = new VlcIconButton("─", VlcTheme.BgTitle, VlcTheme.BtnHover)
            {
                Size = new Size(46, 40),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 12f)
            };

            titleBar.Controls.Add(lblAppTitle);
            titleBar.Controls.Add(btnClose);
            titleBar.Controls.Add(btnMaximize);
            titleBar.Controls.Add(btnMinimize);

            videoPanel = new GlassBackgroundPanel
            {
                Dock = DockStyle.Fill,
                BackColor = VlcTheme.BgDeep
            };

            ambientBackdrop = new GlassBackgroundPanel
            {
                Dock = DockStyle.Fill,
                BackColor = VlcTheme.BgDeep
            };

            albumArtPanel = new AlbumArtPanel
            {
                BackColor = VlcTheme.BgCard,
                Anchor = AnchorStyles.None
            };

            videoPanel.Controls.Add(axWindowsMediaPlayer);
            videoPanel.Controls.Add(ambientBackdrop);
            videoPanel.Controls.Add(albumArtPanel);
            ambientBackdrop.SendToBack();
            axWindowsMediaPlayer.SendToBack();
            albumArtPanel.BringToFront();

            infoPanel = new GlassBar
            {
                Dock = DockStyle.Bottom,
                Height = 52,
                BackColor = VlcTheme.BgCard,
                GlowEdge = GlassBar.Edge.None,
                Padding = new Padding(16, 0, 16, 0)
            };

            lblFileName = new Label
            {
                Text = "No media loaded",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = VlcTheme.TextPrimary,
                BackColor = VlcTheme.BgCard,
                AutoSize = false,
                Height = 26,
                Location = new Point(16, 5),
                Width = 700,
                TextAlign = ContentAlignment.MiddleLeft
            };

            lblMediaMeta = new Label
            {
                Text = "Open a file to start playing",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = VlcTheme.TextMuted,
                BackColor = VlcTheme.BgCard,
                AutoSize = false,
                Height = 20,
                Location = new Point(16, 28),
                Width = 700,
                TextAlign = ContentAlignment.MiddleLeft
            };

            infoPanel.Controls.Add(lblFileName);
            infoPanel.Controls.Add(lblMediaMeta);

            controlBar = new ControlBarPanel
            {
                Dock = DockStyle.Bottom,
                Height = 130,
                BackColor = VlcTheme.BgControl,
                Padding = new Padding(0)
            };

            seekSlider = new VlcSlider
            {
                TrackFillColor = VlcTheme.Orange,
                TrackColor = VlcTheme.SliderTrack,
                ThumbColor = Color.White,
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top,
                Location = new Point(60, 18),
                Height = 20
            };

            lblCurrentTime = new Label
            {
                Text = "0:00",
                ForeColor = VlcTheme.TextMuted,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(8, 20)
            };

            lblTotalTime = new Label
            {
                Text = "0:00",
                ForeColor = VlcTheme.TextMuted,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                BackColor = Color.Transparent,
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            btnShuffle = MakeIconBtn("⇄", 13f);
            btnPrev    = MakeIconBtn("⏮", 14f);
            btnPlay    = new VlcPlayButton { Cursor = Cursors.Hand };
            btnNext    = MakeIconBtn("⏭", 14f);
            btnStop    = MakeIconBtn("⏹", 13f);
            btnRepeat  = MakeIconBtn("↻", 15f);

            lblVolumeIcon = new Label
            {
                Text = "🔊",
                Font = new Font("Segoe UI Emoji", 11f),
                ForeColor = VlcTheme.TextPrimary,
                BackColor = Color.Transparent,
                AutoSize = true,
                Cursor = Cursors.Hand
            };

            volumeSlider = new VlcSlider
            {
                TrackFillColor = VlcTheme.Orange,
                TrackColor = VlcTheme.SliderTrack,
                ThumbColor = Color.White,
                Value = 0.75f,
                Width = 100,
                Height = 20
            };

            btnMute       = MakeIconBtn("🔇", 12f);
            btnOpen       = MakeIconBtn("📂", 13f);
            btnFullscreen = MakeIconBtn("⛶", 14f);

            controlBar.Controls.Add(seekSlider);
            controlBar.Controls.Add(lblCurrentTime);
            controlBar.Controls.Add(lblTotalTime);
            controlBar.Controls.Add(btnShuffle);
            controlBar.Controls.Add(btnPrev);
            controlBar.Controls.Add(btnPlay);
            controlBar.Controls.Add(btnNext);
            controlBar.Controls.Add(btnStop);
            controlBar.Controls.Add(btnRepeat);
            controlBar.Controls.Add(lblVolumeIcon);
            controlBar.Controls.Add(volumeSlider);
            controlBar.Controls.Add(btnMute);
            controlBar.Controls.Add(btnOpen);
            controlBar.Controls.Add(btnFullscreen);

            progressTimer = new Timer(this.components) { Interval = 80 };

            this.Controls.Add(videoPanel);
            this.Controls.Add(infoPanel);
            this.Controls.Add(controlBar);
            this.Controls.Add(titleBar);
        }

        private VlcIconButton MakeIconBtn(string icon, float fontSize)
        {
            return new VlcIconButton(icon, Color.Transparent, VlcTheme.BtnHover)
            {
                Font = new Font("Segoe UI Emoji", fontSize),
                ForeColor = VlcTheme.TextPrimary,
                Size = new Size(40, 40),
                Cursor = Cursors.Hand,
                FlatStyle = FlatStyle.Flat
            };
        }
    }
}

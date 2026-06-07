using System;
using System.Drawing;
using System.Windows.Forms;

namespace MediaPlayerApp
{
    public partial class MainForm : Form
    {
        private bool _seekDragging = false;
        private bool _isMuted = false;
        private int  _savedVolume = 75;
        private bool _shuffleOn = false;
        private bool _repeatOn  = false;
        private bool _isMaximized = false;
        private bool _isFullscreen = false;
        private Point _dragStart;
        private bool  _titleDragging = false;
        private ContextMenuStrip _fsMenu;
        private FullscreenControls _fsControls;

        public MainForm()
        {
            InitializeComponent();
            SetupEventHandlers();
            this.Load += MainForm_Load;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            try { axWindowsMediaPlayer.uiMode = "none"; } catch { }
            try { axWindowsMediaPlayer.stretchToFit = true; } catch { }
            axWindowsMediaPlayer.settings.volume = (int)(volumeSlider.Value * 100);

            _fsMenu = new ContextMenuStrip();
            _fsMenu.Items.Add("Play / Pause").Click += BtnPlay_Click;
            _fsMenu.Items.Add(new ToolStripSeparator());
            _fsMenu.Items.Add("Mute / Unmute").Click += BtnMute_Click;
            _fsMenu.Items.Add("Exit Fullscreen").Click += (s, ev) => { if (_isFullscreen) ToggleFullscreen(); };

            _fsControls = new FullscreenControls { Owner = this };
            _fsControls.PlayPauseRequested += () => BtnPlay_Click(this, EventArgs.Empty);
            _fsControls.PrevRequested      += () => BtnPrev_Click(this, EventArgs.Empty);
            _fsControls.NextRequested      += () => BtnNext_Click(this, EventArgs.Empty);
            _fsControls.MuteRequested      += () => { BtnMute_Click(this, EventArgs.Empty); SyncFsControls(); };
            _fsControls.ExitRequested      += () => { if (_isFullscreen) ToggleFullscreen(); };
            _fsControls.SeekRequested      += FsSeekTo;
            _fsControls.VolumeChanged      += FsVolumeTo;

            LayoutControls();
        }

        private void SetupEventHandlers()
        {
            titleBar.MouseDown += TitleBar_MouseDown;
            titleBar.MouseMove += TitleBar_MouseMove;
            titleBar.MouseUp   += TitleBar_MouseUp;
            titleBar.DoubleClick += (s, e) => ToggleMaximize();
            lblAppTitle.MouseDown += TitleBar_MouseDown;
            lblAppTitle.MouseMove += TitleBar_MouseMove;
            lblAppTitle.MouseUp   += TitleBar_MouseUp;
            lblAppTitle.DoubleClick += (s, e) => ToggleMaximize();

            btnClose.Click    += (s, e) => Application.Exit();
            btnMaximize.Click += (s, e) => ToggleMaximize();
            btnMinimize.Click += (s, e) => this.WindowState = FormWindowState.Minimized;

            btnOpen.Click  += BtnOpen_Click;
            btnPlay.Clicked += BtnPlay_Click;
            btnStop.Click  += BtnStop_Click;
            btnPrev.Click  += BtnPrev_Click;
            btnNext.Click  += BtnNext_Click;

            btnShuffle.Click += (s, e) => { _shuffleOn = !_shuffleOn; btnShuffle.Toggled = _shuffleOn; };
            btnRepeat.Click  += (s, e) => { _repeatOn  = !_repeatOn;  btnRepeat.Toggled  = _repeatOn; };

            btnMute.Click += BtnMute_Click;
            lblVolumeIcon.Click += BtnMute_Click;
            lblVolumeIcon.Cursor = Cursors.Hand;

            btnFullscreen.Click += (s, e) => ToggleFullscreen();
            this.KeyPreview = true;
            this.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.F11) ToggleFullscreen();
                if (e.KeyCode == Keys.Escape && _isFullscreen) ToggleFullscreen();
            };
            videoPanel.DoubleClick   += (s, e) => ToggleFullscreen();
            albumArtPanel.DoubleClick += (s, e) => ToggleFullscreen();

            videoPanel.MouseMove    += (s, e) => RevealFsControls();
            albumArtPanel.MouseMove += (s, e) => RevealFsControls();
            axWindowsMediaPlayer.MouseMoveEvent += (s, e) => RevealFsControls();

            seekSlider.MouseDown += (s, e) => _seekDragging = true;
            seekSlider.MouseUp += (s, e) =>
            {
                _seekDragging = false;
                try
                {
                    double duration = axWindowsMediaPlayer.currentMedia?.duration ?? 0;
                    if (duration > 0)
                    {
                        axWindowsMediaPlayer.Ctlcontrols.currentPosition = seekSlider.Value * duration;
                    }
                }
                catch { }
            };
            seekSlider.ValueChanged += SeekSlider_ValueChanged;

            volumeSlider.ValueChanged += (s, e) =>
            {
                _savedVolume = (int)(volumeSlider.Value * 100);
                axWindowsMediaPlayer.settings.volume = _savedVolume;
                UpdateVolumeIcon();
            };

            axWindowsMediaPlayer.PlayStateChange += WMP_PlayStateChange;
            axWindowsMediaPlayer.OpenStateChange += WMP_OpenStateChange;
            axWindowsMediaPlayer.MouseDownEvent += AxWindowsMediaPlayer_MouseDownEvent;

            progressTimer.Tick += ProgressTimer_Tick;

            this.Resize += (s, e) => LayoutControls();
            controlBar.Resize += (s, e) => LayoutControls();
        }

        private string SafeGetUrl()
        {
            try { return axWindowsMediaPlayer.URL ?? string.Empty; }
            catch { return string.Empty; }
        }

        private string FormatTime(double totalSeconds)
        {
            var ts = TimeSpan.FromSeconds(totalSeconds);
            return ts.Hours > 0
                ? $"{ts.Hours}:{ts.Minutes:D2}:{ts.Seconds:D2}"
                : $"{ts.Minutes}:{ts.Seconds:D2}";
        }
    }
}

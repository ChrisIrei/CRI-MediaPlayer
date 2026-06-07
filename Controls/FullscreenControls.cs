using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace MediaPlayerApp
{
    public class FullscreenControls : Form, IGlassSurface
    {
        public event Action PlayPauseRequested;
        public event Action PrevRequested;
        public event Action NextRequested;
        public event Action MuteRequested;
        public event Action ExitRequested;
        public event Action<double> SeekRequested;
        public event Action<float>  VolumeChanged;

        private readonly VlcSlider     _seek;
        private readonly VlcSlider     _volume;
        private readonly VlcPlayButton _play;
        private readonly VlcIconButton _prev, _next, _mute, _exit;
        private readonly Label _cur, _tot, _volIcon;
        private readonly Timer _hideTimer;
        private readonly Timer _fadeTimer;
        private double _targetOpacity;

        private const int BarHeight  = 90;
        private const int SeekCenter = 28;

        public bool SeekDragging { get; private set; }

        public FullscreenControls()
        {
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar   = false;
            StartPosition   = FormStartPosition.Manual;
            TopMost         = true;
            BackColor       = VlcTheme.BgControl;
            DoubleBuffered  = true;
            Opacity         = 0d;
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);

            _seek = new VlcSlider { Height = 26 };
            _cur  = MakeTime();
            _tot  = MakeTime();

            _prev = MakeIcon("⏮", 14f);
            _play = new VlcPlayButton { Cursor = Cursors.Hand };
            _next = MakeIcon("⏭", 14f);
            _mute = MakeIcon("🔇", 12f);
            _exit = new VlcIconButton("\uE73F", Color.Transparent, VlcTheme.BtnHover)
            {
                Font = new Font("Segoe MDL2 Assets", 11f),
                ForeColor = VlcTheme.TextPrimary,
                Size = new Size(40, 40),
                Cursor = Cursors.Hand,
                FlatStyle = FlatStyle.Flat
            };
            _volIcon = new Label
            {
                Text = "🔊", Font = new Font("Segoe UI Emoji", 11f),
                ForeColor = VlcTheme.TextPrimary, BackColor = Color.Transparent,
                AutoSize = true, Cursor = Cursors.Hand
            };
            _volume = new VlcSlider { Value = 0.75f, Width = 110, Height = 26 };

            Controls.AddRange(new Control[]
            { _seek, _cur, _tot, _prev, _play, _next, _mute, _exit, _volIcon, _volume });

            Size = new Size(900, BarHeight);

            _seek.MouseDown += (s, e) => SeekDragging = true;
            _seek.MouseUp   += (s, e) => { SeekDragging = false; SeekRequested?.Invoke(_seek.Value); KeepAlive(); };
            _volume.ValueChanged += (s, e) => { VolumeChanged?.Invoke(_volume.Value); KeepAlive(); };
            _play.Clicked += (s, e) => { PlayPauseRequested?.Invoke(); KeepAlive(); };
            _prev.Click   += (s, e) => { PrevRequested?.Invoke(); KeepAlive(); };
            _next.Click   += (s, e) => { NextRequested?.Invoke(); KeepAlive(); };
            _mute.Click   += (s, e) => { MuteRequested?.Invoke(); KeepAlive(); };
            _volIcon.Click+= (s, e) => { MuteRequested?.Invoke(); KeepAlive(); };
            _exit.Click   += (s, e) => ExitRequested?.Invoke();

            HookKeepAlive(this);

            _hideTimer = new Timer { Interval = 3200 };
            _hideTimer.Tick += (s, e) => { _hideTimer.Stop(); HideControls(); };

            _fadeTimer = new Timer { Interval = 16 };
            _fadeTimer.Tick += (s, e) =>
            {
                double step = 0.14;
                if (Opacity < _targetOpacity) Opacity = Math.Min(_targetOpacity, Opacity + step);
                else if (Opacity > _targetOpacity) Opacity = Math.Max(_targetOpacity, Opacity - step);
                else { _fadeTimer.Stop(); if (_targetOpacity == 0d) Visible = false; }
            };
        }

        public void ShowControls(Rectangle screenBounds)
        {
            int w = Math.Min(940, screenBounds.Width - 60);
            Size = new Size(w, BarHeight);
            Location = new Point(
                screenBounds.X + (screenBounds.Width - w) / 2,
                screenBounds.Bottom - Height - 40);
            LayoutFs();
            if (!Visible) { Opacity = 0d; Visible = true; }
            BringToFront();
            _targetOpacity = 1d;
            _fadeTimer.Start();
            KeepAlive();
        }

        public void HideControls()
        {
            _hideTimer.Stop();
            _targetOpacity = 0d;
            _fadeTimer.Start();
        }

        public void SetPlaying(bool playing) { _play.IsPlaying = playing; _play.Invalidate(); }

        public void UpdateProgress(double position, double duration)
        {
            if (duration <= 0) return;
            if (!SeekDragging) { _seek.Value = (float)(position / duration); _seek.Invalidate(); }
            _cur.Text = FormatTime(position);
            _tot.Text = FormatTime(duration);
            PositionTimes();
        }

        public void SetVolume(float v) { _volume.Value = v; _volume.Invalidate(); }
        public void SetVolumeIcon(string glyph, bool muted) { _volIcon.Text = glyph; _mute.Toggled = muted; }

        private void KeepAlive()
        {
            if (!Visible) return;
            _hideTimer.Stop();
            _hideTimer.Start();
        }

        private void HookKeepAlive(Control c)
        {
            c.MouseMove += (s, e) => KeepAlive();
            foreach (Control child in c.Controls) HookKeepAlive(child);
        }

        protected override void OnResize(EventArgs e) { base.OnResize(e); LayoutFs(); }

        private void LayoutFs()
        {
            if (_seek == null) return;
            int w = ClientSize.Width, h = ClientSize.Height;
            int pad = 28, timeW = 42;

            using (var path = GlassFx.RoundRect(new Rectangle(0, 0, w, h), 22))
                Region = new Region(path);

            _seek.Left  = pad + timeW;
            _seek.Width = w - (pad + timeW) * 2;
            _seek.Top   = SeekCenter - _seek.Height / 2;
            PositionTimes();

            int cy = h - 28;
            int cx = w / 2;
            _play.Size = new Size(54, 54);
            _prev.Size = new Size(40, 40);
            _next.Size = new Size(40, 40);
            _mute.Size = new Size(38, 38);
            _exit.Size = new Size(40, 40);

            int gap = 18;
            _play.Location = new Point(cx - _play.Width / 2, cy - _play.Height / 2);
            _prev.Location = new Point(_play.Left - gap - _prev.Width, cy - _prev.Height / 2);
            _next.Location = new Point(_play.Right + gap, cy - _next.Height / 2);

            int rightX = w - pad;
            rightX -= _volume.Width;
            _volume.Location = new Point(rightX, cy - _volume.Height / 2);
            rightX -= _volIcon.Width + 6;
            _volIcon.Location = new Point(rightX, cy - _volIcon.Height / 2);
            rightX -= _mute.Width + 12;
            _mute.Location = new Point(rightX, cy - _mute.Height / 2);

            _exit.Location = new Point(pad, cy - _exit.Height / 2);
        }

        private void PositionTimes()
        {
            if (_cur == null) return;
            _cur.Location = new Point(22, SeekCenter - _cur.Height / 2 + 1);
            _tot.Location = new Point(ClientSize.Width - 22 - _tot.Width, SeekCenter - _tot.Height / 2 + 1);
        }

        protected override void OnPaintBackground(PaintEventArgs e) => PaintGlassBackground(e.Graphics);

        public void PaintGlassBackground(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var r = new Rectangle(0, 0, ClientSize.Width - 1, ClientSize.Height - 1);
            using (var path = GlassFx.RoundRect(r, 22))
            {
                using (var baseBr = new SolidBrush(VlcTheme.BgControl))
                    g.FillPath(baseBr, path);
                using (var sheen = new LinearGradientBrush(r,
                    Color.FromArgb(40, 255, 255, 255), Color.FromArgb(6, 255, 255, 255), 90f))
                    g.FillPath(sheen, path);
                using (var pen = new Pen(Color.FromArgb(120, VlcTheme.Neon), 1.4f))
                    g.DrawPath(pen, path);
            }
        }

        private static VlcIconButton MakeIcon(string icon, float size) =>
            new VlcIconButton(icon, Color.Transparent, VlcTheme.BtnHover)
            {
                Font = new Font("Segoe UI Emoji", size),
                ForeColor = VlcTheme.TextPrimary,
                Size = new Size(40, 40),
                Cursor = Cursors.Hand,
                FlatStyle = FlatStyle.Flat
            };

        private static Label MakeTime() => new Label
        {
            Text = "0:00", ForeColor = VlcTheme.TextMuted,
            Font = new Font("Segoe UI", 8.5f), BackColor = Color.Transparent, AutoSize = true
        };

        private static string FormatTime(double totalSeconds)
        {
            var ts = TimeSpan.FromSeconds(totalSeconds);
            return ts.Hours > 0 ? $"{ts.Hours}:{ts.Minutes:D2}:{ts.Seconds:D2}"
                                : $"{ts.Minutes}:{ts.Seconds:D2}";
        }
    }
}

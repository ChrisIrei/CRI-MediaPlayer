using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace MediaPlayerApp
{
    public class VlcPlayButton : Control
    {
        public bool IsPlaying { get; set; } = false;
        private bool _hovered;
        private Timer _animTimer;
        private int _glowAlpha = 70;
        private int _targetAlpha = 70;

        public event EventHandler Clicked;

        public VlcPlayButton()
        {
            Size = new Size(56, 56);
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;

            _animTimer = new Timer { Interval = 16 };
            _animTimer.Tick += (s, e) =>
            {
                if (_glowAlpha < _targetAlpha) _glowAlpha = Math.Min(_glowAlpha + 8, _targetAlpha);
                else if (_glowAlpha > _targetAlpha) _glowAlpha = Math.Max(_glowAlpha - 8, _targetAlpha);
                else _animTimer.Stop();
                Invalidate();
            };
        }

        protected override void OnMouseEnter(EventArgs e) { _hovered = true;  _targetAlpha = 150; _animTimer.Start(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hovered = false; _targetAlpha = 70;  _animTimer.Start(); base.OnMouseLeave(e); }
        protected override void OnClick(EventArgs e) { Clicked?.Invoke(this, e); base.OnClick(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;

            GlassFx.PaintParentSurface(g, this);

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            var orb = new Rectangle(6, 6, Width - 13, Height - 13);

            Color top = _hovered ? VlcTheme.NeonHot : VlcTheme.Neon;
            using (var lgb = new LinearGradientBrush(orb, top, VlcTheme.NeonPurple, 60f))
                g.FillEllipse(lgb, orb);

            using (var path = new GraphicsPath())
            {
                path.AddEllipse(orb.X + 5, orb.Y + 3, orb.Width - 10, orb.Height * 0.6f);
                using (var pgb = new PathGradientBrush(path))
                {
                    pgb.CenterColor = Color.FromArgb(150, 255, 255, 255);
                    pgb.SurroundColors = new[] { Color.FromArgb(0, 255, 255, 255) };
                    g.FillPath(pgb, path);
                }
            }

            using (var pen = new Pen(Color.FromArgb(_hovered ? 230 : 170, VlcTheme.NeonHot), 1.5f))
                g.DrawEllipse(pen, orb);

            float cx = orb.X + orb.Width / 2f;
            float cy = orb.Y + orb.Height / 2f;
            if (IsPlaying)
            {
                float barW = orb.Width * 0.13f;
                float barH = orb.Height * 0.40f;
                float gap  = orb.Width * 0.12f;
                using (var br = new SolidBrush(Color.White))
                {
                    g.FillRectangle(br, cx - gap / 2f - barW, cy - barH / 2f, barW, barH);
                    g.FillRectangle(br, cx + gap / 2f,        cy - barH / 2f, barW, barH);
                }
            }
            else
            {
                GlassFx.FillPlayTriangle(g, cx, cy, orb.Width * 0.34f, Color.White);
            }
        }
    }
}

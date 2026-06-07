using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace MediaPlayerApp
{
    public class VlcIconButton : Button
    {
        private readonly Color _hoverBg;
        private bool _hovered;
        private bool _toggled;

        private Timer _animTimer;
        private int _hoverAlpha = 0;
        private int _targetAlpha = 0;

        public bool Toggled
        {
            get => _toggled;
            set { _toggled = value; Invalidate(); }
        }

        public VlcIconButton(string text, Color normalBg, Color hoverBg)
        {
            Text = text;
            _hoverBg = hoverBg;
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            FlatAppearance.MouseOverBackColor = Color.Transparent;
            FlatAppearance.MouseDownBackColor = Color.Transparent;
            ForeColor = VlcTheme.TextPrimary;
            BackColor = (normalBg == Color.Transparent) ? VlcTheme.BgControl : normalBg;
            UseVisualStyleBackColor = false;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);

            _animTimer = new Timer { Interval = 16 };
            _animTimer.Tick += (s, e) =>
            {
                if (_hoverAlpha < _targetAlpha) _hoverAlpha = Math.Min(_hoverAlpha + 24, _targetAlpha);
                else if (_hoverAlpha > _targetAlpha) _hoverAlpha = Math.Max(_hoverAlpha - 24, _targetAlpha);
                else _animTimer.Stop();
                Invalidate();
            };
        }

        protected override void OnMouseEnter(EventArgs e) { _hovered = true; _targetAlpha = 255; _animTimer.Start(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hovered = false; _targetAlpha = 0; _animTimer.Start(); base.OnMouseLeave(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            GlassFx.PaintParentSurface(g, this);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var pad = Rectangle.Inflate(ClientRectangle, -3, -3);
            Color accent = Color.FromArgb(_hoverBg.R, _hoverBg.G, _hoverBg.B);

            if (_toggled)
            {
                using (var path = GlassFx.RoundRect(pad, 9))
                {
                    using (var glow = new SolidBrush(Color.FromArgb(60, VlcTheme.Neon)))
                        g.FillPath(glow, path);
                    using (var sheen = new LinearGradientBrush(pad,
                        Color.FromArgb(90, VlcTheme.NeonHot), Color.FromArgb(20, VlcTheme.Neon), 90f))
                        g.FillPath(sheen, path);
                    using (var pen = new Pen(Color.FromArgb(180, VlcTheme.NeonHot), 1f))
                        g.DrawPath(pen, path);
                }
            }
            else if (_hoverAlpha > 0)
            {
                int a = _hoverAlpha;
                using (var path = GlassFx.RoundRect(pad, 9))
                {
                    using (var sheen = new LinearGradientBrush(pad,
                        Color.FromArgb(a * 40 / 255, 255, 255, 255),
                        Color.FromArgb(a * 8 / 255, 255, 255, 255), 90f))
                        g.FillPath(sheen, path);
                    using (var tint = new SolidBrush(Color.FromArgb(a * 40 / 255, accent)))
                        g.FillPath(tint, path);
                    using (var pen = new Pen(Color.FromArgb(a * 90 / 255, accent), 1f))
                        g.DrawPath(pen, path);
                }
            }

            Color textColor = _toggled
                ? VlcTheme.NeonHot
                : (_hovered ? VlcTheme.TextPrimary : ForeColor);
            TextRenderer.DrawText(g, Text, Font, ClientRectangle, textColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);
        }
    }
}

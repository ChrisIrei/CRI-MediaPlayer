using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace MediaPlayerApp
{
    public class VlcSlider : Control
    {
        public float Value { get; set; } = 0f;
        public Color ThumbColor { get; set; } = Color.White;
        public Color TrackFillColor { get; set; } = VlcTheme.Neon;
        public Color TrackColor { get; set; } = VlcTheme.SliderTrack;

        private bool _hovered;
        private bool _dragging;

        public event EventHandler ValueChanged;

        public VlcSlider()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            Cursor = Cursors.Hand;
            Height = 20;
        }

        protected override void OnMouseEnter(EventArgs e) { _hovered = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hovered = false; if (!_dragging) Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e) { _dragging = true; UpdateValue(e.X); base.OnMouseDown(e); }
        protected override void OnMouseMove(MouseEventArgs e) { if (_dragging) UpdateValue(e.X); base.OnMouseMove(e); }
        protected override void OnMouseUp(MouseEventArgs e) { _dragging = false; Invalidate(); base.OnMouseUp(e); }

        private void UpdateValue(int x)
        {
            Value = Math.Max(0f, Math.Min(1f, (float)x / Width));
            ValueChanged?.Invoke(this, EventArgs.Empty);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            bool active = _hovered || _dragging;
            int trackH = active ? 6 : 4;
            int trackY = (Height - trackH) / 2;
            int fillW = (int)(Width * Value);

            using (var path = RoundedRect(new Rectangle(0, trackY, Width, trackH), trackH / 2))
            using (var br = new SolidBrush(TrackColor))
                g.FillPath(br, path);

            if (fillW > 1)
            {
                using (var glow = RoundedRect(new Rectangle(-2, trackY - 3, fillW + 4, trackH + 6), (trackH + 6) / 2))
                using (var gb = new SolidBrush(Color.FromArgb(active ? 90 : 55, TrackFillColor)))
                    g.FillPath(gb, glow);

                using (var path = RoundedRect(new Rectangle(0, trackY, fillW, trackH), trackH / 2))
                using (var lgb = new LinearGradientBrush(
                    new Rectangle(0, trackY, Math.Max(1, fillW), trackH),
                    VlcTheme.NeonHot, TrackFillColor, LinearGradientMode.Horizontal))
                    g.FillPath(lgb, path);
            }

            if (active)
            {
                int r = 7;
                int tx = Math.Max(0, Math.Min(Width - r * 2, fillW - r));
                int ty = (Height - r * 2) / 2;
                var thumb = new Rectangle(tx, ty, r * 2, r * 2);

                using (var halo = new SolidBrush(Color.FromArgb(120, TrackFillColor)))
                    g.FillEllipse(halo, tx - 4, ty - 4, r * 2 + 8, r * 2 + 8);
                using (var br = new SolidBrush(Color.White))
                    g.FillEllipse(br, thumb);
                using (var pen = new Pen(Color.FromArgb(220, VlcTheme.NeonHot), 1.5f))
                    g.DrawEllipse(pen, thumb);
            }
        }

        private GraphicsPath RoundedRect(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            int d = Math.Max(1, radius * 2);
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}

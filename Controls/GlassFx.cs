using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace MediaPlayerApp
{
    public interface IGlassSurface
    {
        void PaintGlassBackground(Graphics g);
    }

    public static class GlassFx
    {
        public static void PaintParentSurface(Graphics g, Control child)
        {
            if (child.Parent is IGlassSurface surface)
            {
                var state = g.Save();
                g.TranslateTransform(-child.Left, -child.Top);
                surface.PaintGlassBackground(g);
                g.Restore(state);
            }
            else
            {
                using (var br = new SolidBrush(child.BackColor))
                    g.FillRectangle(br, child.ClientRectangle);
            }
        }

        public static GraphicsPath RoundRect(Rectangle r, int radius)
        {
            var p = new GraphicsPath();
            int d = System.Math.Max(1, radius * 2);
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }

        public static void FillPlayTriangle(Graphics g, float cx, float cy, float size, Color color)
        {
            float tw = size, th = size * 1.12f;
            float nudge = tw * 0.16f;
            float left = cx - tw / 2f + nudge;
            using (var path = new GraphicsPath())
            {
                path.AddPolygon(new[]
                {
                    new PointF(left,      cy - th / 2f),
                    new PointF(left,      cy + th / 2f),
                    new PointF(left + tw, cy)
                });
                using (var br = new SolidBrush(color))
                    g.FillPath(br, path);
            }
        }
    }

    public class GlassBar : Panel, IGlassSurface
    {
        public enum Edge { None, Top, Bottom }
        public Edge GlowEdge { get; set; } = Edge.None;

        public GlassBar()
        {
            this.DoubleBuffered = true;
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
        }

        protected override void OnPaintBackground(PaintEventArgs e) => PaintGlassBackground(e.Graphics);

        public void PaintGlassBackground(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (var baseBr = new SolidBrush(BackColor))
                g.FillRectangle(baseBr, ClientRectangle);
            using (var sheen = new LinearGradientBrush(
                new Rectangle(0, 0, Width, System.Math.Max(1, Height)),
                Color.FromArgb(26, 255, 255, 255),
                Color.FromArgb(4,  255, 255, 255), 90f))
                g.FillRectangle(sheen, ClientRectangle);

            if (GlowEdge != Edge.None)
            {
                int y = GlowEdge == Edge.Top ? 0 : Height - 1;
                using (var lgb = new LinearGradientBrush(new Rectangle(0, 0, Width, 1),
                    Color.FromArgb(0, VlcTheme.Neon), Color.FromArgb(150, VlcTheme.Neon), 0f))
                {
                    lgb.Blend = new Blend
                    {
                        Factors   = new float[] { 0f, 1f, 0f },
                        Positions = new float[] { 0f, 0.5f, 1f }
                    };
                    using (var pen = new Pen(lgb, 1f))
                        g.DrawLine(pen, 0, y, Width, y);
                }
            }
        }
    }

    public class GlassBackgroundPanel : Panel, IGlassSurface
    {
        public GlassBackgroundPanel()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override void OnResize(System.EventArgs e) { base.OnResize(e); Invalidate(); }

        protected override void OnPaintBackground(PaintEventArgs e) => PaintGlassBackground(e.Graphics);

        public void PaintGlassBackground(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (var lgb = new LinearGradientBrush(ClientRectangle,
                VlcTheme.BgDeep, VlcTheme.BgDeep2, 55f))
                g.FillRectangle(lgb, ClientRectangle);

            Blob(g, new PointF(Width * 0.18f, Height * 0.28f), Width * 0.60f, VlcTheme.Neon,       82);
            Blob(g, new PointF(Width * 0.84f, Height * 0.80f), Width * 0.60f, VlcTheme.NeonPink,   72);
            Blob(g, new PointF(Width * 0.92f, Height * 0.10f), Width * 0.40f, VlcTheme.NeonPurple, 55);
            Blob(g, new PointF(Width * 0.06f, Height * 0.95f), Width * 0.36f, VlcTheme.NeonPurple, 42);
        }

        private static void Blob(Graphics g, PointF center, float radius, Color color, int peakAlpha)
        {
            using (var path = new GraphicsPath())
            {
                path.AddEllipse(center.X - radius, center.Y - radius, radius * 2, radius * 2);
                using (var pgb = new PathGradientBrush(path))
                {
                    pgb.CenterColor = Color.FromArgb(peakAlpha, color);
                    pgb.SurroundColors = new[] { Color.FromArgb(0, color) };
                    pgb.CenterPoint = center;
                    g.FillPath(pgb, path);
                }
            }
        }
    }
}

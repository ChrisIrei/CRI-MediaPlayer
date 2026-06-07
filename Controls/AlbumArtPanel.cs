using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace MediaPlayerApp
{
    public class AlbumArtPanel : Panel
    {
        private static readonly Image _art = LoadArt();

        public AlbumArtPanel()
        {
            this.DoubleBuffered = true;
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
        }

        private static Image LoadArt()
        {
            try
            {
                var asm = typeof(AlbumArtPanel).Assembly;
                string name = Array.Find(asm.GetManifestResourceNames(),
                    n => n.EndsWith("playbtn.png", StringComparison.OrdinalIgnoreCase));
                if (name == null) return null;
                using (var s = asm.GetManifestResourceStream(name))
                using (var tmp = Image.FromStream(s))
                    return new Bitmap(tmp);
            }
            catch { return null; }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int rad = 22;
            var inner = new Rectangle(2, 2, Width - 5, Height - 5);

            GlassFx.PaintParentSurface(g, this);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (var path = GlassFx.RoundRect(inner, rad))
            {
                g.SetClip(path);
                using (var sheen = new LinearGradientBrush(inner,
                    Color.FromArgb(26, 255, 255, 255),
                    Color.FromArgb(4,  255, 255, 255), 120f))
                    g.FillPath(sheen, path);
                g.ResetClip();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int rad = 22;
            var ring = new Rectangle(2, 2, Width - 5, Height - 5);

            if (_art != null)
            {
                int side = (int)(Math.Min(Width, Height) * 0.46);
                var dest = new Rectangle((Width - side) / 2, (Height - side) / 2, side, side);
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.Half;
                g.DrawImage(_art, dest);
            }

            using (var path = GlassFx.RoundRect(Rectangle.Inflate(ring, 3, 3), rad + 3))
            using (var pen = new Pen(Color.FromArgb(50, VlcTheme.Neon), 4f))
                g.DrawPath(pen, path);

            using (var path = GlassFx.RoundRect(ring, rad))
            using (var lgb = new LinearGradientBrush(new Rectangle(0, 0, Width, Height),
                VlcTheme.Neon, VlcTheme.NeonPink, 45f))
            using (var pen = new Pen(lgb, 2f))
                g.DrawPath(pen, path);
        }
    }
}

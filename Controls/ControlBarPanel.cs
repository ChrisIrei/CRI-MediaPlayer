using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace MediaPlayerApp
{
    public class ControlBarPanel : Panel, IGlassSurface
    {
        public ControlBarPanel()
        {
            this.DoubleBuffered = true;
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
        }

        protected override void OnPaintBackground(PaintEventArgs e) => PaintGlassBackground(e.Graphics);

        public void PaintGlassBackground(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (var baseBr = new SolidBrush(VlcTheme.BgControl))
                g.FillRectangle(baseBr, ClientRectangle);
            using (var sheen = new LinearGradientBrush(ClientRectangle,
                Color.FromArgb(30, 255, 255, 255),
                Color.FromArgb(4,  255, 255, 255), 90f))
                g.FillRectangle(sheen, ClientRectangle);
        }
    }
}

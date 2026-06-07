using System;
using System.Drawing;

namespace MediaPlayerApp
{
    public partial class MainForm
    {
        private void LayoutControls()
        {
            if (controlBar == null) return;
            int w = controlBar.Width;
            int pad = 16;

            btnClose.Location    = new Point(w - 48, 0);
            btnMaximize.Location = new Point(w - 94, 0);
            btnMinimize.Location = new Point(w - 140, 0);

            int seekY = 14;
            lblCurrentTime.Location = new Point(pad, seekY + 2);
            int timeW = 42;
            seekSlider.Height   = 28;
            seekSlider.Location = new Point(pad + timeW + 4, seekY - 4);
            seekSlider.Width    = w - pad * 2 - timeW * 2 - 8;
            lblTotalTime.Location = new Point(w - pad - timeW, seekY + 2);

            int btnY = 50;
            int cx = w / 2;
            int centerY = btnY + 20;

            btnPlay.Size = new Size(56, 56);
            btnPrev.Size = new Size(40, 40);
            btnNext.Size = new Size(40, 40);
            btnStop.Size = new Size(36, 36);
            btnShuffle.Size = new Size(36, 36);
            btnRepeat.Size = new Size(36, 36);

            int gap = 16;

            btnPlay.Location = new Point(cx - btnPlay.Width / 2, centerY - btnPlay.Height / 2);

            btnPrev.Location = new Point(btnPlay.Left - gap - btnPrev.Width, centerY - btnPrev.Height / 2);
            btnStop.Location = new Point(btnPrev.Left - gap - btnStop.Width, centerY - btnStop.Height / 2);
            btnShuffle.Location = new Point(btnStop.Left - gap - btnShuffle.Width, centerY - btnShuffle.Height / 2);

            btnNext.Location = new Point(btnPlay.Right + gap, centerY - btnNext.Height / 2);
            btnRepeat.Location = new Point(btnNext.Right + gap, centerY - btnRepeat.Height / 2);

            btnMute.Size     = new Size(36, 36);
            volumeSlider.Width    = 100;
            volumeSlider.Height   = 26;

            int rightX = w - pad;
            rightX -= volumeSlider.Width;
            volumeSlider.Location = new Point(rightX, centerY - volumeSlider.Height / 2);

            rightX -= lblVolumeIcon.Width + 4;
            lblVolumeIcon.Location = new Point(rightX, centerY - lblVolumeIcon.Height / 2);

            rightX -= btnMute.Width + 12;
            btnMute.Location = new Point(rightX, centerY - btnMute.Height / 2);

            btnOpen.Size     = new Size(36, 36);
            btnOpen.Location = new Point(pad, btnY + 2);

            btnFullscreen.Size     = new Size(36, 36);
            btnFullscreen.Location = new Point(pad + 44, btnY + 2);

            if (infoPanel != null)
            {
                lblFileName.Width  = infoPanel.Width - 40;
                lblMediaMeta.Width = infoPanel.Width - 40;
            }

            if (videoPanel != null && albumArtPanel != null)
            {
                int artSize = Math.Min(Math.Min(videoPanel.Width, videoPanel.Height) - 60, 280);
                if (artSize < 80) artSize = 80;
                albumArtPanel.Size = new Size(artSize, artSize);
                albumArtPanel.Location = new Point(
                    (videoPanel.Width - artSize) / 2,
                    (videoPanel.Height - artSize) / 2);
            }

            controlBar.Invalidate();
            titleBar.Invalidate();
        }
    }
}

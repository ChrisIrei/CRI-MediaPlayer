using System;
using System.Drawing;
using System.Windows.Forms;
using WMPLib;

namespace MediaPlayerApp
{
    public partial class MainForm
    {
        protected override CreateParams CreateParams
        {
            get
            {
                const int WS_THICKFRAME  = 0x00040000;
                const int WS_MINIMIZEBOX = 0x00020000;
                var cp = base.CreateParams;
                cp.Style |= WS_THICKFRAME | WS_MINIMIZEBOX;
                return cp;
            }
        }

        private void TitleBar_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) { _titleDragging = true; _dragStart = e.Location; }
        }

        private void TitleBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (_titleDragging && e.Button == MouseButtons.Left)
            {
                Point delta = new Point(e.Location.X - _dragStart.X, e.Location.Y - _dragStart.Y);
                this.Location = new Point(this.Location.X + delta.X, this.Location.Y + delta.Y);
            }
        }

        private void TitleBar_MouseUp(object sender, MouseEventArgs e) { _titleDragging = false; }

        private void ToggleMaximize()
        {
            if (_isFullscreen) return;
            if (_isMaximized)
            {
                this.WindowState = FormWindowState.Normal;
                _isMaximized = false;
            }
            else
            {
                this.WindowState = FormWindowState.Maximized;
                _isMaximized = true;
            }
        }

        private void ToggleFullscreen()
        {
            if (_isFullscreen)
            {
                titleBar.Visible  = true;
                infoPanel.Visible = true;
                controlBar.Visible = true;
                this.WindowState = _isMaximized ? FormWindowState.Maximized : FormWindowState.Normal;
                btnFullscreen.Toggled = false;
                _isFullscreen = false;
                _fsControls?.HideControls();
            }
            else
            {
                titleBar.Visible  = false;
                infoPanel.Visible = false;
                controlBar.Visible = false;
                this.WindowState = FormWindowState.Maximized;
                btnFullscreen.Toggled = true;
                _isFullscreen = true;
                SyncFsControls();
                _fsControls?.ShowControls(this.Bounds);
            }
        }

        private void RevealFsControls()
        {
            if (_isFullscreen && _fsControls != null)
                _fsControls.ShowControls(this.Bounds);
        }

        private void SyncFsControls()
        {
            if (_fsControls == null) return;
            _fsControls.SetPlaying(axWindowsMediaPlayer.playState == WMPPlayState.wmppsPlaying);
            _fsControls.SetVolume((float)_savedVolume / 100f);
            _fsControls.SetVolumeIcon(lblVolumeIcon.Text, _isMuted);
            try
            {
                double dur = axWindowsMediaPlayer.currentMedia?.duration ?? 0;
                double pos = axWindowsMediaPlayer.Ctlcontrols.currentPosition;
                _fsControls.UpdateProgress(pos, dur);
            }
            catch { }
        }

        private void FsSeekTo(double fraction)
        {
            try
            {
                double duration = axWindowsMediaPlayer.currentMedia?.duration ?? 0;
                if (duration > 0)
                    axWindowsMediaPlayer.Ctlcontrols.currentPosition = fraction * duration;
            }
            catch { }
        }

        private void FsVolumeTo(float value)
        {
            _savedVolume = (int)(value * 100);
            _isMuted = false;
            axWindowsMediaPlayer.settings.volume = _savedVolume;
            volumeSlider.Value = value;
            volumeSlider.Invalidate();
            UpdateVolumeIcon();
        }
    }
}

using System;
using System.IO;
using System.Windows.Forms;
using WMPLib;
using AxWMPLib;

namespace MediaPlayerApp
{
    public partial class MainForm
    {
        private static readonly string[] VideoExtensions =
            { ".mp4", ".avi", ".wmv", ".mkv", ".mov", ".m4v", ".flv", ".webm" };

        private void BtnOpen_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Title = "Open Media File";
                ofd.Filter = "All Media|*.mp3;*.mp4;*.avi;*.wmv;*.wav;*.flac;*.mkv;*.m4a;*.ogg;*.aac|" +
                             "Audio|*.mp3;*.wav;*.flac;*.m4a;*.ogg;*.aac|" +
                             "Video|*.mp4;*.avi;*.wmv;*.mkv|All Files|*.*";
                if (ofd.ShowDialog() == DialogResult.OK)
                    LoadFile(ofd.FileName);
            }
        }

        private void LoadFile(string path)
        {
            axWindowsMediaPlayer.URL = path;
            try { axWindowsMediaPlayer.uiMode = "none"; } catch { }
            axWindowsMediaPlayer.settings.volume = _isMuted ? 0 : _savedVolume;

            string ext = Path.GetExtension(path).ToLowerInvariant();
            bool isVideo = Array.IndexOf(VideoExtensions, ext) >= 0;

            albumArtPanel.Visible   = !isVideo;
            ambientBackdrop.Visible = !isVideo;

            lblFileName.Text  = Path.GetFileNameWithoutExtension(path);
            lblMediaMeta.Text = Path.GetExtension(path).TrimStart('.').ToUpperInvariant() +
                                "  •  " + new FileInfo(path).DirectoryName;
            axWindowsMediaPlayer.Ctlcontrols.play();
            progressTimer.Start();
            btnPlay.IsPlaying = true;
            btnPlay.Invalidate();
            titleBar.Invalidate();
        }

        private void BtnPlay_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(SafeGetUrl())) return;

            var state = axWindowsMediaPlayer.playState;
            if (state == WMPPlayState.wmppsPlaying)
            {
                axWindowsMediaPlayer.Ctlcontrols.pause();
                progressTimer.Stop();
                btnPlay.IsPlaying = false;
            }
            else
            {
                axWindowsMediaPlayer.Ctlcontrols.play();
                progressTimer.Start();
                btnPlay.IsPlaying = true;
            }
            btnPlay.Invalidate();
            _fsControls?.SetPlaying(btnPlay.IsPlaying);
        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            axWindowsMediaPlayer.Ctlcontrols.stop();
            progressTimer.Stop();
            seekSlider.Value = 0;
            seekSlider.Invalidate();
            lblCurrentTime.Text = "0:00";
            btnPlay.IsPlaying = false;
            btnPlay.Invalidate();
        }

        private void BtnPrev_Click(object sender, EventArgs e)
        {
            try { axWindowsMediaPlayer.Ctlcontrols.currentPosition = 0; } catch { }
        }

        private void BtnNext_Click(object sender, EventArgs e)
        {
            try { axWindowsMediaPlayer.Ctlcontrols.next(); } catch { }
        }

        private void BtnMute_Click(object sender, EventArgs e)
        {
            _isMuted = !_isMuted;
            axWindowsMediaPlayer.settings.volume = _isMuted ? 0 : _savedVolume;
            UpdateVolumeIcon();
        }

        private void UpdateVolumeIcon()
        {
            if (_isMuted || _savedVolume == 0) lblVolumeIcon.Text = "🔇";
            else if (_savedVolume < 40)         lblVolumeIcon.Text = "🔈";
            else if (_savedVolume < 75)         lblVolumeIcon.Text = "🔉";
            else                                lblVolumeIcon.Text = "🔊";
            btnMute.Toggled = _isMuted;
            _fsControls?.SetVolumeIcon(lblVolumeIcon.Text, _isMuted);
        }

        private void ProgressTimer_Tick(object sender, EventArgs e)
        {
            if (_seekDragging) return;
            try
            {
                double duration = axWindowsMediaPlayer.currentMedia?.duration ?? 0;
                double position = axWindowsMediaPlayer.Ctlcontrols.currentPosition;
                if (duration > 0)
                {
                    seekSlider.Value = (float)(position / duration);
                    seekSlider.Invalidate();
                    lblCurrentTime.Text = FormatTime(position);
                    lblTotalTime.Text   = FormatTime(duration);
                    if (_isFullscreen && _fsControls != null && _fsControls.Visible)
                        _fsControls.UpdateProgress(position, duration);
                }
            }
            catch { }
        }

        private void SeekSlider_ValueChanged(object sender, EventArgs e)
        {
            if (_seekDragging)
            {
                try
                {
                    double duration = axWindowsMediaPlayer.currentMedia?.duration ?? 0;
                    if (duration > 0)
                        lblCurrentTime.Text = FormatTime(seekSlider.Value * duration);
                }
                catch { }
            }
        }

        private void WMP_PlayStateChange(object sender, _WMPOCXEvents_PlayStateChangeEvent e)
        {
            var state = (WMPPlayState)e.newState;
            if (state == WMPPlayState.wmppsStopped || state == WMPPlayState.wmppsMediaEnded)
            {
                progressTimer.Stop();
                btnPlay.IsPlaying = false;
                btnPlay.Invalidate();
                _fsControls?.SetPlaying(false);
                if (state == WMPPlayState.wmppsMediaEnded)
                {
                    if (_repeatOn)
                    {
                        axWindowsMediaPlayer.Ctlcontrols.currentPosition = 0;
                        axWindowsMediaPlayer.Ctlcontrols.play();
                        progressTimer.Start();
                        btnPlay.IsPlaying = true;
                        btnPlay.Invalidate();
                    }
                    else
                    {
                        seekSlider.Value = 0;
                        seekSlider.Invalidate();
                        lblCurrentTime.Text = "0:00";
                    }
                }
            }
            else if (state == WMPPlayState.wmppsPlaying)
            {
                btnPlay.IsPlaying = true;
                btnPlay.Invalidate();
                _fsControls?.SetPlaying(true);
            }
        }

        private void WMP_OpenStateChange(object sender, _WMPOCXEvents_OpenStateChangeEvent e) { }

        private void AxWindowsMediaPlayer_MouseDownEvent(object sender, _WMPOCXEvents_MouseDownEvent e)
        {
            if (e.nButton == 2 && _isFullscreen)
                _fsMenu.Show(Cursor.Position);
        }
    }
}

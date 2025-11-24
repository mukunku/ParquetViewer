
using NAudio.FileFormats.Wav;
using NAudio.Wave;
using ParquetViewer.Analytics;
using ParquetViewer.Engine.Types;
using ParquetViewer.Helpers;
using ParquetViewer.Properties;
using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ParquetViewer.Controls
{
    internal class AudioPlayerDataGridViewCell : DataGridViewTextBoxCell
    {
        private WaveStream? _audioStream;
        private IWavePlayer? _audioPlayer;
        private Timer _updateTimer = new() { Interval = 100 };
        private Timer _initializationTimer = new() { Interval = 100 };

        private bool _isInitialized = false;
        private bool? _isValidWavFile = null;
        private string _errorMessage = "loading...";
        private bool _isCellTooSmall = false;

        private Rectangle _cellBounds;
        private Rectangle _playPauseButtonBounds;
        private Rectangle _stopButtonBounds;
        private Rectangle _trackBarBounds;
        private Rectangle _contextMenuButtonBounds;

        private string? _debugText = null;
        private bool _isCursorHoveringPlayPauseButton;
        private bool _isCursorHoveringStopButton;
        private bool _isCursorHoveringMenuButton;
        private bool _isPlaying = false;

        private TimeSpan? _postStopSeekLocation;

        public AudioPlayerDataGridViewCell()
        {
            _updateTimer.Tick += (sender, e) => RedrawCell();
            _initializationTimer.Tick += (sender, e) =>
            {
                if (this._isInitialized)
                {
                    _initializationTimer.Stop();
                    RedrawCell();
                }
            };
            _initializationTimer.Start();
        }

        private void RedrawCell() => DataGridView?.InvalidateCell(this);

        private Task? _initializationTask = null;
        private Task InitializePlayerAsync()
            => _initializationTask ??= Task.Run(() =>
            {
                try
                {
                    //Prepare audio stream
                    if (this.Value is ByteArrayValue byteArray)
                    {
                        //It's somewhat safe to not dispose memory streams: https://stackoverflow.com/a/4274769/1458738
                        var ms = new MemoryStream(byteArray.Data);
                        this._audioStream = new WaveFileReader(ms);
                        this._isValidWavFile = true;

                        //Prepare output device
                        this._audioPlayer = new WaveOutEvent();

                        this._audioPlayer.Init(this._audioStream);
                        this._audioPlayer.PlaybackStopped += OnPlaybackStopped;
                    }
                    else if (this.Value == DBNull.Value)
                    {
                        this._isValidWavFile = false;
                    }
                    else
                    {
                        throw new InvalidDataException($"{this.ValueType.Name} was not the expected type {nameof(ByteArrayValue)}");
                    }
                }
                catch (Exception ex)
                {
                    this._isValidWavFile = false;
                    this._errorMessage = ex.Message.Left(50);
                }
                finally
                {
                    this._isInitialized = true;
                }
            });

        private void OnPlaybackStopped(object? source, StoppedEventArgs args)
        {
            this._updateTimer.Stop();
            this._isPlaying = false;

            if (this._audioStream is not null)
            {
                this._audioStream.Position = 0;

                if (this._postStopSeekLocation is not null)
                {
                    this._audioStream.CurrentTime = this._postStopSeekLocation.Value;
                    this._postStopSeekLocation = null;
                }
            }

            this.RedrawCell();
        }

        protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates cellState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
        {
            InitializePlayerAsync(); //Trigger initialization if it wasn't performed yet

            //Call base render so we have a nice base to draw on
            base.Paint(graphics, clipBounds, cellBounds, rowIndex, cellState, value, formattedValue, errorText, cellStyle, advancedBorderStyle,
                paintParts & ~DataGridViewPaintParts.ContentForeground & ~DataGridViewPaintParts.SelectionBackground);

            if (value == null || value == DBNull.Value)
            {
                return;
            }

            if (this._isValidWavFile != true || !this._isInitialized)
            {
                TextRenderer.DrawText(graphics, this._errorMessage, cellStyle.Font, cellBounds, cellStyle.ForeColor, TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
                return;
            }

            if (this._audioStream is null || this._audioPlayer is null)
                return; //this shouldn't happen if we're initialized

            // Define UI element bounds
            var buttonHeight = Math.Max(cellBounds.Height - 4, 18);
            var buttonMaxWidth = ((cellBounds.Right - cellBounds.Left) * 25) / 100; // %25 of the cell width
            var buttonWidth = Math.Min(buttonHeight, buttonMaxWidth);
            this._playPauseButtonBounds = new Rectangle(cellBounds.Left + 2, cellBounds.Top + 2, buttonWidth, buttonHeight);
            this._stopButtonBounds = new Rectangle(_playPauseButtonBounds.Right + 2, cellBounds.Top + 2, buttonWidth, buttonHeight);
            this._contextMenuButtonBounds = new Rectangle(cellBounds.Right - buttonWidth - 2, cellBounds.Top + 2, buttonWidth, buttonHeight);
            this._trackBarBounds = new Rectangle(_stopButtonBounds.Right + 2, cellBounds.Top + 2, _contextMenuButtonBounds.Left - _stopButtonBounds.Right - 4, cellBounds.Height - 4);
            this._cellBounds = cellBounds;

            this._isCellTooSmall = !(this._cellBounds.Height > 20 && this._cellBounds.Width > 90);
            if (!this._isCellTooSmall)
            {
                using var foreColorBrush = new SolidBrush(Theme.LightModeTheme.TextColor);

                // Draw Buttons
                ControlPaint.DrawButton(graphics, _playPauseButtonBounds, GetButtonState(this._isCursorHoveringPlayPauseButton));
                ControlPaint.DrawButton(graphics, _stopButtonBounds, GetButtonState(this._isCursorHoveringStopButton));
                ControlPaint.DrawButton(graphics, _contextMenuButtonBounds, GetButtonState(this._isCursorHoveringMenuButton));

                if (this._isPlaying) // Draw Pause
                {
                    using var pen = new Pen(Theme.LightModeTheme.TextColor, buttonWidth / 9.8f);
                    var centerPoint = _playPauseButtonBounds.Left + (_playPauseButtonBounds.Right - _playPauseButtonBounds.Left) / 2;
                    var padding = buttonHeight / 8f;
                    graphics.DrawLine(pen, centerPoint + padding, _playPauseButtonBounds.Top + 6, centerPoint + padding, _playPauseButtonBounds.Bottom - 6);
                    graphics.DrawLine(pen, centerPoint - padding, _playPauseButtonBounds.Top + 6, centerPoint - padding, _playPauseButtonBounds.Bottom - 6);
                }
                else // Draw Play
                {
                    Point[] triangle = {
                        new (_playPauseButtonBounds.Left + 6, _playPauseButtonBounds.Top + 5),
                        new (_playPauseButtonBounds.Right - 6, _playPauseButtonBounds.Top + (_playPauseButtonBounds.Height / 2)),
                        new (_playPauseButtonBounds.Left + 6, _playPauseButtonBounds.Bottom - 6)
                    };
                    graphics.FillPolygon(foreColorBrush, triangle);
                }

                // Draw Stop icon
                graphics.FillRectangle(foreColorBrush, _stopButtonBounds.X + 6, _stopButtonBounds.Y + 7, _stopButtonBounds.Width - 14, _stopButtonBounds.Height - 14);

                // Draw Context Menu icon (ellipsis)
                int dotSize = 4;
                int dotSpacing = 5;
                int centerX = _contextMenuButtonBounds.X + _contextMenuButtonBounds.Width / 2;
                int centerY = _contextMenuButtonBounds.Y + _contextMenuButtonBounds.Height / 2;
                graphics.FillEllipse(foreColorBrush, centerX - dotSize / 2 - dotSpacing, centerY - dotSize / 2, dotSize, dotSize);
                graphics.FillEllipse(foreColorBrush, centerX - dotSize / 2, centerY - dotSize / 2, dotSize, dotSize);
                graphics.FillEllipse(foreColorBrush, centerX - dotSize / 2 + dotSpacing, centerY - dotSize / 2, dotSize, dotSize);
            }
            else
            {
                this._trackBarBounds = this._cellBounds;
            }

            // Draw Track Bar
            using var trackBarBrush = new SolidBrush(Color.FromArgb(185, Color.DodgerBlue));
            double progress = this._audioStream.CurrentTime.TotalSeconds / this._audioStream.TotalTime.TotalSeconds;
            int progressWidth = (int)(_trackBarBounds.Width * progress);
            graphics.FillRectangle(Brushes.LightGray, _trackBarBounds);
            graphics.FillRectangle(trackBarBrush, _trackBarBounds.X, _trackBarBounds.Y, progressWidth, _trackBarBounds.Height); //Brushes.DodgerBlue
            ControlPaint.DrawBorder3D(graphics, _trackBarBounds, Border3DStyle.Sunken);

            // Draw Time
            string timeFormat = this._audioStream.TotalTime.TotalHours >= 1 ? @"hh\:mm\:ss" : @"mm\:ss";
            timeFormat += this._audioStream.TotalTime.TotalSeconds < 0 ? @"\.fff" : string.Empty; //show milliseconds if the audio is less than 1 second
            string currentTime = this._audioStream.CurrentTime.ToString(timeFormat) ?? TimeSpan.FromSeconds(0).ToString(timeFormat);
            string totalTime = this._audioStream.TotalTime.ToString(timeFormat) ?? TimeSpan.FromSeconds(0).ToString(timeFormat);
            TextRenderer.DrawText(graphics, _debugText ?? $"{currentTime} / {totalTime}", cellStyle.Font, _trackBarBounds, Theme.LightModeTheme.TextColor, TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
        }

        protected override void OnMouseMove(DataGridViewCellMouseEventArgs e)
        {
            base.OnMouseMove(e);

            Rectangle translatedBounds = this._playPauseButtonBounds;
            translatedBounds.Offset(-this._cellBounds.Location.X, -this._cellBounds.Location.Y);
            this._isCursorHoveringPlayPauseButton = ContainsCursor(this._playPauseButtonBounds, e.Location);
            this._isCursorHoveringStopButton = ContainsCursor(this._stopButtonBounds, e.Location);
            this._isCursorHoveringMenuButton = ContainsCursor(this._contextMenuButtonBounds, e.Location);

            this.RedrawCell();
        }

        protected override void OnMouseLeave(int rowIndex)
        {
            base.OnMouseLeave(rowIndex);

            this._isCursorHoveringPlayPauseButton = false;
            this._isCursorHoveringStopButton = false;
            this._isCursorHoveringMenuButton = false;
            this._isLeftMouseButtonPressed = false; //better ux
            this.RedrawCell();
        }

        private bool ContainsCursor(Rectangle elementRectangle, Point cursorLocation)
        {
            elementRectangle.Offset(-this._cellBounds.Location.X, -this._cellBounds.Location.Y);
            return elementRectangle.Contains(cursorLocation);
        }

        private ButtonState GetButtonState(bool isCursorHoveringOverButton)
        {
            if (_isLeftMouseButtonPressed && isCursorHoveringOverButton)
            {
                return ButtonState.Pushed;
            }
            else if (isCursorHoveringOverButton)
            {
                return ButtonState.Normal;
            }
            else
            {
                return ButtonState.Flat;
            }
        }

        private bool _isLeftMouseButtonPressed = false;
        protected override void OnMouseDown(DataGridViewCellMouseEventArgs e)
        {
            this._isLeftMouseButtonPressed = e.Button == MouseButtons.Left;
        }
        protected override void OnMouseUp(DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                this._isLeftMouseButtonPressed = false;
        }

        protected override void OnMouseClick(DataGridViewCellMouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (e.Button != MouseButtons.Left)
                return;

            if (ContainsCursor(this._playPauseButtonBounds, e.Location) && !this._isCellTooSmall)
            {
                TogglePlayPause();
            }
            else if (ContainsCursor(this._stopButtonBounds, e.Location) && !this._isCellTooSmall)
            {
                StopPlayback();
            }
            else if (ContainsCursor(this._contextMenuButtonBounds, e.Location) && !this._isCellTooSmall)
            {
                ShowContextMenu(e.Location);
            }
            else if (ContainsCursor(this._trackBarBounds, e.Location))
            {
                Seek(e.Location);
            }
        }

        private bool _sentQuickPeekEvent = false;
        private void TogglePlayPause()
        {
            if (this._audioPlayer is null || this._audioStream is null)
                return;

            if (this._isPlaying)
            {
                this._audioPlayer.Pause();
                this._isPlaying = false;
                this._updateTimer.Stop();
            }
            else
            {
                this._audioPlayer.Play();
                this._isPlaying = true;
                this._updateTimer.Start();

                // Lets throttle events just in case. Not sure if it's necessary
                if (!this._sentQuickPeekEvent)
                {
                    QuickPeekEvent.FireAndForget(QuickPeekEvent.DataTypeId.Audio);
                    this._sentQuickPeekEvent = true;
                }
            }

            this.RedrawCell();
        }

        private void StopPlayback()
        {
            if (this._audioPlayer == null || this._audioPlayer == null)
                return;

            if (this._audioPlayer.PlaybackState != PlaybackState.Stopped)
                this._audioPlayer.Stop(); //Triggers playback stopped event
            else
            {
                //If we're already stopped, trigger the playback stopped event ourselves
                this.OnPlaybackStopped(null, new StoppedEventArgs());
            }
        }

        private void Seek(Point location)
        {
            if (this._audioPlayer is null || this._audioStream is null)
                return;

            var trackbarWidth = this._trackBarBounds.Right - this._trackBarBounds.Left;
            var cellboundsLeft = this._trackBarBounds.Left - this._cellBounds.X;
            var clickLocation = location.X - cellboundsLeft;

            var seekPercentage = (double)clickLocation / trackbarWidth;
            var seekLocation = TimeSpan.FromSeconds(this._audioStream.TotalTime.TotalSeconds * seekPercentage);
            if (this._audioPlayer.PlaybackState == PlaybackState.Stopped)
            {
                this._postStopSeekLocation = null;
                this._audioStream.CurrentTime = seekLocation;
            }
            else
            {
                this._postStopSeekLocation = seekLocation;
                this.StopPlayback(); //Stopping is recommended before seeking
            }

            DataGridView?.InvalidateCell(this);
        }

        private void ShowContextMenu(Point location)
        {
            if (this.DataGridView is null) //just in case
                return;

            if (this.Value is not ByteArrayValue byteArrayValue && false /*TODO: FOR TESTING. REMOVE!!!*/)
                return;

            var menu = new ContextMenuStrip();
            menu.Items.Add("Save as Wav", Resources.save_icon, async (s, a) =>
            {
                using var saveFileDialog = new SaveFileDialog
                {
                    Filter = "WAV file|*.wav",
                    Title = $"Save audio as WAV"
                };
                saveFileDialog.ShowDialog();

                if (string.IsNullOrWhiteSpace(saveFileDialog.FileName))
                    return;

                CleanupFile(saveFileDialog.FileName); //Delete any existing file (user already confirmed any overwrite)

                if (this.Value is not ByteArrayValue byteArray)
                    throw new InvalidDataException("Audio data was not found");

                await File.WriteAllBytesAsync(saveFileDialog.FileName, byteArray.Data);

                MessageBox.Show($"Audio saved to {saveFileDialog.FileName}", "Save complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            });

            if (this._audioStream is not null) //just in case
            {
                //Show additional metadata about the audio in the context menu
                menu.Items.Add(new ToolStripSeparator());

                var isFirst = true;
                foreach (var text in this._audioStream.WaveFormat.ToString().Split(":"))
                {
                    var waveFormatItem = new ToolStripButton((isFirst ? "Format: " : string.Empty) + text.Trim())
                    {
                        Enabled = false,
                        AutoToolTip = false,
                    };
                    menu.Items.Add(waveFormatItem);
                    isFirst = false;
                }
            }

            menu.Show(this.DataGridView, location + (Size)this._cellBounds.Location);
            menu.PerformLayout();

            void CleanupFile(string filePath)
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(filePath))
                        File.Delete(filePath);
                }
                catch (Exception) { /*Swallow*/ }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this._audioPlayer?.DisposeSafely();
                this._audioStream?.DisposeSafely();
                this._updateTimer.DisposeSafely();
                this._initializationTimer.DisposeSafely();
            }

            base.Dispose(disposing);
        }

        public static bool IsWavAudio(byte[] data)
        {
            try
            {
                using var ms = new MemoryStream(data);
                var wavReader = new WaveFileChunkReader();
                wavReader.ReadWaveHeader(ms);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
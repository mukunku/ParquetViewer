using ParquetViewer.Analytics;
using ParquetViewer.Engine.Types;
using ParquetViewer.Helpers;
using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ParquetViewer.Controls;

internal class AudioPlayerDataGridViewCell : DataGridViewTextBoxCell
{
    private AudioPlayer? _audioPlayer;
    private readonly Timer _updateTimer = new() { Interval = 100 };
    private readonly Timer _initializationTimer = new() { Interval = 100 };

    private volatile bool _isInitialized = false; //volatile because written by the background initialization task and read by the UI thread while painting.
    private readonly string _loadingMessage = "loading...";
    private readonly string _errorMessage = "#ERR";
    private bool _isCellTooSmall = false;

    private Rectangle _cellBounds;
    private Rectangle _playPauseButtonBounds;
    private Rectangle _stopButtonBounds;
    private Rectangle _trackBarBounds;
    private Rectangle _contextMenuButtonBounds;

    private bool _isCursorHoveringPlayPauseButton;
    private bool _isCursorHoveringStopButton;
    private bool _isCursorHoveringMenuButton;
    private bool _isPlaying = false;

    public AudioPlayerDataGridViewCell()
    {
        _updateTimer.Tick += (sender, e) => RedrawCell();
        _initializationTimer.Tick += (sender, e) =>
        {
            if (_isInitialized)
            {
                _initializationTimer.Stop();
                RedrawCell();
            }
        };
        _initializationTimer.Start();
    }

    private void RedrawCell() => DataGridView?.InvalidateCell(this);

    private Task? _initializationTask = null;

    /// <summary>
    /// Kicks off audio initialization on a background thread, once per cell.
    /// </summary>
    /// <remarks>Must be called from the UI thread: it reads the cell's Value before dispatching.</remarks>
    private Task InitializePlayerAsync()
    {
        if (_initializationTask is not null)
            return _initializationTask;

        //Read the cell state here rather than inside the task.
        var cellValue = Value;
        return _initializationTask = Task.Run(() =>
        {
            //Prepare audio stream
            if (cellValue is IByteArrayValue byteArray)
            {
                _audioPlayer = new AudioPlayer(byteArray.Data);
                _audioPlayer.PlaybackStopped += OnPlaybackStopped;
            }
            else
            {
                _audioPlayer = null;
            }

            _isInitialized = true;
        });
    }

    private void OnPlaybackStopped(object? source, EventArgs args)
    {
        if (DataGridView?.InvokeRequired == true) //NAudio captures the synchronization context so this check isn't needed actually...
        {
            DataGridView.Invoke(OnPlaybackStopped);
        }
        else
        {
            _updateTimer.Stop();
            _isPlaying = false;
            RedrawCell(); //Convert pause button to play button
        }
    }

    protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates cellState, object? value, object? formattedValue, string? errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
    {
        InitializePlayerAsync(); //Trigger initialization if it wasn't performed yet

        //Call base render so we have a nice base to draw on
        base.Paint(graphics, clipBounds, cellBounds, rowIndex, cellState, value, formattedValue, errorText, cellStyle, advancedBorderStyle,
            paintParts & ~DataGridViewPaintParts.ContentForeground & ~DataGridViewPaintParts.SelectionBackground);

        if (value is null || value == DBNull.Value || _audioPlayer is null)
        {
            return;
        }

        if (!_isInitialized)
        {
            TextRenderer.DrawText(graphics, _loadingMessage, cellStyle.Font, cellBounds, cellStyle.ForeColor, TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
            return;
        }
        if (_audioPlayer.AudioFormat == AudioPlayer.AudioFormatType.Invalid)
        {
            TextRenderer.DrawText(graphics, _errorMessage, cellStyle.Font, cellBounds, cellStyle.ForeColor, TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
            return;
        }

        // Define UI element bounds
        var buttonHeight = Math.Max(cellBounds.Height - 4, 18);
        var buttonMaxWidth = ((cellBounds.Right - cellBounds.Left) * 25) / 100; // %25 of the cell width
        var buttonWidth = Math.Min(buttonHeight, buttonMaxWidth);
        _playPauseButtonBounds = new Rectangle(cellBounds.Left + 2, cellBounds.Top + 2, buttonWidth, buttonHeight);
        _stopButtonBounds = new Rectangle(_playPauseButtonBounds.Right + 2, cellBounds.Top + 2, buttonWidth, buttonHeight);
        _contextMenuButtonBounds = new Rectangle(cellBounds.Right - buttonWidth - 2, cellBounds.Top + 2, buttonWidth, buttonHeight);
        _trackBarBounds = new Rectangle(_stopButtonBounds.Right + 2, cellBounds.Top + 2, _contextMenuButtonBounds.Left - _stopButtonBounds.Right - 4, cellBounds.Height - 4);
        _cellBounds = cellBounds;

        _isCellTooSmall = !(_cellBounds.Height > 20 && _cellBounds.Width > 90);
        if (!_isCellTooSmall)
        {
            using var foreColorBrush = new SolidBrush(Theme.LightModeTheme.TextColor);

            // Draw Buttons
            ControlPaint.DrawButton(graphics, _playPauseButtonBounds, GetButtonState(_isCursorHoveringPlayPauseButton));
            ControlPaint.DrawButton(graphics, _stopButtonBounds, GetButtonState(_isCursorHoveringStopButton));
            ControlPaint.DrawButton(graphics, _contextMenuButtonBounds, GetButtonState(_isCursorHoveringMenuButton));

            if (_isPlaying) // Draw Pause
            {
                using var pen = new Pen(Theme.LightModeTheme.TextColor, buttonWidth / 9.8f);
                var centerPoint = _playPauseButtonBounds.Left + (_playPauseButtonBounds.Right - _playPauseButtonBounds.Left) / 2;
                var padding = buttonHeight / 8f;
                graphics.DrawLine(pen, centerPoint + padding, _playPauseButtonBounds.Top + 6, centerPoint + padding, _playPauseButtonBounds.Bottom - 6);
                graphics.DrawLine(pen, centerPoint - padding, _playPauseButtonBounds.Top + 6, centerPoint - padding, _playPauseButtonBounds.Bottom - 6);
            }
            else // Draw Play
            {
                Point[] triangle = [
                    new (_playPauseButtonBounds.Left + 6, _playPauseButtonBounds.Top + 5),
                    new (_playPauseButtonBounds.Right - 6, _playPauseButtonBounds.Top + (_playPauseButtonBounds.Height / 2)),
                    new (_playPauseButtonBounds.Left + 6, _playPauseButtonBounds.Bottom - 6)
                ];
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
            _trackBarBounds = _cellBounds;
        }

        // Draw Track Bar
        using var trackBarBrush = new SolidBrush(Color.FromArgb(185, Color.DodgerBlue));
        double progress = _audioPlayer.CurrentTime.TotalSeconds / _audioPlayer.TotalTime.TotalSeconds;
        int progressWidth = (int)(_trackBarBounds.Width * progress);
        graphics.FillRectangle(Brushes.LightGray, _trackBarBounds);
        graphics.FillRectangle(trackBarBrush, _trackBarBounds.X, _trackBarBounds.Y, progressWidth, _trackBarBounds.Height); //Brushes.DodgerBlue
        ControlPaint.DrawBorder3D(graphics, _trackBarBounds, Border3DStyle.Sunken);

        // Draw Time
        string timeFormat = _audioPlayer.TotalTime.TotalHours >= 1 ? @"hh\:mm\:ss" : @"mm\:ss";
        timeFormat += _audioPlayer.TotalTime.TotalSeconds < 0 ? @"\.fff" : string.Empty; //show milliseconds if the audio is less than 1 second
        string currentTime = _audioPlayer.CurrentTime.ToString(timeFormat) ?? TimeSpan.FromSeconds(0).ToString(timeFormat);
        string totalTime = _audioPlayer.TotalTime.ToString(timeFormat) ?? TimeSpan.FromSeconds(0).ToString(timeFormat);
        TextRenderer.DrawText(graphics, $"{currentTime} / {totalTime}", cellStyle.Font, _trackBarBounds, Theme.LightModeTheme.TextColor, TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
    }

    protected override void OnMouseMove(DataGridViewCellMouseEventArgs e)
    {
        base.OnMouseMove(e);

        _isCursorHoveringPlayPauseButton = ContainsCursor(_playPauseButtonBounds, e.Location);
        _isCursorHoveringStopButton = ContainsCursor(_stopButtonBounds, e.Location);
        _isCursorHoveringMenuButton = ContainsCursor(_contextMenuButtonBounds, e.Location);

        RedrawCell();
    }

    protected override void OnMouseLeave(int rowIndex)
    {
        base.OnMouseLeave(rowIndex);

        _isCursorHoveringPlayPauseButton = false;
        _isCursorHoveringStopButton = false;
        _isCursorHoveringMenuButton = false;
        _isLeftMouseButtonPressed = false; //better ux
        RedrawCell();
    }

    private bool ContainsCursor(Rectangle elementRectangle, Point cursorLocation)
    {
        elementRectangle.Offset(-_cellBounds.Location.X, -_cellBounds.Location.Y);
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
        _isLeftMouseButtonPressed = e.Button == MouseButtons.Left;
    }
    protected override void OnMouseUp(DataGridViewCellMouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
            _isLeftMouseButtonPressed = false;
    }

    protected override void OnMouseClick(DataGridViewCellMouseEventArgs e)
    {
        base.OnMouseClick(e);
        if (e.Button == MouseButtons.Left)
        {
            if (ContainsCursor(_playPauseButtonBounds, e.Location) && !_isCellTooSmall)
            {
                TogglePlayPause();
            }
            else if (ContainsCursor(_stopButtonBounds, e.Location) && !_isCellTooSmall)
            {
                _audioPlayer?.Stop();
            }
            else if (ContainsCursor(_contextMenuButtonBounds, e.Location) && !_isCellTooSmall)
            {
                ShowContextMenu(e.Location);
            }
            else if (ContainsCursor(_trackBarBounds, e.Location))
            {
                Seek(e.Location);
            }
        }
        else if (e.Button == MouseButtons.Right)
        {
            // Allow play/pause even when no ui buttons are rendered
            TogglePlayPause();
        }
    }

    private bool _sentQuickPeekEvent = false;
    private void TogglePlayPause()
    {
        if (_audioPlayer is null)
            return;

        if (_isPlaying)
        {
            _audioPlayer.Pause();
            _isPlaying = false;
            _updateTimer.Stop();
        }
        else
        {
            _audioPlayer.Play();
            _isPlaying = true;
            _updateTimer.Start();

            // Lets throttle events just in case. Not sure if it's necessary
            if (!_sentQuickPeekEvent)
            {
                QuickPeekEvent.FireAndForget(QuickPeekEvent.DataTypeId.Audio);
                _sentQuickPeekEvent = true;
            }
        }

        RedrawCell();
    }

    private void Seek(Point location)
    {
        if (_audioPlayer is null)
            return;

        var trackbarWidth = _trackBarBounds.Right - _trackBarBounds.Left;
        var cellboundsLeft = _trackBarBounds.Left - _cellBounds.X;
        var clickLocation = location.X - cellboundsLeft;

        var seekPercentage = (double)clickLocation / trackbarWidth;
        var seekLocation = TimeSpan.FromSeconds(_audioPlayer.TotalTime.TotalSeconds * seekPercentage);
        _audioPlayer.CurrentTime = seekLocation;

        DataGridView?.InvalidateCell(this);
    }

    private void ShowContextMenu(Point location)
    {
        if (DataGridView is null) //just in case
            return;

        if (Value is not IByteArrayValue byteArrayValue)
            return;

        if (_audioPlayer is null || _audioPlayer.AudioFormat == AudioPlayer.AudioFormatType.Invalid)
            return;

        var menu = new ContextMenuStrip();
        menu.Items.Add($"Save as {_audioPlayer.AudioFormat}", Resources.Icons.save_icon, async (s, a) =>
        {
            using var saveFileDialog = new SaveFileDialog
            {
                Filter = $"{_audioPlayer.AudioFormat.ToString().ToUpperInvariant()} file|*.{_audioPlayer.AudioFormat.ToString().ToLowerInvariant()}",
                Title = $"Save audio as {_audioPlayer.AudioFormat.ToString().ToUpperInvariant()}"
            };
            saveFileDialog.ShowDialog();

            if (string.IsNullOrWhiteSpace(saveFileDialog.FileName))
                return;

            CleanupFile(saveFileDialog.FileName); //Delete any existing file (user already confirmed any overwrite)

            if (Value is not IByteArrayValue byteArray)
                throw new InvalidDataException("Audio data was not found");

            await File.WriteAllBytesAsync(saveFileDialog.FileName, byteArray.Data);

            MessageBox.Show($"Audio saved to {saveFileDialog.FileName}", "Save complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
        });

        //Show additional metadata about the audio in the context menu
        menu.Items.Add(new ToolStripSeparator());

        var isFirst = true;
        foreach (var text in _audioPlayer.WaveFormat.ToString().Split(":"))
        {
            var waveFormatItem = new ToolStripButton((isFirst ? "Format: " : string.Empty) + text.Trim())
            {
                Enabled = false,
                AutoToolTip = false,
            };
            menu.Items.Add(waveFormatItem);
            isFirst = false;
        }

        //No way to dispose the context menu properly so we dispose it on Close instead.            
        menu.Closed += (_, _) => menu.BeginInvoke(menu.Dispose);

        menu.Show(DataGridView, location + (Size)_cellBounds.Location);
        menu.PerformLayout();

        static void CleanupFile(string filePath)
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
            _audioPlayer.DisposeSafely();
            _updateTimer.DisposeSafely();
            _initializationTimer.DisposeSafely();
        }

        base.Dispose(disposing);
    }
}
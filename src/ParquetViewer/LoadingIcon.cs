using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace ParquetViewer;

public class LoadingIcon : IDisposable, IProgress<int>
{
    private const int LoadingPanelWidth = 200;
    private const int LoadingPanelHeight = 200;

    private readonly Form _form;
    private readonly Panel _panel;
    private readonly Button _cancelButton;
    private readonly long _loadingBarMax = 0;
    private readonly CancellationTokenSource _cancellationToken = new();
    private long _progressSoFar = 0;
    private int _progressRatio = 0;

    public CancellationToken CancellationToken => _cancellationToken.Token;

    public event EventHandler? OnShow;
    public event EventHandler? OnHide;

    public LoadingIcon(Form form, string message, long loadingBarMax = 0)
    {
        ArgumentNullException.ThrowIfNull(form);

        _form = form;
        _panel = new Panel();
        _panel.BorderStyle = BorderStyle.FixedSingle;
        _panel.Size = new Size(LoadingPanelWidth, LoadingPanelHeight);
        _panel.Location = GetFormCenter();
        _loadingBarMax = loadingBarMax;

        _panel.Controls.Add(new Label()
        {
            Name = "loadingmessagelabel",
            Text = message,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Top,
            Font = new Font("Segoe UI Semibold", 9F, FontStyle.Regular)
        });

        var pictureBox = new PictureBox()
        {
            Name = "loadingpicturebox",
            Image = Resources.Icons.hourglass,
            Size = new Size(200, 200)
        };
        _panel.Controls.Add(pictureBox);

        _cancelButton = new Button()
        {
            Name = "cancelloadingbutton",
            Text = Resources.Strings.CancelButtonText,
            Dock = DockStyle.Bottom,
            Enabled = _cancellationToken.Token.CanBeCanceled,
            BackColor = Color.White,
            ForeColor = Color.Black
        };
        _cancelButton.Click += (object? buttonSender, EventArgs buttonClickEventArgs) =>
        {
            _cancellationToken.Cancel();

            if (buttonSender is Button button)
            {
                button.Enabled = false;
                button.Text = Resources.Strings.CancelInitiatedLabelText;
            }
        };
        _panel.Controls.Add(_cancelButton);
        _cancelButton.BringToFront();

        //Center on form resize
        _form.SizeChanged += (object? sender, EventArgs e) =>
        {
            _panel.Location = GetFormCenter();
        };
    }

    public void Reset(string? newMessage = null)
    {
        if (newMessage is not null)
        {
            foreach (Control control in _panel.Controls.Find("loadingmessagelabel", false))
            {
                control.Text = newMessage;
            }
        }

        _progressSoFar = 0;
        _progressRatio = 0;
        _cancelButton.BackgroundImage = null;
        _cancelButton.Invoke(_cancelButton.Refresh);
    }

    public void Show()
    {
        _form.Controls.Add(_panel);
        _panel.BringToFront();
        _panel.Show();
        _cancelButton.Focus();

        _cancelButton.BackgroundImage = new Bitmap(_cancelButton.ClientSize.Width, _cancelButton.ClientSize.Height);
        OnShow?.Invoke(this, EventArgs.Empty);
    }

    private Point GetFormCenter()
        => new((_form.Width / 2) - (LoadingPanelWidth / 2), (_form.Height / 2) - (LoadingPanelHeight / 2));

    public void Dispose()
    {
        OnHide?.Invoke(this, EventArgs.Empty);
        _panel.Dispose();
    }

    private readonly object _lock = new();
    public void Report(int progress)
    {
        if (_loadingBarMax <= 0)
            return;

        var progressSoFar = Interlocked.Add(ref _progressSoFar, progress);
        var progressRatio = (int)Math.Ceiling((progressSoFar * 100) / (double)_loadingBarMax);
        if (progressRatio != _progressRatio)
        {
            _progressRatio = progressRatio;

            lock (_lock) //This part isn't thread-safe
            {
                //Convert the cancel button into a progress bar
                var bitmap = new Bitmap(_cancelButton.ClientSize.Width, _cancelButton.ClientSize.Height);
                using (var solidBrush = new SolidBrush(Color.FromArgb(160, 40, 160, 60)))
                {
                    using (Graphics graphics = Graphics.FromImage(bitmap))
                    {
                        float wid = bitmap.Width * _progressRatio / 100;
                        float hgt = bitmap.Height;
                        RectangleF rect = new RectangleF(0, 0, wid, hgt);
                        graphics.FillRectangle(solidBrush, rect);
                    }
                }
                _cancelButton.BackgroundImage = bitmap;
                _cancelButton.Invoke(_cancelButton.Refresh);
            }
        }
    }
}
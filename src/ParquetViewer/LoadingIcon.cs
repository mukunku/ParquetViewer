using ParquetViewer.Controls;
using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace ParquetViewer;

public sealed class LoadingIcon : IDisposable, IProgress<int>
{
    private const int LOADING_PANEL_WIDTH = 200;
    private const int LOADING_PANEL_HEIGHT = 200;

    private readonly Form _form;
    private readonly Panel _panel;
    private readonly ProgressButton _cancelButton;
    private readonly double _loadingBarMaxRatio = 0;
    private readonly CancellationTokenSource _cancellationToken = new();
    private long _progressSoFar = 0;

    public CancellationToken CancellationToken => _cancellationToken.Token;

    public event EventHandler? OnShow;
    public event EventHandler? OnHide;

    public LoadingIcon(Form form, string message, long loadingBarMax = 0)
    {
        ArgumentNullException.ThrowIfNull(form);

        _form = form;
        _panel = new Panel
        {
            BorderStyle = BorderStyle.FixedSingle,
            Size = new Size(LOADING_PANEL_WIDTH, LOADING_PANEL_HEIGHT),
            Location = GetFormCenter()
        };
        _loadingBarMaxRatio = 100d / loadingBarMax;

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

        _cancelButton = new ProgressButton()
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

            _cancelButton.Enabled = false;
            _cancelButton.Text = Resources.Strings.CancelInitiatedLabelText;
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
        _cancelButton.Percent = 0;
        _cancelButton.Invoke(_cancelButton.Refresh);
    }

    public void Show()
    {
        _form.Controls.Add(_panel);
        _panel.BringToFront();
        _panel.Show();
        _cancelButton.Focus();
        OnShow?.Invoke(this, EventArgs.Empty);
    }

    private Point GetFormCenter()
        => new((_form.Width / 2) - (LOADING_PANEL_WIDTH / 2), (_form.Height / 2) - (LOADING_PANEL_HEIGHT / 2));

    public void Dispose()
    {
        OnHide?.Invoke(this, EventArgs.Empty);
        _panel.Dispose();
    }

    public void Report(int progress)
    {
        if (_loadingBarMaxRatio <= 0)
            return;

        //We always have one background thread loading data so this is thread-safe
        _progressSoFar += progress;

        _cancelButton.Percent = (int)Math.Ceiling(_progressSoFar * _loadingBarMaxRatio);
    }
}
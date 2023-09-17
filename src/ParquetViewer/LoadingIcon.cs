using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace ParquetViewer
{
    public class LoadingIcon : IDisposable, IProgress<int>
    {
        private const int LoadingPanelWidth = 200;
        private const int LoadingPanelHeight = 200;

        private Form _form;
        private Panel _panel;
        private Button _cancelButton;
        private long _loadingBarMax = 0;
        private long _progressSoFar = 0;
        private CancellationTokenSource _cancellationToken = new();
        private int _progressRatio = 0;

        public CancellationToken CancellationToken => this._cancellationToken.Token;

        public event EventHandler OnShow;
        public event EventHandler OnHide;

        public LoadingIcon(Form form, string message, long loadingBarMax = 0)
        {
            if (form is null)
                throw new ArgumentNullException(nameof(form));

            this._form = form;
            this._panel = new Panel();
            this._panel.Size = new Size(LoadingPanelWidth, LoadingPanelHeight);
            this._panel.Location = this.GetFormCenter();
            this._loadingBarMax = loadingBarMax;

            this._panel.Controls.Add(new Label()
            {
                Name = "loadingmessagelabel",
                Text = message,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top
            });

            var pictureBox = new PictureBox()
            {
                Name = "loadingpicturebox",
                Image = Properties.Resources.hourglass,
                Size = new Size(200, 200)
            };
            this._panel.Controls.Add(pictureBox);

            this._cancelButton = new Button()
            {
                Name = "cancelloadingbutton",
                Text = "Cancel",
                Dock = DockStyle.Bottom,
                Enabled = this._cancellationToken.Token.CanBeCanceled
            };
            this._cancelButton.Click += (object buttonSender, EventArgs buttonClickEventArgs) =>
            {
                this._cancellationToken.Cancel();

                ((Button)buttonSender).Enabled = false;
                ((Button)buttonSender).Text = "Cancelling...";
            };
            this._panel.Controls.Add(this._cancelButton);
            this._cancelButton.BringToFront();

            //Center on form resize
            this._form.SizeChanged += (object sender, EventArgs e) =>
            {
                this._panel.Location = this.GetFormCenter();
            };
        }

        public void Show()
        {
            this._form.Controls.Add(this._panel);
            this._panel.BringToFront();
            this._panel.Show();
            this._cancelButton.Focus();

            this._cancelButton.BackgroundImage = new Bitmap(_cancelButton.ClientSize.Width, _cancelButton.ClientSize.Height);
            this.OnShow?.Invoke(this, EventArgs.Empty);
        }

        private Point GetFormCenter()
            => new((this._form.Width / 2) - (LoadingPanelWidth / 2), (this._form.Height / 2) - (LoadingPanelHeight / 2));

        public void Dispose()
        {
            this.OnHide?.Invoke(this, EventArgs.Empty);
            this._panel.Dispose();
        }

        private object _lock = new object();
        public void Report(int _)
        {
            if (this._loadingBarMax <= 0)
                return;

            var progressSoFar = Interlocked.Increment(ref this._progressSoFar);
            var progressRatio = (int)Math.Ceiling((progressSoFar * 100) / (double)this._loadingBarMax);
            if (progressRatio != this._progressRatio)
            {
                this._progressRatio = progressRatio;

                lock (_lock) //This part isn't thread-safe
                {
                    //Convert the cancel button into a progress bar
                    var bitmap = new Bitmap(_cancelButton.ClientSize.Width, _cancelButton.ClientSize.Height);
                    using (var solidBrush = new SolidBrush(Color.FromArgb(160, 40, 160, 60)))
                    {
                        using (Graphics graphics = Graphics.FromImage(bitmap))
                        {
                            float wid = bitmap.Width * this._progressRatio / 100;
                            float hgt = bitmap.Height;
                            RectangleF rect = new RectangleF(0, 0, wid, hgt);
                            graphics.FillRectangle(solidBrush, rect);
                        }
                    }
                    this._cancelButton.BackgroundImage = bitmap;
                    this._cancelButton.Invoke(this._cancelButton.Refresh);
                }
            }
        }
    }
}

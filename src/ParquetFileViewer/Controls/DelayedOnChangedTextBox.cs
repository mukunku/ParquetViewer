using System;
using System.Windows.Forms;

namespace ParquetFileViewer.Controls
{
    public class DelayedOnChangedTextBox : TextBox
    {
        private bool _skipNextTextChange = false;
        private Timer _delayedTextChangedTimer;

        public event EventHandler DelayedTextChanged;

        public DelayedOnChangedTextBox()
            : base()
        {
            this.DelayedTextChangedTimeout = 1 * 1000;
        }

        public DelayedOnChangedTextBox(int secondsDelay)
            : base()
        {
            this.DelayedTextChangedTimeout = secondsDelay * 1000;
        }

        protected override void Dispose(bool disposing)
        {
            if (_delayedTextChangedTimer != null)
            {
                _delayedTextChangedTimer.Stop();
                if (disposing)
                    _delayedTextChangedTimer.Dispose();
            }

            base.Dispose(disposing);
        }

        public int DelayedTextChangedTimeout { get; set; }

        protected virtual void OnDelayedTextChanged(EventArgs e)
        {
            this.DelayedTextChanged?.Invoke(this, e);
        }

        protected override void OnTextChanged(EventArgs e)
        {
            if (this._skipNextTextChange)
            {
                _skipNextTextChange = false; 
            }
            else
            {
                this.InitializeDelayedTextChangedEvent();
            }

            base.OnTextChanged(e);
        }

        /// <summary>
        /// Sets the Text value of the textbox without triggering the text changed event
        /// </summary>
        /// <param name="text">New value to set as the textbox's text</param>
        public void SetTextQuiet(string text)
        {
            if (!this.Text.Equals(text)) //don't change value if it's the same because OnTextChanged won't get triggered
            {
                this._skipNextTextChange = true;
                this.Text = text;
            }
        }

        private void InitializeDelayedTextChangedEvent()
        {
            if (_delayedTextChangedTimer != null)
                _delayedTextChangedTimer.Stop();

            if (_delayedTextChangedTimer == null || _delayedTextChangedTimer.Interval != this.DelayedTextChangedTimeout)
            {
                _delayedTextChangedTimer = new Timer();
                _delayedTextChangedTimer.Tick += new EventHandler(HandleDelayedTextChangedTimerTick);
                _delayedTextChangedTimer.Interval = this.DelayedTextChangedTimeout;
            }

            _delayedTextChangedTimer.Start();
        }

        private void HandleDelayedTextChangedTimerTick(object sender, EventArgs e)
        {
            Timer timer = sender as Timer;
            timer.Stop();

            this.OnDelayedTextChanged(EventArgs.Empty);
        }
    }
}

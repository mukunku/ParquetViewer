using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace ParquetViewer.Controls
{
    public class DelayedOnChangedTextBox : TextBox
    {
        private bool _skipNextTextChange = false;
        private Timer? _delayedTextChangedTimer;

        public event EventHandler? DelayedTextChanged;

        public DelayedOnChangedTextBox()
            : base()
        {
            DelayedTextChangedTimeout = 1 * 1000;
        }

        public DelayedOnChangedTextBox(int secondsDelay)
            : base()
        {
            DelayedTextChangedTimeout = secondsDelay * 1000;
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

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int DelayedTextChangedTimeout { get; set; }

        protected virtual void OnDelayedTextChanged(EventArgs e)
        {
            DelayedTextChanged?.Invoke(this, e);
        }

        protected override void OnTextChanged(EventArgs e)
        {
            if (_skipNextTextChange)
            {
                _skipNextTextChange = false;
            }
            else
            {
                InitializeDelayedTextChangedEvent();
            }

            base.OnTextChanged(e);
        }

        /// <summary>
        /// Sets the Text value of the textbox without triggering the text changed event
        /// </summary>
        /// <param name="text">New value to set as the textbox's text</param>
        public void SetTextQuiet(string text)
        {
            if (!Text.Equals(text)) //don't change value if it's the same because OnTextChanged won't get triggered
            {
                _skipNextTextChange = true;
                Text = text;
            }
        }

        private void InitializeDelayedTextChangedEvent()
        {
            if (_delayedTextChangedTimer != null)
                _delayedTextChangedTimer.Stop();

            if (_delayedTextChangedTimer == null || _delayedTextChangedTimer.Interval != DelayedTextChangedTimeout)
            {
                _delayedTextChangedTimer = new Timer();
                _delayedTextChangedTimer.Tick += new EventHandler(HandleDelayedTextChangedTimerTick);
                _delayedTextChangedTimer.Interval = DelayedTextChangedTimeout;
            }

            _delayedTextChangedTimer.Start();
        }

        private void HandleDelayedTextChangedTimerTick(object? sender, EventArgs e)
        {
            var timer = sender as Timer;
            timer?.Stop();

            OnDelayedTextChanged(EventArgs.Empty);
        }
    }
}

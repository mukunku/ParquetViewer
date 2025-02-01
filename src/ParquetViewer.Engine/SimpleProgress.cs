namespace ParquetViewer.Engine
{
    public class SimpleProgress : IProgress<int>
    {
        private int _progress = 0;
        public Action<int>? ProgressChanged;

        public void Report(int value)
        {
            _progress += value;
            ProgressChanged?.Invoke(_progress);
        }
    }
}

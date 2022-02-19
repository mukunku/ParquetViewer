using System;
using System.IO;
using System.Windows.Forms;

namespace ParquetFileViewer
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            string fileToOpen = null;
            try
            {
                if (args?.Length > 0 && File.Exists(args[0]))
                {
                    fileToOpen = args[0];
                }
            }
            catch (Exception) { /*Swallow Exception*/ }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //Form must be created after calling SetCompatibleTextRenderingDefault();
            Form mainForm;
            if (string.IsNullOrWhiteSpace(fileToOpen))
                mainForm = new MainForm();
            else
                mainForm = new MainForm(fileToOpen);

            Application.Run(mainForm);
        }
    }
}

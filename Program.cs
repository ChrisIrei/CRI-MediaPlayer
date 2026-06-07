using System;
using System.Windows.Forms;

namespace MediaPlayerApp
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                System.IO.File.WriteAllText("crash.log", ex.ToString());
                MessageBox.Show(ex.Message, "Fatal Error");
            }
        }
    }
}

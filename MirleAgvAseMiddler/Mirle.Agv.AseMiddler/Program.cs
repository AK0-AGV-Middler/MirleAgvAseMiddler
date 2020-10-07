using Mirle.Agv.AseMiddler.View;
using System;
using System.Threading;
using System.Windows.Forms;


namespace Mirle.Agv.AseMiddler
{
    static class Program
    {
        static string appGuid = "Mirle_Agv_AseMiddler_Mutex_Locker";

        /// <summary>
        /// 應用程式的主要進入點.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using (Mutex mutex = new Mutex(false, "Global\\" + appGuid))
            {
                if (!mutex.WaitOne(0, false))
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new SingleExecuteWarnForm());
                    return;
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new InitialForm());
            }
        }
    }
}

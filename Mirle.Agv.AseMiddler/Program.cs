using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Mirle.Agv.AseMiddler.View;

namespace Mirle.Agv.AseMiddler
{
    static class Program
    {
        /// <summary>
        /// 應用程式的主要進入點。
        /// </summary>
        [STAThread]
        static void Main()
        {
            

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());

            InitialForm initialForm = new InitialForm();
            Application.Run(initialForm);
        }
    }
}

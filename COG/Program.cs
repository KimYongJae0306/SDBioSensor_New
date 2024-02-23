using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace COG
{
    static class Program
    {
        /// <summary>
        /// 해당 응용 프로그램의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main()
        {
            int nCount = 0;
            string _Assemblyname = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name; //속성 -> 응용프로그램 ->어셈블리이름

            foreach (Process proc in Process.GetProcesses())
            {
                //if (proc.ProcessName == _Assemblyname)
                //{
                //    nCount++;
                //    if (nCount > 1)
                //    {
                //        return;
                //    }
                //}
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);

            var mainForm = new MainForm();
            SystemManager.Instance().SetMainForm(mainForm);
            Application.Run(mainForm);
        }
        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.ToString());
        }
    }
}

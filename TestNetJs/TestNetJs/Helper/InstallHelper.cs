using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TestNetJs.Helper
{
    public class InstallHelper
    {
        static Process myProcess;
        public static void OpenInstallPackage(string filename)
        {
            try
            {
                myProcess = Process.Start(string.Format("{0}/{1}.exe", AppDomain.CurrentDomain.BaseDirectory, filename)); ;
                myProcess.WaitForExit();
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        
        }
        public static void OpenSoft(string filename)
        {
            try
            {
                Process p = new Process();
                p.StartInfo.FileName = @"C:\Program Files (x86)\Tencent\QQ\Bin\qq.exe";
                p.StartInfo.Arguments = "";
                p.StartInfo.WorkingDirectory = "";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.ErrorDialog = false;
                p.StartInfo.CreateNoWindow = true;
                p.Start();
                p.Close();
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }

        }
    }
}

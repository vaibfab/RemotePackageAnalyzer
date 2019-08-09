using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
namespace ServerProcess
{
    public class ProcessMain
    {
        public bool SpawnProc(List<string> filepath)
        {
            ProcessStartInfo oStartInfo = new ProcessStartInfo();
            oStartInfo.CreateNoWindow = false;           
            oStartInfo.FileName = Path.GetFullPath("../../../DemoExecutive/bin/Debug/DemoExecutive.exe");
            string[] array = filepath.ToArray();
            StringBuilder commandArgs = new StringBuilder();
            foreach (string filename in filepath)
                commandArgs.Append(filename).Append(',');
            string arg=commandArgs.ToString().Remove(commandArgs.ToString().LastIndexOf(','));
            oStartInfo.Arguments =arg;
            try
            {
                using (Process oProc = Process.Start(oStartInfo))
                {
                    oProc.WaitForExit();
                    oProc.Close();
                }
            }
            catch
            {

            }
            return false;
        }

    }
}

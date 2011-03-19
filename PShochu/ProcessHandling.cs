using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace PShochu
{
    public class ProcessHandling
    {
        public static void InvokeScript(string moduleLocation, string scriptPath, Action<string> onConsoleOut, Action<string> onErrorOut)
        {
            var psakeModulePath = Path.Combine(new DirectoryInfo(moduleLocation).FullName, "psake.psm1");
            var psakeScriptPath = new FileInfo(scriptPath).FullName;

            ProcessStartInfo psi = new ProcessStartInfo();

            psi.FileName = "powershell";
            psi.UseShellExecute = false;
            psi.RedirectStandardError = true;
            psi.RedirectStandardOutput = true;

            StringBuilder arguments = new StringBuilder();
            arguments.Append(String.Format(@"import-module ""{0}"";", psakeModulePath));
            arguments.Append(String.Format(@"invoke-psake ""{0}"";", psakeScriptPath));

            psi.Arguments = arguments.ToString();

            using(var process = Process.Start(psi))
            {
                try
                {
                    process.OutputDataReceived += delegate(object sendingProcess, DataReceivedEventArgs errorEvent)
                    {
                        onConsoleOut(errorEvent.Data);
                    };

                    process.ErrorDataReceived += delegate(object sendingProcess, DataReceivedEventArgs errorEvent)
                    {
                        onErrorOut(errorEvent.Data);
                    };

                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    process.WaitForExit();
                }
                catch(Exception e)
                {
                    process.Kill();
                }

                process.WaitForExit();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using PShochu.Util;

namespace PShochu
{
    public class ProcessHandling
    {
        public static int InvokeScript(string moduleLocation, string scriptPath, Action<string> onConsoleOut, Action<string> onErrorOut)
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

                return process.ExitCode;
            }
        }

        public static InvokeResult InvokeScript(string moduleLocation, string scriptPath)
        {
            InvokeResult result = null;
            int? exitCode = null;

            var consoleStream = new MemoryStream();
            var errorStream = new MemoryStream();

            try
            {
                using(var consoleWriter = new NonclosingStreamWriter(consoleStream))
                using(var errorWriter = new NonclosingStreamWriter(errorStream))
                {
                    exitCode = InvokeScript(moduleLocation, scriptPath, consoleWriter.WriteLine,errorWriter.WriteLine);

                    consoleWriter.Flush();
                    errorWriter.Flush();

                    consoleStream.Seek(0, SeekOrigin.Begin);
                    errorStream.Seek(0, SeekOrigin.Begin);

                    var consoleOutput =
                        new NonclosingStreamReader(consoleStream).ReadToEnd().Split(new[] { consoleWriter.NewLine },
                            StringSplitOptions.None);

                    var errorOutput =
                        new NonclosingStreamReader(errorStream).ReadToEnd().Split(new[] { consoleWriter.NewLine },
                            StringSplitOptions.None);

                    result = new InvokeResult(consoleStream, errorStream, consoleOutput, errorOutput, exitCode);
                    consoleStream = null;
                    errorStream = null;
                }
            }
            finally
            {
                if (consoleStream != null)
                    consoleStream.Dispose();
                
                if (errorStream != null)
                    errorStream.Dispose();
            }

            return result;
        }
    }
}

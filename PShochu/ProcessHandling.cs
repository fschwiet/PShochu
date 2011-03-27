using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using PShochu.PInvoke;
using PShochu.Util;

namespace PShochu
{
    public class ProcessHandling
    {
        public static ConsoleApplicationResult RunNoninteractiveConsoleProcess(string command, string commandArguments)
        {
            ConsoleApplicationResult result = null;
            int? exitCode = null;

            var consoleStream = new MemoryStream();
            var errorStream = new MemoryStream();

            try
            {
                using(var consoleWriter = new NonclosingStreamWriter(consoleStream))
                using(var errorWriter = new NonclosingStreamWriter(errorStream))
                {
                    exitCode = RunNoninteractiveConsoleProcess(command, commandArguments, consoleWriter.WriteLine, errorWriter.WriteLine);

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

                    result = new ConsoleApplicationResult(consoleStream, errorStream, consoleOutput, errorOutput, exitCode);
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

        public static int RunNoninteractiveConsoleProcess(string command, string commandArguments, Action<string> onConsoleOut, Action<string> onErrorOut)
        {
            ProcessStartInfo psi = new ProcessStartInfo();

            psi.FileName = command;
            psi.UseShellExecute = false;
            psi.RedirectStandardError = true;
            psi.RedirectStandardOutput = true;
            psi.CreateNoWindow = true;

            psi.Arguments = commandArguments;

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
    }
}

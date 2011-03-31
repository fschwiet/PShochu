using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using PShochu.PInvoke;
using PShochu.PInvoke.NetWrappers;
using PShochu.Util;

namespace PShochu
{
    public class ProcessHandling
    {
        public static ConsoleApplicationResult RunNoninteractiveConsoleProcess(string commandLine)
        {
            string newLine;
            var consoleStreamsResult = RunNoninteractiveConsoleProcessForStreams(commandLine, out newLine);

            return ConsoleApplicationResult.LoadConsoleOutput(consoleStreamsResult, newLine);
        }

        public static ConsoleApplicationResultStreams RunNoninteractiveConsoleProcessForStreams(string commandArguments, out string newLine)
        {
            StreamReader consoleReader = null;
            StreamReader errorReader = null;

            try
            {
                using (var threadToken = AccessToken.GetCurrentAccessTokenDuplicatedAsPrimary())
                {
                    var process = ProcessUtil.CreateProcessWithToken(threadToken.DangerousGetHandle(), null, 
                        commandArguments, out consoleReader, out errorReader);

                    process.WaitForExit();

                    newLine = Environment.NewLine;

                    var result = new ConsoleApplicationResultStreams(consoleReader, errorReader, process.ExitCode);

                    consoleReader = null;
                    errorReader = null;

                    return result;
                }
            }
            finally
            {
                if (consoleReader != null)
                    consoleReader.Dispose();

                if (errorReader != null)
                    errorReader.Dispose();
            }            
        }

        public static ConsoleApplicationResultStreams RunNoninteractiveConsoleProcessForStreamsWithManagedCode(string command, string commandArguments, out string newLine)
        {
            var consoleStream = new MemoryStream();
            var errorStream = new MemoryStream();

            try
            {
                using (var consoleWriter = new NonclosingStreamWriter(consoleStream))
                using (var errorWriter = new NonclosingStreamWriter(errorStream))
                {
                    ProcessStartInfo psi = new ProcessStartInfo();

                    psi.FileName = command;
                    psi.UseShellExecute = false;
                    psi.RedirectStandardError = true;
                    psi.RedirectStandardOutput = true;
                    psi.CreateNoWindow = true;

                    psi.Arguments = commandArguments;

                    using (var process = Process.Start(psi))
                    {
                        try
                        {
                            process.OutputDataReceived += delegate(object sendingProcess, DataReceivedEventArgs errorEvent)
                            {
                                consoleWriter.WriteLine(errorEvent.Data);
                            };

                            process.ErrorDataReceived += delegate(object sendingProcess, DataReceivedEventArgs errorEvent)
                            {
                                errorWriter.WriteLine(errorEvent.Data);
                            };

                            process.BeginOutputReadLine();
                            process.BeginErrorReadLine();

                            process.WaitForExit();
                        }
                        catch (Exception e)
                        {
                            process.Kill();
                        }

                        process.WaitForExit();

                        consoleWriter.Flush();
                        errorWriter.Flush();

                        consoleStream.Seek(0, SeekOrigin.Begin);
                        errorStream.Seek(0, SeekOrigin.Begin);

                        var result = new ConsoleApplicationResultStreams(new StreamReader(consoleStream), new StreamReader(errorStream), process.ExitCode);
                        consoleStream = null;
                        errorStream = null;

                        newLine = consoleWriter.NewLine;

                        return result;
                    }
                }
            }
            finally
            {
                if (consoleStream != null)
                    consoleStream.Dispose();

                if (errorStream != null)
                    errorStream.Dispose();
            }

        }
    }
}

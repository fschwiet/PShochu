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
            using (var threadToken = AccessToken.GetCurrentAccessTokenDuplicatedAsPrimary())
            {
                StreamReader consoleOutput;
                StreamReader errorOutput;
                IntPtr processHandle;
                int dwProcessId;
                using (var startupInfo = StartupInfoWithOutputStreams.Create())
                {
                    AdvApi32PInvoke.PROCESS_INFORMATION lpProcessInformation = new AdvApi32PInvoke.PROCESS_INFORMATION();

                    AdvApi32PInvoke.LogonFlags logonFlags = AdvApi32PInvoke.LogonFlags.LOGON_WITH_PROFILE;
                    AdvApi32PInvoke.CreationFlags creationFlags = AdvApi32PInvoke.CreationFlags.CREATE_NEW_CONSOLE;

                    if (!AdvApi32PInvoke.CreateProcessWithTokenW(threadToken.DangerousGetHandle(), logonFlags, null,
                        commandArguments, (int)creationFlags, Constants.NULL, Constants.NULL, ref startupInfo.STARTUP_INFO, out lpProcessInformation))
                    {
                        int lastWin32Error = Marshal.GetLastWin32Error();

                        if (lastWin32Error == 0xc1)  // found in Process.StartWithCreateProcess
                            throw new Win32Exception("Invalid application");

                        throw new Win32Exception(lastWin32Error);
                    }

                    Kernel32.CloseHandle(lpProcessInformation.hThread);

                    processHandle = lpProcessInformation.hProcess;
                    lpProcessInformation.hProcess = Constants.NULL;

                    consoleOutput = startupInfo.ConsoleOutput;
                    errorOutput = startupInfo.ErrorOutput;
                    startupInfo.ConsoleOutput = null;
                    startupInfo.ErrorOutput = null;
                }

                consoleOutput.Peek();

                var waitResult = Kernel32.WaitForSingleObject(processHandle, Kernel32.INFINITE);

                if (waitResult == Kernel32.WAIT_FAILED)
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                if (waitResult == Kernel32.WAIT_TIMEOUT)
                    throw new TimeoutException("Timed out waiting for process.");

                if (waitResult != Kernel32.WAIT_OBJECT_0)
                    throw new InvalidOperationException("Unexpected wait result.");

                int exitCode;

                if (!Kernel32.GetExitCodeProcess(processHandle, out exitCode))
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                
                MemoryStream consoleStore = ReadStreamToMemoryStream(consoleOutput.BaseStream);
                MemoryStream errorStore = ReadStreamToMemoryStream(errorOutput.BaseStream);

                consoleStore.Seek(0, SeekOrigin.Begin);
                errorStore.Seek(0, SeekOrigin.Begin);

                newLine = Environment.NewLine;

                return new ConsoleApplicationResultStreams(new StreamReader(consoleStore), new StreamReader(errorStore), exitCode);
            }
        }

        private static MemoryStream ReadStreamToMemoryStream(Stream inputStream)
        {
            int bytesRead = 0;
            var buffer = new byte[4096];

            MemoryStream result = new MemoryStream();

            do
            {
                IAsyncResult asyncRead = inputStream.BeginRead(buffer, 0, buffer.Length, null, null);

                if (asyncRead.AsyncWaitHandle.WaitOne(250))
                {
                    bytesRead = inputStream.EndRead(asyncRead);
                    result.Write(buffer, 0, bytesRead);
                }
            } while (bytesRead == buffer.Length);
            return result;
        }
    }
}

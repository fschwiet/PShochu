using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Microsoft.Win32.SafeHandles;
using PShochu.Reflection;

namespace PShochu.PInvoke.NetWrappers
{
    public class ProcessUtil
    {
        static public Process CreateProcessWithToken(
            IntPtr userPrincipalToken, 
            string applicationName, 
            string applicationCommand,
            out StreamReader consoleOutput, 
            out StreamReader errorOutput)
        {
            AdvApi32PInvoke.PROCESS_INFORMATION lpProcessInformation = new AdvApi32PInvoke.PROCESS_INFORMATION();

            using (var startupInfo = StartupInfoWithOutputPipes.Create())
            {
                AdvApi32PInvoke.LogonFlags logonFlags = AdvApi32PInvoke.LogonFlags.LOGON_WITH_PROFILE;

                if (!AdvApi32PInvoke.CreateProcessWithTokenW(userPrincipalToken, logonFlags, applicationName,
                    applicationCommand, 0, Constants.NULL, Constants.NULL, ref startupInfo.STARTUP_INFO, out lpProcessInformation))
                {
                    int lastWin32Error = Marshal.GetLastWin32Error();

                    if (lastWin32Error == 0xc1)  // found in Process.StartWithCreateProcess
                        throw new Win32Exception("Invalid application");

                    throw new Win32Exception(lastWin32Error);
                }

                var setProcessHandleMethod = typeof(Process).GetMethod("SetProcessHandle", BindingFlags.Instance | BindingFlags.NonPublic);
                var setProcessIdMethod = typeof(Process).GetMethod("SetProcessInfo", BindingFlags.NonPublic);

                var result = new Process();
                setProcessHandleMethod.Invoke(result, new object[] {  SafeHandles.CreateSafeProcessHandle(lpProcessInformation.hProcess) });
                lpProcessInformation.hProcess = Constants.NULL;

                setProcessIdMethod.Invoke(result, new object[] { lpProcessInformation.dwProcessId });

                Kernel32.CloseHandle(lpProcessInformation.hThread);

                consoleOutput = new StreamReader(new FileStream(startupInfo.stdOutput, FileAccess.Read, 0x1000, false), Encoding.GetEncoding(Kernel32.GetConsoleOutputCP()), true, 0x1000);
                startupInfo.stdOutput = null;

                errorOutput = new StreamReader(new FileStream(startupInfo.stdError, FileAccess.Read, 0x1000, false), Encoding.GetEncoding(Kernel32.GetConsoleOutputCP()), true, 0x1000);
                startupInfo.stdError = null;

                return result;
            }
        }
    }
}

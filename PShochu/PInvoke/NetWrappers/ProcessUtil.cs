using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace PShochu.PInvoke.NetWrappers
{
    public class ProcessUtil
    {
        static public Process CreateProcessWithToken(
            IntPtr userPrincipalToken, string applicationName, string applicationCommand, bool dontCreateWindow, bool createWithProfile,
            out StreamReader consoleOutput, out StreamReader errorOutput)
        {
            AdvApi32PInvoke.PROCESS_INFORMATION lpProcessInformation = new AdvApi32PInvoke.PROCESS_INFORMATION();

            var startupInfo = StartupInfoWithOutputPipes.Create();

            int creationFlags = dontCreateWindow ? AdvApi32PInvoke.CREATE_NO_WINDOW : 0;

            AdvApi32PInvoke.LogonFlags logonFlags = createWithProfile
                                                        ? AdvApi32PInvoke.LogonFlags.LOGON_WITH_PROFILE
                                                        : 0;

            if (!AdvApi32PInvoke.CreateProcessWithTokenW(userPrincipalToken, logonFlags, applicationName,
                applicationCommand, creationFlags, Constants.NULL, Constants.NULL, ref startupInfo.STARTUP_INFO, out lpProcessInformation))
            {
                int lastWin32Error = Marshal.GetLastWin32Error();

                if (lastWin32Error == 0xc1)  // found in Process.StartWithCreateProcess
                    throw new Win32Exception("Invalid application");

                throw new Win32Exception(lastWin32Error);
            }

            var setProcessHandleMethod = typeof(Process).GetMethod("SetProcessHandle", BindingFlags.NonPublic);
            var setProcessIdMethod = typeof(Process).GetMethod("SetProcessInfo", BindingFlags.NonPublic);

            var result = new Process();
            setProcessHandleMethod.Invoke(result, new object[] { lpProcessInformation.hProcess});
            setProcessIdMethod.Invoke(result, new object[] { lpProcessInformation.dwProcessId});

            Kernel32.CloseHandle(lpProcessInformation.hThread);

            consoleOutput = new StreamReader(new FileStream(startupInfo.stdOutput, FileAccess.Read, 0x1000, false), Encoding.GetEncoding(Kernel32.GetConsoleOutputCP()), true, 0x1000);
            errorOutput = new StreamReader(new FileStream(startupInfo.stdError, FileAccess.Read, 0x1000, false), Encoding.GetEncoding(Kernel32.GetConsoleOutputCP()), true, 0x1000);

            return result;
        }
    }
}

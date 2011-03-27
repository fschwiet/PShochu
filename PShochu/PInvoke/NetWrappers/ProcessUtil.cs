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
            AdvApi32PInvoke.STARTUPINFO lpStartupInfo = new AdvApi32PInvoke.STARTUPINFO();
            AdvApi32PInvoke.PROCESS_INFORMATION lpProcessInformation = new AdvApi32PInvoke.PROCESS_INFORMATION();
            lpStartupInfo.hStdInput = Kernel32.GetStdHandle(Kernel32.STD_INPUT_HANDLE);

            SafeFileHandle ignored;

            SafeFileHandle stdOutput;
            SafeFileHandle stdError;

            Win32Pipe.CreatePipe(out ignored, out stdOutput, false);
            Win32Pipe.CreatePipe(out ignored, out stdError, false);

            lpStartupInfo.hStdInput = stdOutput.DangerousGetHandle();
            lpStartupInfo.hStdError = stdError.DangerousGetHandle();
            lpStartupInfo.dwFlags = AdvApi32PInvoke.STARTF_USESTDHANDLES;

            int creationFlags = dontCreateWindow ? AdvApi32PInvoke.CREATE_NO_WINDOW : 0;

            AdvApi32PInvoke.LogonFlags logonFlags = createWithProfile
                                                        ? AdvApi32PInvoke.LogonFlags.LOGON_WITH_PROFILE
                                                        : 0;

            if (!AdvApi32PInvoke.CreateProcessWithTokenW(userPrincipalToken, logonFlags, applicationName,
                applicationCommand, creationFlags, Constants.NULL, Constants.NULL, ref lpStartupInfo, out lpProcessInformation))
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

            consoleOutput = new StreamReader(new FileStream(stdOutput, FileAccess.Read, 0x1000, false), Encoding.GetEncoding(Kernel32.GetConsoleOutputCP()), true, 0x1000);
            errorOutput = new StreamReader(new FileStream(stdError, FileAccess.Read, 0x1000, false), Encoding.GetEncoding(Kernel32.GetConsoleOutputCP()), true, 0x1000);

            return result;
        }
    }
}

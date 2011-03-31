using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace PShochu.PInvoke.NetWrappers
{
    public class StartupInfoWithOutputPipes : IDisposable
    {
        public AdvApi32PInvoke.STARTUPINFO STARTUP_INFO;
        public SafeFileHandle stdOutput;
        public SafeFileHandle stdError;

        public static StartupInfoWithOutputPipes Create()
        {
            var result = new StartupInfoWithOutputPipes();

            result.STARTUP_INFO = new AdvApi32PInvoke.STARTUPINFO();
            result.STARTUP_INFO.cb = Marshal.SizeOf(typeof(AdvApi32PInvoke.STARTUPINFO));

            SafeFileHandle ignored;

            Win32Pipe.CreatePipe(out ignored, out result.stdOutput, false);
            Win32Pipe.CreatePipe(out ignored, out result.stdError, false);

            result.STARTUP_INFO.hStdInput = Kernel32.GetStdHandle(Kernel32.STD_INPUT_HANDLE);
            result.STARTUP_INFO.hStdOutput = result.stdOutput.DangerousGetHandle();
            result.STARTUP_INFO.hStdError = result.stdError.DangerousGetHandle();
            result.STARTUP_INFO.dwFlags = AdvApi32PInvoke.STARTF_USESTDHANDLES;

            return result;
        }

        public void Dispose()
        {
            if (stdOutput != null)
            {
                stdOutput.Dispose();
                stdOutput = null;
            }

            if (stdError != null)
            {
                stdError.Dispose();
                stdError = null;
            }
        }
    }
}

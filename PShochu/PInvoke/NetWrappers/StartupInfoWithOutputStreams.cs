using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace PShochu.PInvoke.NetWrappers
{
    public class StartupInfoWithOutputStreams : IDisposable
    {
        public AdvApi32PInvoke.STARTUPINFO STARTUP_INFO;
        public StreamReader ConsoleOutput;
        public StreamReader ErrorOutput;

        public static StartupInfoWithOutputStreams Create()
        {
            var result = new StartupInfoWithOutputStreams();

            result.STARTUP_INFO = new AdvApi32PInvoke.STARTUPINFO();
            result.STARTUP_INFO.cb = Marshal.SizeOf(typeof(AdvApi32PInvoke.STARTUPINFO));

            SafeFileHandle internalConsoleOutput;
            SafeFileHandle internalErrorOutput;
            SafeFileHandle consoleOutput;
            SafeFileHandle errorOutput;

            Win32Pipe.CreatePipe(out internalConsoleOutput, out consoleOutput, false);
            Win32Pipe.CreatePipe(out internalErrorOutput, out errorOutput, false);

            result.STARTUP_INFO.hStdInput = Kernel32.GetStdHandle(Kernel32.STD_INPUT_HANDLE);
            result.STARTUP_INFO.hStdOutput = consoleOutput.DangerousGetHandle();
            result.STARTUP_INFO.hStdError = errorOutput.DangerousGetHandle();
            result.STARTUP_INFO.dwFlags = AdvApi32PInvoke.STARTF_USESTDHANDLES;

            Encoding encoding = Encoding.GetEncoding(Kernel32.GetConsoleOutputCP());
            result.ConsoleOutput = new StreamReader(new FileStream(internalConsoleOutput, FileAccess.Read, 0x1000, false), encoding, true, 0x1000);
            result.ErrorOutput = new StreamReader(new FileStream(internalErrorOutput, FileAccess.Read, 0x1000, false), encoding, true, 0x1000);

            return result;
        }

        public void Dispose()
        {
            if (ConsoleOutput != null)
            {
                ConsoleOutput.Dispose();
                ConsoleOutput = null;
            }

            if (ErrorOutput != null)
            {
                ErrorOutput.Dispose();
                ErrorOutput = null;
            }
        }
    }
}

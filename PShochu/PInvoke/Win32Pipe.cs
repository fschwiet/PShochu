using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace PShochu.PInvoke
{
    public class Win32Pipe
    {
        public static void CreatePipe(out SafeFileHandle parentHandle, out SafeFileHandle childHandle, bool parentInputs)
        {
            AdvApi32PInvoke.SECURITY_ATTRIBUTES lpPipeAttributes = new AdvApi32PInvoke.SECURITY_ATTRIBUTES();
            lpPipeAttributes.bInheritHandle = true;
            SafeFileHandle hWritePipe = null;
            try
            {
                if (parentInputs)
                {
                    CreatePipeWithSecurityAttributes(out childHandle, out hWritePipe, lpPipeAttributes, 0);
                }
                else
                {
                    CreatePipeWithSecurityAttributes(out hWritePipe, out childHandle, lpPipeAttributes, 0);
                }

                if (!Kernel32.DuplicateHandle(Kernel32.GetCurrentProcess(), hWritePipe, Kernel32.GetCurrentProcess(), out parentHandle, 0, false, Kernel32.DUPLICATE_SAME_ACCESS))
                {
                    throw new Win32Exception();
                }
            }
            finally
            {
                if ((hWritePipe != null) && !hWritePipe.IsInvalid)
                {
                    hWritePipe.Close();
                }
            }
        }

        private static void CreatePipeWithSecurityAttributes(out SafeFileHandle hReadPipe, out SafeFileHandle hWritePipe, AdvApi32PInvoke.SECURITY_ATTRIBUTES lpPipeAttributes, uint nSize)
        {
            if ((!Kernel32.CreatePipe(out hReadPipe, out hWritePipe, ref lpPipeAttributes, nSize) || hReadPipe.IsInvalid) || hWritePipe.IsInvalid)
            {
                throw new Win32Exception();
            }
        }
    }
}

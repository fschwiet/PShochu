using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace PShochu.PInvoke.NetWrappers
{
    public class AccessToken
    {
        static public SafeHandle GetCurrentThreadAccessToken()
        {
            IntPtr originalToken = Constants.INVALID_HANDLE_VALUE;
            IntPtr duplicatedToken = Constants.INVALID_HANDLE_VALUE;

            IntPtr threadPSeudoHandle = Kernel32.GetCurrentThread(); 

            if (!AdvApi32PInvoke.OpenThreadToken(threadPSeudoHandle, AdvApi32PInvoke.TOKEN_QUERY | AdvApi32PInvoke.TOKEN_DUPLICATE | AdvApi32PInvoke.TOKEN_ASSIGN_PRIMARY, false, out originalToken))
            {
                var lastWin32Error = Marshal.GetLastWin32Error();

                if (lastWin32Error != ErrorCodes.ERROR_NO_TOKEN)
                {
                    throw new Win32Exception(lastWin32Error);
                }

                var processPseudohandle = Kernel32.GetCurrentProcess();

                if (!AdvApi32PInvoke.OpenProcessToken(processPseudohandle, AdvApi32PInvoke.TOKEN_QUERY | AdvApi32PInvoke.TOKEN_DUPLICATE | AdvApi32PInvoke.TOKEN_ASSIGN_PRIMARY, out originalToken))
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }

            try
            {
                if (!AdvApi32PInvoke.DuplicateTokenEx(originalToken, 
                    AdvApi32PInvoke.TOKEN_QUERY | AdvApi32PInvoke.TOKEN_DUPLICATE | AdvApi32PInvoke.TOKEN_ASSIGN_PRIMARY, 
                    Constants.NULL, AdvApi32PInvoke.SECURITY_IMPERSONATION_LEVEL.SecurityDelegation, AdvApi32PInvoke.TOKEN_TYPE.TokenPrimary,
                    out duplicatedToken))
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                var result = new SafeFileHandle(duplicatedToken, true);
                duplicatedToken = Constants.INVALID_HANDLE_VALUE;

                return result;
            }
            finally
            {
                Kernel32.CloseHandle(originalToken);
                Kernel32.CloseHandle(duplicatedToken);
            }
        }
    }
}

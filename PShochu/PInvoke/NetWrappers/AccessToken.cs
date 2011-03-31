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
        static public SafeHandle GetCurrentAccessTokenDuplicatedAsPrimary()
        {
            using (var originalToken = GetCurrentAccessToken())
            {
                return DuplicateTokenAsPrimaryToken(originalToken);
            }
        }

        public static SafeHandle GetCurrentAccessToken()
        {
            IntPtr originalToken = Constants.INVALID_HANDLE_VALUE;

            IntPtr threadPSeudoHandle = Kernel32.GetCurrentThread(); 

            if (!AdvApi32PInvoke.OpenThreadToken(threadPSeudoHandle,
                AdvApi32PInvoke.TOKEN_QUERY
                | AdvApi32PInvoke.TOKEN_DUPLICATE
                | AdvApi32PInvoke.TOKEN_ASSIGN_PRIMARY
                | AdvApi32PInvoke.TOKEN_ADJUST_DEFAULT
                | AdvApi32PInvoke.TOKEN_ADJUST_SESSIONID, false, out originalToken))
            {
                var lastWin32Error = Marshal.GetLastWin32Error();

                if (lastWin32Error != ErrorCodes.ERROR_NO_TOKEN)
                {
                    throw new Win32Exception(lastWin32Error);
                }

                var processPseudohandle = Kernel32.GetCurrentProcess();

                if (!AdvApi32PInvoke.OpenProcessToken(processPseudohandle, 
                    AdvApi32PInvoke.TOKEN_QUERY 
                    | AdvApi32PInvoke.TOKEN_DUPLICATE 
                    | AdvApi32PInvoke.TOKEN_ASSIGN_PRIMARY
                    | AdvApi32PInvoke.TOKEN_ADJUST_DEFAULT
                    | AdvApi32PInvoke.TOKEN_ADJUST_SESSIONID, out originalToken))
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }

            return new SafeFileHandle(originalToken, true);
        }

        public static SafeHandle LogonUser(string username, string password, string domain = ".")
        {
            IntPtr handle;

            if (!AdvApi32PInvoke.LogonUser(username,domain, password, 
                (int)AdvApi32PInvoke.LogonType.LOGON32_LOGON_BATCH,
                (int)AdvApi32PInvoke.LogonProvider.LOGON32_PROVIDER_DEFAULT,
                out handle))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            return new SafeFileHandle(handle, true);
        }

        public static SafeHandle DuplicateTokenAsPrimaryToken(SafeHandle originalToken)
        {
            IntPtr duplicatedToken = Constants.INVALID_HANDLE_VALUE;

            if (!AdvApi32PInvoke.DuplicateTokenEx(originalToken.DangerousGetHandle(), 
                AdvApi32PInvoke.TOKEN_QUERY | AdvApi32PInvoke.TOKEN_DUPLICATE | AdvApi32PInvoke.TOKEN_ASSIGN_PRIMARY, 
                Constants.NULL, 
                AdvApi32PInvoke.SECURITY_IMPERSONATION_LEVEL.SecurityImpersonation, 
                AdvApi32PInvoke.TOKEN_TYPE.TokenPrimary,
                out duplicatedToken))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            return new SafeFileHandle(duplicatedToken, true);
        }
    }
}

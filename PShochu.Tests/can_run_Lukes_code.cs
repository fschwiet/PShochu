using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using NJasmine;
using PShochu.PInvoke;
using PShochu.PInvoke.NetWrappers;

namespace PShochu.Tests
{
    public class can_run_Lukes_code : GivenWhenThenFixture
    {
        public override void Specify()
        {
            it("can run code like Luke's code", delegate
            {
                IntPtr hToken = AccessToken.GetCurrentAccessToken().DangerousGetHandle();

                //if(!AdvApi32PInvoke.OpenProcessToken(Kernel32.GetCurrentProcess(), AdvApi32PInvoke.TOKEN_DUPLICATE, out hToken))
                //{
                //    throw new Win32Exception(Marshal.GetLastWin32Error());
                //}

                IntPtr hDuplicate;
                if (AdvApi32PInvoke.DuplicateTokenEx(hToken,
                    AdvApi32PInvoke.TOKEN_ASSIGN_PRIMARY |
                    AdvApi32PInvoke.TOKEN_DUPLICATE |
                    AdvApi32PInvoke.TOKEN_QUERY |
                    AdvApi32PInvoke.TOKEN_ADJUST_DEFAULT |
                    AdvApi32PInvoke.TOKEN_ADJUST_SESSIONID,
                    Constants.NULL, AdvApi32PInvoke.SECURITY_IMPERSONATION_LEVEL.SecurityImpersonation,
                    AdvApi32PInvoke.TOKEN_TYPE.TokenPrimary, out hDuplicate))
                {
                    string commandLine = ("powershell");

                    AdvApi32PInvoke.STARTUPINFO startupInfo = new AdvApi32PInvoke.STARTUPINFO();
                    startupInfo.cb = Marshal.SizeOf(typeof(AdvApi32PInvoke.STARTUPINFO));

                    AdvApi32PInvoke.PROCESS_INFORMATION processInformation =
                        new AdvApi32PInvoke.PROCESS_INFORMATION();
                    if (
                        !AdvApi32PInvoke.CreateProcessWithTokenW(hDuplicate,
                            AdvApi32PInvoke.LogonFlags.LOGON_WITH_PROFILE, null, commandLine, 0, Constants.NULL,
                            Constants.NULL, ref startupInfo, out processInformation))
                    {
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                    }
                }
                else
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }); 
            
            it("can run Luke's code", delegate
            {
                IntPtr hToken;

                if (!AdvApi32PInvoke.OpenProcessToken(Kernel32.GetCurrentProcess(), AdvApi32PInvoke.TOKEN_DUPLICATE, out hToken))
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                IntPtr hDuplicate;
                if (AdvApi32PInvoke.DuplicateTokenEx(hToken,
                    AdvApi32PInvoke.TOKEN_ASSIGN_PRIMARY |
                    AdvApi32PInvoke.TOKEN_DUPLICATE |
                    AdvApi32PInvoke.TOKEN_QUERY |
                    AdvApi32PInvoke.TOKEN_ADJUST_DEFAULT |
                    AdvApi32PInvoke.TOKEN_ADJUST_SESSIONID,
                    Constants.NULL, AdvApi32PInvoke.SECURITY_IMPERSONATION_LEVEL.SecurityImpersonation,
                    AdvApi32PInvoke.TOKEN_TYPE.TokenPrimary, out hDuplicate))
                {
                    string commandLine = ("powershell");

                    AdvApi32PInvoke.STARTUPINFO startupInfo = new AdvApi32PInvoke.STARTUPINFO();
                    startupInfo.cb = Marshal.SizeOf(typeof(AdvApi32PInvoke.STARTUPINFO));

                    AdvApi32PInvoke.PROCESS_INFORMATION processInformation =
                        new AdvApi32PInvoke.PROCESS_INFORMATION();
                    if (
                        !AdvApi32PInvoke.CreateProcessWithTokenW(hDuplicate,
                            AdvApi32PInvoke.LogonFlags.LOGON_WITH_PROFILE, null, commandLine, 0, Constants.NULL,
                            Constants.NULL, ref startupInfo, out processInformation))
                    {
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                    }
                }
                else
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            });
        }
    }
}

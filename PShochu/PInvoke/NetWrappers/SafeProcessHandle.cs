using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace PShochu.PInvoke.NetWrappers
{
    [SuppressUnmanagedCodeSecurity, HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
    internal sealed class SafeProcessHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        // Fields
        internal static SafeProcessHandle InvalidHandle = new SafeProcessHandle(IntPtr.Zero);

        // Methods
        internal SafeProcessHandle()
            : base(true)
        {
        }

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        internal SafeProcessHandle(IntPtr handle)
            : base(true)
        {
            base.SetHandle(handle);
        }

        internal void InitialSetHandle(IntPtr h)
        {
            base.handle = h;
        }

        protected override bool ReleaseHandle()
        {
            return Kernel32.CloseHandle(base.handle);
        }
    }
}

using System;
using System.Reflection;

namespace PShochu.Reflection
{
    static class SafeHandles
    {
        public static object CreateSafeProcessHandle(IntPtr processHandle)
        {
            var systemAssembly = Assembly.LoadWithPartialName("System");
            var safeProcessHandleType = systemAssembly.GetType("Microsoft.Win32.SafeHandles.SafeProcessHandle");
            var constructor = safeProcessHandleType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance,(Binder)null, new Type[0],null);
            var setHandleMethod = safeProcessHandleType.GetMethod("InitialSetHandle",
                BindingFlags.NonPublic | BindingFlags.Instance);

            var instance = constructor.Invoke(new object[0]);
            setHandleMethod.Invoke(instance, new object[] {processHandle});

            return instance;
        }
    }
}

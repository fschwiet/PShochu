using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using NJasmine;
using PShochu.PInvoke;
using PShochu.PInvoke.NetWrappers;

namespace PShochu.Tests
{
    public class verify_privileges : GivenWhenThenFixture
    {
        public override void Specify()
        {
            given("the current user's access token", delegate
            {

                //var currentUserToken = arrange(() => AccessToken.GetCurrentAccessToken());
                var currentUserToken = arrange(() => AccessToken.LogonUser("user", "password"));
                var duplicatedToken = arrange(() => AccessToken.DuplicateTokenAsPrimaryToken(currentUserToken));

                it("should have SE_SHUTDOWN_NAME", delegate
                {
                    var exectedPrivilege = AdvApi32PInvoke.SE_SHUTDOWN_NAME;

                    expect(() => CheckPrivilege(exectedPrivilege, duplicatedToken));
                });

                it("should not have privilege SE_ASSIGNPRIMARYTOKEN_NAME", delegate
                {
                    var exectedPrivilege = AdvApi32PInvoke.SE_ASSIGNPRIMARYTOKEN_NAME;

                    expect(() => !CheckPrivilege(exectedPrivilege, duplicatedToken));
                });

                it("should not have privilege SE_TCB_NAME", delegate
                {
                    var exectedPrivilege = AdvApi32PInvoke.SE_TCB_NAME;

                    expect(() => !CheckPrivilege(exectedPrivilege, duplicatedToken));
                });
            });

            given("the local system account", delegate
            {
                SafeHandle localSystemToken = null; // TODO

                it("should privilege SE_TCB_NAME", delegate
                {
                    var exectedPrivilege = AdvApi32PInvoke.SE_TCB_NAME;

                    expect(() => CheckPrivilege(exectedPrivilege, localSystemToken));
                });
            });

            given("we'd like CreateProcessWithTokenW to work", delegate
            {
                it("should have privilege SE_ASSIGNPRIMARYTOKEN_NAME");
                it("should have privilege SE_INCREASE_QUOTA_NAME");
                it("should have privilege SE_TCB_NAME");
            });
        }

        private bool CheckPrivilege(string exectedPrivilege, SafeHandle currentUserToken)
        {
            AdvApi32PInvoke.LUID luid = default(AdvApi32PInvoke.LUID);

            expect(() => get_privilege_identifier(exectedPrivilege, out luid));

            AdvApi32PInvoke.PRIVILEGE_SET privilegeSet = new AdvApi32PInvoke.PRIVILEGE_SET();
            privilegeSet.PrivilegeCount = 1;
            privilegeSet.Control = AdvApi32PInvoke.PRIVILEGE_SET.PRIVILEGE_SET_ALL_NECESSARY;
            privilegeSet.Privilege = new AdvApi32PInvoke.LUID_AND_ATTRIBUTES[1];
            privilegeSet.Privilege[0].Luid = luid;
            privilegeSet.Privilege[0].Attributes = AdvApi32PInvoke.LUID_AND_ATTRIBUTES.SE_PRIVILEGE_REMOVED;

            Console.WriteLine(luid.LowPart + " " + luid.HighPart);
            bool privilegeCheckResult;

            bool executionResult = AdvApi32PInvoke.PrivilegeCheck(currentUserToken.DangerousGetHandle(),
                ref privilegeSet, out privilegeCheckResult);

            expect(() => executionResult);

            return privilegeCheckResult;
        }

        private bool get_privilege_identifier(string expectedPrivilege, out AdvApi32PInvoke.LUID luid)
        {
            string systemName = null;

            return AdvApi32PInvoke.LookupPrivilegeValue(systemName, expectedPrivilege, out luid);
        }
    }
}

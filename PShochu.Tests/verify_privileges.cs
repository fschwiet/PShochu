using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NJasmine;

namespace PShochu.Tests
{
    public class verify_privileges : GivenWhenThenFixture
    {
        public override void Specify()
        {
            it("should have privilege SE_ASSIGNPRIMARYTOKEN_NAME");

            it("should have privilege SE_TCB_NAME");

            it("should have privilege SE_INCREASE_QUOTA_NAME");
        }
    }
}

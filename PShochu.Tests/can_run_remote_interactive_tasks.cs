using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NJasmine;

namespace PShochu.Tests
{
    public class can_run_remote_interactive_tasks : GivenWhenThenFixture
    {
        public override void Specify()
        {
            given("a psake script which writes the current process id to output", delegate
            {
                when("that script is invoked interactively", delegate
                {
                    then("he output file has a different process id than the current");
                });
            });

            given("a vhd image (64bit?  32bit?)", delegate
            {
                given("a psake script that writes its IP address and process id", delegate
                {
                    when("the script is invoked remotely", delegate
                    {
                        then("the reported ip address and process id are different");
                    });
                });
            });
        }
    }
}

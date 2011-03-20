using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using NJasmine;

namespace PShochu.Tests
{
    public class can_run_remote_interactive_tasks : GivenWhenThenFixture
    {
        public override void Specify()
        {
            var psakeModuleLocation = Properties.Settings.Default.PsakeModulePath;

            given("a psake script which writes the current process id to output", delegate
            {
                string scriptPath = GetVerifiedPathOfTestScript("task_writes_process_id.ps1");

                when("that script is invoked interactively", delegate
                {
                    InvokeResult invocation = arrange(() =>
                                                      ProcessHandling.InvokeScript(Path.Combine(".", psakeModuleLocation), scriptPath));

                    then("the script succeeds", delegate
                    {
                        expect(() => invocation.ExitCode == 0);
                    });

                    then("the output file has a different process id than the current", delegate
                    {
                        var allLines = invocation.ConsoleOutput;

                        int? processId =
                            allLines.Select(l => Regex.Match(l, @"Process ID: (\d*)")).Where(m => m.Success).Select(
                                m => int.Parse(m.Groups[1].Value)).Single();

                        expect(() => processId.Value > 0);
                        expect(() => processId.Value != Process.GetCurrentProcess().Id);
                    });
                });
            });

            given("a psake script that fails", delegate
            {
                var scriptPath = GetVerifiedPathOfTestScript("task_fails_assert.ps1");

                when("that script is invoked interactively", delegate
                {
                    InvokeResult invocation = arrange(() =>
                                                      ProcessHandling.InvokeScript(Path.Combine(".", psakeModuleLocation), scriptPath));

                    then("the error output has the expected error string", delegate
                    {
                        var allLines = invocation.ErrorOutput;

                        Console.WriteLine("Last exit code:" + invocation.ExitCode);

                        foreach (var line in allLines)
                        {
                            Console.WriteLine(line);
                        }

                    });
                });
            });

            given("a non-default psake script", delegate
            {
                var scriptPath = GetVerifiedPathOfTestScript("task_writes_process_id.ps1");

                when("that script is invoked interactively", delegate
                {
                    InvokeResult invocation = arrange(() =>
                                                      ProcessHandling.InvokeScript(
                                                          Path.Combine(".", psakeModuleLocation), scriptPath, "Other"));

                    then("the other tasks output is seen", delegate
                    {
                        expect(() => invocation.ConsoleOutput.Any(l => l.Equals("Another task")));
                    });
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

        private string GetVerifiedPathOfTestScript(string psakeScript)
        {
            var result = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                @"scripts\" + psakeScript);

            arrange(() => expect(() => File.Exists(result)));

            return result;
        }
    }
}

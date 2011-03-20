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

            var consoleStream = arrange(() => new MemoryStream());
            var errorStream = arrange(() => new MemoryStream());

            var consoleWriter = arrange(() => new NonclosingStreamWriter(consoleStream));
            var errorWriter = arrange(() => new NonclosingStreamWriter(errorStream));

            given("a psake script which writes the current process id to output", delegate
            {
                string scriptPath = GetVerifiedPathOfTestScript("task_writes_process_id.ps1");

                when("that script is invoked interactively", delegate
                {
                    var exitCode = arrange(() =>
                        ProcessHandling.InvokeScript(Path.Combine(".", psakeModuleLocation), 
                            scriptPath, s => consoleWriter.WriteLine(s),
                                s => errorWriter.WriteLine(s)));

                    then("the script succeeds", delegate
                    {
                        expect(() => exitCode == 0);
                    });

                    then("the output file has a different process id than the current", delegate
                    {
                        consoleWriter.Flush();
                        consoleStream.Seek(0, SeekOrigin.Begin);

                        int? processId = null;

                        using(var reader = new NonclosingStreamReader(consoleStream))
                        {
                            string nextLine;

                            while ((nextLine = reader.ReadLine()) != null)
                            {
                                Console.WriteLine(nextLine);
                                var match = Regex.Match(nextLine, @"Process ID: (\d*)");

                                if (match.Success)
                                    processId = int.Parse(match.Groups[1].Value);
                            }
                        }

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
                    var exitCode = arrange(() =>
                            ProcessHandling.InvokeScript(Path.Combine(".", psakeModuleLocation),
                                scriptPath, s => consoleWriter.WriteLine(s),
                                s => errorWriter.WriteLine(s)));

                    then("the error output has the expected error string", delegate
                    {
                        errorWriter.Flush();
                        errorStream.Seek(0, SeekOrigin.Begin);

                        var reader = arrange(() => new NonclosingStreamReader(errorStream));
                        var allLines = reader.ReadToEnd().Split(new string[] {errorWriter.NewLine}, StringSplitOptions.None);

                        Console.WriteLine("Last exit code:" + exitCode);

                        foreach (var line in allLines)
                        {
                            Console.WriteLine(line);
                        }

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

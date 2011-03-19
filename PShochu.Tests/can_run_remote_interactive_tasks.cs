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
            given("a psake script which writes the current process id to output", delegate
            {
                var scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                    @"scripts\task_writes_process_id.ps1");

                var psakeModuleLocation = Properties.Settings.Default.PsakeModulePath;

                arrange(() => expect(() => File.Exists(scriptPath)));

                when("that script is invoked interactively", delegate
                {
                    var consoleStream = arrange(() => new MemoryStream());
                    var errorStream = arrange(() => new MemoryStream());

                    var consoleWriter = arrange(() => new NonclosingStreamWriter(consoleStream));
                    var errorWriter = arrange(() => new NonclosingStreamWriter(errorStream));

                    arrange(
                        () =>
                        ProcessHandling.InvokeScript(Path.Combine(".", psakeModuleLocation), 
                            scriptPath, s => consoleWriter.WriteLine(s),
                                s => errorWriter.WriteLine(s)));

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

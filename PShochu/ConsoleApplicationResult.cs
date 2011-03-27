using System;
using System.Collections.Generic;
using System.IO;
using PShochu.Util;

namespace PShochu
{
    public class ConsoleApplicationResult : ConsoleApplicationResultStreams
    {
        public readonly IEnumerable<string> ConsoleOutput;
        public readonly IEnumerable<string> ErrorOutput;

        public ConsoleApplicationResult(MemoryStream consoleStream, MemoryStream errorStream, string[] consoleOutput, string[] errorOutput, int? exitCode)
            : base(consoleStream, errorStream, exitCode)
        {
            ConsoleOutput = consoleOutput;
            ErrorOutput = errorOutput;
        }

        public void TraceToConsole()
        {
            Console.WriteLine("CONSOLE OUTPUT");
            foreach (var o in ConsoleOutput)
                Console.WriteLine(o);

            Console.WriteLine("ERROR OUTPUT");
            foreach (var o in this.ErrorOutput)
                Console.WriteLine(o);
        }

        public static ConsoleApplicationResult LoadConsoleOutput(MemoryStream consoleStream, MemoryStream errorStream, NonclosingStreamWriter consoleWriter, int? exitCode)
        {
            ConsoleApplicationResult result;
            var consoleOutput =
                new NonclosingStreamReader(consoleStream).ReadToEnd().Split(new[] { consoleWriter.NewLine },
                    StringSplitOptions.None);

            var errorOutput =
                new NonclosingStreamReader(errorStream).ReadToEnd().Split(new[] { consoleWriter.NewLine },
                    StringSplitOptions.None);

            result = new ConsoleApplicationResult(consoleStream, errorStream, consoleOutput, errorOutput, exitCode);

            return result;
        }
    }
}
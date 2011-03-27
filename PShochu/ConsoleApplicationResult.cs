using System;
using System.Collections.Generic;
using PShochu.Util;

namespace PShochu
{
    public class ConsoleApplicationResult
    {
        public int ExitCode;
        public readonly IEnumerable<string> ConsoleOutput;
        public readonly IEnumerable<string> ErrorOutput;

        public ConsoleApplicationResult(string[] consoleOutput, string[] errorOutput, int exitCode)
        {
            ExitCode = exitCode;
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

        public static ConsoleApplicationResult LoadConsoleOutput(ConsoleApplicationResultStreams resultStreams, string newline)
        {
            var consoleOutput =
                resultStreams.ConsoleStream.ReadToEnd().Split(new[] { newline },
                    StringSplitOptions.None);

            var errorOutput =
                resultStreams.ErrorStream.ReadToEnd().Split(new[] { newline },
                    StringSplitOptions.None);

            return new ConsoleApplicationResult(consoleOutput, errorOutput, resultStreams.ExitCode);
        }
    }
}
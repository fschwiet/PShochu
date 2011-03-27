using System;
using System.Collections.Generic;
using System.IO;
using PShochu.Util;

namespace PShochu
{
    public class ConsoleApplicationResult : IDisposable
    {
        public readonly Stream ConsoleStream = new MemoryStream();
        public readonly Stream ErrorStream = new MemoryStream();

        public readonly int? ExitCode;
        public readonly IEnumerable<string> ConsoleOutput;
        public readonly IEnumerable<string> ErrorOutput;

        public ConsoleApplicationResult(MemoryStream consoleStream, MemoryStream errorStream, string[] consoleOutput, string[] errorOutput, int? exitCode)
        {
            ConsoleStream = consoleStream;
            ErrorStream = errorStream;
            ConsoleOutput = consoleOutput;
            ErrorOutput = errorOutput;
            ExitCode = exitCode;
        }

        public void Dispose()
        {
            ConsoleStream.Dispose();
            ErrorStream.Dispose();
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
    }
}
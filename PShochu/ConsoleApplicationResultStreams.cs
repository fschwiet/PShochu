using System;
using System.IO;

namespace PShochu
{
    public class ConsoleApplicationResultStreams : IDisposable
    {
        public Stream ConsoleStream;
        public Stream ErrorStream;
        public int? ExitCode;

        public ConsoleApplicationResultStreams(MemoryStream consoleStream, MemoryStream errorStream, int? exitCode)
        {
            ConsoleStream = consoleStream;
            ErrorStream = errorStream;
            ExitCode = exitCode;
        }

        public void Dispose()
        {
            ConsoleStream.Dispose();
            ErrorStream.Dispose();
        }
    }
}
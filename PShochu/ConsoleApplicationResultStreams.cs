using System;
using System.IO;

namespace PShochu
{
    public class ConsoleApplicationResultStreams : IDisposable
    {
        public StreamReader ConsoleStream;
        public StreamReader ErrorStream;
        public int ExitCode;

        public ConsoleApplicationResultStreams(StreamReader consoleStream, StreamReader errorStream, int exitCode)
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
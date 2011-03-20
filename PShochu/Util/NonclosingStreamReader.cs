using System.IO;

namespace PShochu.Util
{
    public class NonclosingStreamReader : StreamReader
    {
        public NonclosingStreamReader(MemoryStream stream) : base(stream)
        {
        }

        public override void Close()
        {
            base.Dispose(false);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(false);
        }
    }
}

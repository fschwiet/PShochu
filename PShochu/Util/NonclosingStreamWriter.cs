using System.IO;

namespace PShochu.Util
{
    public class NonclosingStreamWriter : StreamWriter
    {
        public NonclosingStreamWriter(MemoryStream stream) : base(stream)
        {
        }

        public override void Close()
        {
            base.Dispose(false);
        }

        protected override void  Dispose(bool disposing)
        {
            base.Dispose(false);
        }
    }
}

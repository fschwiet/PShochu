using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PShochu.Tests
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

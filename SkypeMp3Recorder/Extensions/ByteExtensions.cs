using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkypeMp3Recorder.Extensions
{
    public static class ByteExtensions
    {
        public static int IndexOfSequence(this byte[] buffer, byte[] pattern, int startIndex)
        {
            List<int> positions = new List<int>();
            int i = Array.IndexOf<byte>(buffer, pattern[0], startIndex);
            while (i >= 0 && i <= buffer.Length - pattern.Length)
            {
                byte[] segment = new byte[pattern.Length];
                Buffer.BlockCopy(buffer, i, segment, 0, pattern.Length);
                if (segment.SequenceEqual<byte>(pattern))
                    return i;
                i = Array.IndexOf<byte>(buffer, pattern[0], i + 1);
            }
            return -1;
        }
    }
}

using BlueToothDesktop.Serial;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BlueToothDesktop.Utils
{
    public class ByteArrayHandler
    {
        public static byte[] ConstructByteArray(List<ByteArrayModel> Bytes)
        {
            // get how many byes we need
            var byteNo = Bytes.Select(t => t.Bytes.Length).Sum();

            // create byte array
            byte[] bytes = new byte[byteNo];

            int offset = 0;

            foreach (ByteArrayModel Byte in Bytes)
            {
                // swap endian if needed
                if (!Byte.isString && SerialHandler.SwapEndian) Array.Reverse(Byte.Bytes);
                Buffer.BlockCopy(Byte.Bytes, 0, bytes, offset, Byte.Bytes.Length);
                offset += Byte.Bytes.Length;
            }

            return bytes;
        }

        public static byte[] GetBytesFromArray(byte[] array, int offset, int length, bool isString)
        {
            // create destination array
            byte[] dest = new byte[length];

            // copy array section
            Buffer.BlockCopy(array, offset, dest, 0, length);

            // swap endian if needed (unless it's a string)
            if (!isString && SerialHandler.SwapEndian) Array.Reverse(dest);

            return dest;
        }

        // helpers
        public class ByteArrayModel
        {
            public byte[] Bytes { get; set; }
            public bool isString { get; set; }
        }
    }
}

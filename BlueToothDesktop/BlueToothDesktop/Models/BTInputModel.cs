using BlueToothDesktop.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BlueToothDesktop.Utils.ByteArrayHandler;

namespace BlueToothDesktop.Models
{
    public class BTInputModel
    {
        public byte Key { get; internal set; }
        public ushort Value { get; internal set; }

        public byte[] GetByteArray()
        {
            List<ByteArrayModel> bList = new List<ByteArrayModel>();
            
            // create byte array list
            bList.Add(new ByteArrayModel { Bytes = new byte[] { Key }, isString = false });
            bList.Add(new ByteArrayModel { Bytes = BitConverter.GetBytes(Value), isString = true });

            return ByteArrayHandler.ConstructByteArray(bList);
        }

        public static BTInputModel DecodeByteArray(byte[] bytes)
        {
            byte Key = bytes[0];
            ushort Value = BitConverter.ToUInt16(ByteArrayHandler.GetBytesFromArray(bytes, 1, 2, false), 0);

            return new BTInputModel {
                Value = Value,
                Key = Key
            };
        }
    }
}

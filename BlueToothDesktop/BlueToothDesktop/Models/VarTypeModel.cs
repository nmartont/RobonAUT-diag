using BlueToothDesktop.Enums;
using BlueToothDesktop.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static BlueToothDesktop.Utils.ByteArrayHandler;

namespace BlueToothDesktop.Models
{
    public class VarTypeListModel
    {
        public VarTypeListModel()
        {
            VarTypes = new ObservableCollection<VarTypeModel>();
        }

        public ObservableCollection<VarTypeModel> VarTypes { get; set; }

        public byte[] GetByteArray()
        {
            return VarTypes.Select(t => t.GetByteArray()).SelectMany(byteArr => byteArr).ToArray();
        }

        public static VarTypeListModel DecodeByteArray(byte[] bytes)
        {
            VarTypeListModel Model = new VarTypeListModel();

            int offset = 0;
            while (offset < bytes.Length)
            {
                // read name string length
                byte len = ByteArrayHandler.GetBytesFromArray(bytes, offset, 1, false)[0];

                // get the VarType model
                int arrayLen = len + 2;
                byte[] byteArr = new byte[arrayLen];
                Buffer.BlockCopy(bytes, offset, byteArr, 0, arrayLen);
                VarTypeModel m = VarTypeModel.DecodeByteArray(byteArr);

                offset += arrayLen;
                
                // add model
                Model.VarTypes.Add(m);
            }

            return Model;
        }
    }

    public class VarTypeModel
    {
        public string Name { get; set; }
        public VarTypeEnum VarType { get; set; }

        public byte[] GetByteArray() {
            List<ByteArrayModel> bList = new List<ByteArrayModel>();

            // get string bytes
            byte[] str = Encoding.ASCII.GetBytes(Name);

            // get length
            byte len = Convert.ToByte(str.Length);

            // create byte array list
            bList.Add(new ByteArrayModel { Bytes = new byte[] { len }, isString = false});
            bList.Add(new ByteArrayModel { Bytes = str, isString = true });
            bList.Add(new ByteArrayModel { Bytes = new byte[] { (byte)VarType }, isString = false });
                
            return ByteArrayHandler.ConstructByteArray(bList);
        }

        public static VarTypeModel DecodeByteArray(byte[] bytes)
        {
            int offset = 0;
            // read name string length
            byte len = ByteArrayHandler.GetBytesFromArray(bytes, offset, 1, false)[0];
            offset++;
            // get name string
            string name = Encoding.ASCII.GetString(ByteArrayHandler.GetBytesFromArray(bytes, offset, len, true));
            offset += len;
            // get var type
            byte varType = ByteArrayHandler.GetBytesFromArray(bytes, offset, 1, false)[0];
            offset++;
            // create model
            VarTypeModel m = new VarTypeModel { Name = name, VarType = (VarTypeEnum)varType };

            return m;
        }
    }
}

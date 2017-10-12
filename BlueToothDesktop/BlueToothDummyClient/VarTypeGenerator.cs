using System;
using BlueToothDesktop.Enums;
using BlueToothDesktop.Models;
using static BlueToothDesktop.Utils.ByteArrayHandler;
using System.Collections.Generic;
using BlueToothDesktop.Utils;

namespace BlueToothDummyClient
{
    public class VarTypeGenerator
    {
        private static int i = 0;

        public static VarTypeListModel GetVarTypes()
        {
            VarTypeListModel VarTypesList = new VarTypeListModel();

            VarTypesList.VarTypes.Add(new VarTypeModel
            {
                Name = "DummyVar1",
                VarType = VarTypeEnum.int16
            });

            VarTypesList.VarTypes.Add(new VarTypeModel
            {
                Name = "AnotherVar",
                VarType = VarTypeEnum.int32
            });

            VarTypesList.VarTypes.Add(new VarTypeModel
            {
                Name = "DummyVar2",
                VarType = VarTypeEnum.int8
            });

            VarTypesList.VarTypes.Add(new VarTypeModel
            {
                Name = "AnotherVar3",
                VarType = VarTypeEnum.uint16
            });

            VarTypesList.VarTypes.Add(new VarTypeModel
            {
                Name = "DummyVar4",
                VarType = VarTypeEnum.uint32
            });

            VarTypesList.VarTypes.Add(new VarTypeModel
            {
                Name = "AnotherVar5",
                VarType = VarTypeEnum.uint8
            });
            
            return VarTypesList;
        }

        internal static byte[] GetVarBytes()
        {
            var varTypeList = GetVarTypes();
            List<ByteArrayModel> Bytes = new List<ByteArrayModel>();

            // loop through var types, create values
            foreach (VarTypeModel VarType in varTypeList.VarTypes)
            {
                switch (VarType.VarType)
                {
                    case VarTypeEnum.uint8:
                        Bytes.Add(new ByteArrayModel { Bytes = new byte[] { 200 }, isString = false });
                        break;
                    case VarTypeEnum.uint16:
                        Bytes.Add(new ByteArrayModel { Bytes = BitConverter.GetBytes((ushort)765), isString = false });
                        break;
                    case VarTypeEnum.uint32:
                        Bytes.Add(new ByteArrayModel { Bytes = BitConverter.GetBytes((uint)11765), isString = false });
                        break;
                    case VarTypeEnum.int8:
                        Bytes.Add(new ByteArrayModel { Bytes = new byte[] { 200 }, isString = false });
                        break;
                    case VarTypeEnum.int16:
                        Bytes.Add(new ByteArrayModel { Bytes = BitConverter.GetBytes((short)365), isString = false });
                        break;
                    case VarTypeEnum.int32:
                        Bytes.Add(new ByteArrayModel { Bytes = BitConverter.GetBytes(i), isString = false });
                        break;
                }
            }

            i++;
            byte[] bytes = ByteArrayHandler.ConstructByteArray(Bytes);
            return bytes;
        }
    }
}

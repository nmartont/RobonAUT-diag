using System;
using BlueToothDesktop.Enums;
using BlueToothDesktop.Models;
using System.Collections.Generic;

namespace BlueToothDesktop.Utils
{
    public class ModelDecoder
    {
        public static dynamic DecodeMessage(MessageTypeEnum msgType, byte[] msgBytes)
        {
            switch (msgType)
            {
                case MessageTypeEnum.VarList:
                    return VarTypeListModel.DecodeByteArray(msgBytes);
                case MessageTypeEnum.StatusError:
                    return ErrorStatusModel.DecodeByteArray(msgBytes);
                case MessageTypeEnum.StatusOk:
                    return OkStatusModel.DecodeByteArray(msgBytes);
                case MessageTypeEnum.StatusRequest:
                    return StatusRequestModel.DecodeByteArray(msgBytes);
                case MessageTypeEnum.MonitorStart:
                    return MonitorStartModel.DecodeByteArray(msgBytes);
                case MessageTypeEnum.MonitorStop:
                    return MonitorStopModel.DecodeByteArray(msgBytes);
                case MessageTypeEnum.VarListRequest:
                    return VarListRequestModel.DecodeByteArray(msgBytes);
                case MessageTypeEnum.VarValues:
                    return msgBytes;
                default:
                    return null;
            }
        }

        internal static object[] GetVarValuesFromBytes(VarTypeListModel varTypeList, byte[] msgBytes)
        {
            // output model
            List<object> ObjList = new List<object>();

            // loop through var types, parse shit
            int offset = 0;
            foreach(VarTypeModel VarType in varTypeList.VarTypes)
            {
                byte[] b;
                switch (VarType.VarType)
                {
                    case VarTypeEnum.uint8:
                        ObjList.Add(msgBytes[offset]);
                        offset += 1;
                        break;
                    case VarTypeEnum.uint16:
                        b = ByteArrayHandler.GetBytesFromArray(msgBytes, offset, 2, false);
                        offset += b.Length;
                        ObjList.Add(BitConverter.ToUInt16(b, 0));
                        break;
                    case VarTypeEnum.uint32:
                        b = ByteArrayHandler.GetBytesFromArray(msgBytes, offset, 4, false);
                        offset += b.Length;
                        ObjList.Add(BitConverter.ToUInt32(b, 0));
                        break;
                    case VarTypeEnum.int8:
                        ObjList.Add((sbyte)msgBytes[offset]);
                        offset += 1;
                        break;
                    case VarTypeEnum.int16:
                        b = ByteArrayHandler.GetBytesFromArray(msgBytes, offset, 2, false);
                        ObjList.Add(BitConverter.ToInt16(b, 0));
                        offset += b.Length;
                        break;
                    case VarTypeEnum.int32:
                        b = ByteArrayHandler.GetBytesFromArray(msgBytes, offset, 4, false);
                        ObjList.Add(BitConverter.ToInt32(b, 0));
                        offset += b.Length;
                        break;
                }
            }
            return ObjList.ToArray();
        }
    }
}

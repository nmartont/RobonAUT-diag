using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueToothDesktop.Models
{
    public class MonitorStopModel
    {
        public byte[] GetByteArray()
        {
            return new byte[0];
        }

        public static MonitorStopModel DecodeByteArray(byte[] bytes)
        {
            return new MonitorStopModel();
        }
    }
}

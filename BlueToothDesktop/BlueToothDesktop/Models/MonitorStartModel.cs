using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueToothDesktop.Models
{
    public class MonitorStartModel
    {
        public byte[] GetByteArray()
        {
            return new byte[0];
        }

        public static MonitorStartModel DecodeByteArray(byte[] bytes)
        {
            return new MonitorStartModel();
        }
    }
}

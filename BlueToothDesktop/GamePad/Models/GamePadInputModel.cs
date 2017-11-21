using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamePad.Models
{
    public class GamePadInputModel
    {
        public string InputName { get; set; }
        public int Value { get; set; }
        public int Timestamp { get; set; }
        public int InputNumber { get; set; }

        public override string ToString() {
            return InputName + " (" + InputNumber + "): " + Value + " (" + Timestamp + ")";
        }
    }
}

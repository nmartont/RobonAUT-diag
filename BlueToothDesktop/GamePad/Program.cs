using GamePad.Models;
using GamePad.PadHandler;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamePad
{
    class Program
    {
        static void Main(string[] args)
        {
            // Initialize DirectInput
            var directInput = new DirectInput();

            // Find a Joystick Guid
            var joystickGuid = Guid.Empty;

            var GamePads = directInput.GetDevices(DeviceType.Gamepad,
                        DeviceEnumerationFlags.AllDevices);

            var JoySticks = directInput.GetDevices(DeviceType.Joystick,
                        DeviceEnumerationFlags.AllDevices);

            foreach (var deviceInstance in GamePads)
                joystickGuid = deviceInstance.InstanceGuid;

            // If Gamepad not found, look for a Joystick
            if (joystickGuid == Guid.Empty)
                foreach (var deviceInstance in directInput.GetDevices(DeviceType.Joystick,
                        DeviceEnumerationFlags.AllDevices))
                    joystickGuid = deviceInstance.InstanceGuid;

            // If Joystick not found, throws an error
            if (joystickGuid == Guid.Empty)
            {
                Console.WriteLine("No joystick/Gamepad found.");
                Console.ReadKey();
                Environment.Exit(1);
            }

            GamePadHandler Hand = new GamePadHandler(new JoyHandler());
            Hand.ConnectJoystick(joystickGuid.ToString());

            var v = Hand.GetGuids();

            // while shit so program doesn't stop
            while (true)
            {
                System.Threading.Thread.Sleep(1000);
            }
        }
    }

    class JoyHandler : JoyStickCallback
    {
        public void JoyStickInput(GamePadInputModel input)
        {
            
        }
    }
}

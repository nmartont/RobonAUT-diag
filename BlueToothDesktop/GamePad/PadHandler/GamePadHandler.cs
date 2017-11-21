using GamePad.Models;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GamePad.PadHandler
{
    public class GamePadHandler : INotifyPropertyChanged
    {
        private JoyStickCallback callback;
        private DirectInput dInput;
        private Guid guid;
        private Joystick Joy;
        
        public GamePadHandler(JoyStickCallback jcb)
        {
            dInput = new DirectInput();
            callback = jcb;
            IsConnected = false;
        }

        // event handling for binding
        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        // properties of being connected
        public bool _is_connected = false;
        public bool NotConnected
        {
            get { return !_is_connected; }
            set
            {
                OnPropertyChanged("NotConnected");
            }
        }
        public bool IsConnected
        {
            get { return _is_connected; }
            set
            {
                _is_connected = value;
                NotConnected = !value; // just to trigger event
                OnPropertyChanged("IsConnected");
            }
        }

        public string[] GetGuids()
        {
            List<string> Model = new List<string>();

            var GamePads = dInput.GetDevices(DeviceType.Gamepad,
                        DeviceEnumerationFlags.AllDevices);

            var JoySticks = dInput.GetDevices(DeviceType.Joystick,
                        DeviceEnumerationFlags.AllDevices);

            foreach (var deviceInstance in GamePads)
                Model.Add(deviceInstance.InstanceGuid.ToString());

            foreach (var deviceInstance in JoySticks)
                Model.Add(deviceInstance.InstanceGuid.ToString());

            return Model.Distinct().ToArray();
        }
        
        public void ConnectJoystick(string joystickGuid)
        {
            guid = Guid.Parse(joystickGuid);
            
            // Instantiate the joystick
            Joy = new Joystick(dInput, guid);

            Console.WriteLine("Found Joystick/Gamepad with GUID: {0}", joystickGuid);

            // Set BufferSize in order to use buffered data.
            Joy.Properties.BufferSize = 128;

            // Acquire the joystick
            Joy.Acquire();

            IsConnected = true;
            
            // Poll events from joystick
            Thread t = new Thread(delegate ()
            {
                JoyPadListener();
            });
            t.IsBackground = true;
            t.Start();
        }

        public void ReleaseJoystick()
        {
            // Unacquire the joystick
            Joy.Unacquire();

            IsConnected = false;
        }
        
        private void JoyPadListener()
        {
            while (IsConnected)
            {
                try
                {
                    Joy.Poll();
                    var datas = Joy.GetBufferedData();
                    foreach (var state in datas)
                    {
                        // Console.WriteLine(state);
                        // create model for the input
                        GamePadInputModel Model = new GamePadInputModel {
                            InputName = state.Offset.ToString(),
                            Timestamp = state.Timestamp,
                            Value = state.Value,
                            InputNumber = state.RawOffset
                        };

                        // callback
                        callback.JoyStickInput(Model);
                    }
                }
                catch
                {
                    IsConnected = false;
                    Joy.Unacquire();
                }
            }
        }
    }

    public interface JoyStickCallback
    {
        void JoyStickInput(GamePadInputModel input);
    }
}

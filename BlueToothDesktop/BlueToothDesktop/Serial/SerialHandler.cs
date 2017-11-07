using BlueToothDesktop.Enums;
using BlueToothDesktop.Utils;
using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.IO.Ports;
using System.Threading;

namespace BlueToothDesktop.Serial
{
    public abstract class SerialHandler : INotifyPropertyChanged
    {
        public WindowCallback Callback;
        private SerialPort port;
        private BlockingCollection<byte[]> IncomingQueue = new BlockingCollection<byte[]>();
        private byte[] extraBytes = new byte[0];
        private static bool desiredLittleEndian = true;
        private static bool converterLittleEndian = BitConverter.IsLittleEndian;
        public static byte msgEndChar = 255;

        public SerialHandler(WindowCallback cb)
        {
            Callback = cb;

            // start queue listeners
            Thread t = new Thread(delegate ()
            {
                InputQueueListener();
            });
            t.IsBackground = true;
            t.Start();
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

        // property for endian swap
        public static bool SwapEndian
        {
            get { return desiredLittleEndian != converterLittleEndian; }
        }

        // Connection handling functions
        public void Connect(string portName)
        {

            port = new SerialPort(portName, 115200, Parity.None, 8, StopBits.One);
            port.WriteTimeout = 1000;

            try
            {
                SerialPortProgram();
                IsConnected = true;
                Callback.AppendLog("Successfully connected to port " + portName);
            }
            catch (Exception ex)
            {
                Callback.AppendLog("Error while connecting to port " + portName + ":\n" + ex.Message);
            }
        }

        public void Disconnect()
        {
            try
            {
                port.Close();
                IsConnected = false;
                Callback.AppendLog("Successfully disconnected from port " + port.PortName);
            }
            catch (Exception ex)
            {
                Callback.AppendLog("Error while disconnecting from port:\n" + ex.Message);
            }
        }

        // COM port handling
        private void SerialPortProgram()
        {
            // Attach a method to be called when there
            // is data waiting in the port's buffer
            port.DataReceived += new
              SerialDataReceivedEventHandler(port_DataReceived);

            // Begin communications
            port.Open();
            // Empty buffer so stuck data is discarded
            port.DiscardInBuffer();
        }

        // COM message callback
        private void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int bytes = port.BytesToRead;
            byte[] buffer = new byte[bytes];
            port.Read(buffer, 0, bytes);

            // append buffer to extra bytes from before
            byte[] extendedBuffer = new byte[extraBytes.Length + buffer.Length];
            Buffer.BlockCopy(extraBytes, 0, extendedBuffer, 0, extraBytes.Length);
            Buffer.BlockCopy(buffer, 0, extendedBuffer, extraBytes.Length, buffer.Length);

            // separate messages from buffer
            int offset = 0;
            while (offset < extendedBuffer.Length)
            {
                // check for end message character
                int index = -1;
                for (int i = offset; i < extendedBuffer.Length; i++)
                {
                    if (extendedBuffer[i] == msgEndChar)
                    {
                        index = i;
                        break;
                    }
                }

                // error handling, end of message can't be first element
                if (index == 0)
                {
                    extraBytes = new byte[0];
                    break;
                }
                else if (index != -1)
                {
                    extraBytes = new byte[0];
                    // parse message
                    byte[] msgBytes = new byte[index - offset]; // do not copy end of message char
                    Buffer.BlockCopy(extendedBuffer, offset, msgBytes, 0, msgBytes.Length);

                    // put the bytes in a queue
                    IncomingQueue.Add(msgBytes);

                    offset = index + 1;
                }
                else
                {
                    extraBytes = new byte[extendedBuffer.Length - offset];
                    Buffer.BlockCopy(extendedBuffer, offset, extraBytes, 0, extraBytes.Length);
                    offset = extendedBuffer.Length;
                }
            }
        }

        // listen on the queue
        public void InputQueueListener()
        {
            while (true)
            {
                var bytes = IncomingQueue.Take();
                HandleReceivedBytes(bytes);
            }
        }

        public void HandleReceivedBytes(byte[] buffer)
        {
            // check message type
            MessageTypeEnum MsgType = MessageTypeEnum.StatusError;

            try
            {
                MsgType = (MessageTypeEnum)buffer[0];
            }
            catch (Exception ex)
            {
                Callback.AppendLog("Error while parsing message type:\n" + ex.Message);
                return;
            }

            // remove msg type
            byte[] msgBytes = new byte[buffer.Length - 1];
            Buffer.BlockCopy(buffer, 1, msgBytes, 0, msgBytes.Length);

            // decode message into c# model
            dynamic MessageModel = null;
            try
            {
                MessageModel = ModelDecoder.DecodeMessage(MsgType, msgBytes);
            }
            catch (Exception ex)
            {
                Callback.AppendLog("Error while parsing message:\n" + ex.Message);
                return;
            }

            if (MessageModel == null)
            {
                Callback.AppendLog("Unknown message type, cannot decode.");
                return;
            }

            // handle message in implementation
            HandleIncomingMessageModel(MsgType, MessageModel);
        }

        public abstract void HandleIncomingMessageModel(MessageTypeEnum msgType, dynamic messageModel);
        
        public bool SendBytes(byte[] bytes)
        {
            try
            {
                // send bytes to port
                port.Write(bytes, 0, bytes.Length);
                return true;
            }
            catch (Exception ex)
            {
                Callback.AppendLog("Error while sending bytes:\n" + ex.Message);
                return false;
            }
        }

        public void SendBytes(MessageTypeEnum msgType, byte[] payload)
        {
            // In payload, swap 255 for 254
            for (int i = 0; i < payload.Length; i++)
            {
                if (payload[i] == msgEndChar)
                    payload[i] -= 1;
            }

            // get message length
            ushort msgLength = (ushort)(payload.Length + 2);
            // create byte array for the final message
            byte[] b = new byte[msgLength];
            // put msg type into first bit
            b[0] = (byte)msgType;
            // put payload at the end
            Buffer.BlockCopy(payload, 0, b, 1, payload.Length);
            // put end of message character at the end
            b[msgLength - 1] = msgEndChar;

            // send message
            SendBytes(b);
        }

        public void SendBytes(MessageTypeEnum msgType, dynamic Model)
        {
            // get bytes
            var bytes = Model.GetByteArray();
            
            // send bytes
            SendBytes(msgType, bytes);
        }
    }
}

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

            port = new SerialPort(portName, 9600, Parity.None, 8, StopBits.One);
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
                // check message length
                byte[] msgLenB = new byte[2];
                Buffer.BlockCopy(extendedBuffer, offset, msgLenB, 0, msgLenB.Length);
                ushort msgLen = BitConverter.ToUInt16(msgLenB, 0);

                // if msgLen is bigger than the remaining buffer length, save this part of the message to extra bits and break
                if (msgLen > extendedBuffer.Length - offset)
                {
                    extraBytes = new byte[extendedBuffer.Length - offset];
                    Buffer.BlockCopy(extendedBuffer, offset, extraBytes, 0, extraBytes.Length);
                    break;
                }

                // if not, parse message
                byte[] msgBytes = new byte[msgLen - msgLenB.Length];
                Buffer.BlockCopy(extendedBuffer, offset + msgLenB.Length, msgBytes, 0, msgBytes.Length);

                // put the bytes in a queue
                IncomingQueue.Add(msgBytes);

                offset += msgLen;
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
            Buffer.BlockCopy(buffer, 1, msgBytes, 0, buffer.Length - 1);

            // decode message into c# model
            dynamic MessageModel = ModelDecoder.DecodeMessage(MsgType, msgBytes);

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
            // get message length
            ushort msgLength = (ushort)(payload.Length + 3);
            // create byte array for the final message
            byte[] b = new byte[msgLength];
            // put message length into first two bits
            byte[] lenBytes = BitConverter.GetBytes(msgLength);
            Buffer.BlockCopy(lenBytes, 0, b, 0, lenBytes.Length);
            // put msg type into third bit
            b[2] = (byte)msgType;
            // put payload at the end
            Buffer.BlockCopy(payload, 0, b, 3, payload.Length);

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

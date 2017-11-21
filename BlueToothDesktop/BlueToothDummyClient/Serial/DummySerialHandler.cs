using BlueToothDesktop.Serial;
using BlueToothDesktop;
using BlueToothDesktop.Enums;
using System.Threading;
using System;
using BlueToothDesktop.Models;

namespace BlueToothDummyClient.Serial
{
    class DummySerialHandler: SerialHandler
    {
        private bool runMemes = false;
        private StatusEnum Status = StatusEnum.OK;

        public DummySerialHandler(WindowCallback cb) : base(cb) { }

        public override void HandleIncomingMessageModel(MessageTypeEnum msgType, dynamic messageModel)
        {
            Callback.AppendLog("Incoming message: " + msgType.ToString());

            switch (msgType)
            {
                case MessageTypeEnum.StatusRequest:
                    SendStatus();
                    break;
                case MessageTypeEnum.MonitorStart:
                    MonitorStart();
                    break;
                case MessageTypeEnum.MonitorStop:
                    MonitorStop();
                    break;
                case MessageTypeEnum.VarListRequest:
                    SendVarTypes();
                    break;
                case MessageTypeEnum.BTInput:
                    Callback.AppendLog("Button" + messageModel.Key + ": " + messageModel.Value);
                    break;
            }
        }

        private void MonitorStop()
        {
            runMemes = false;
        }

        private void MonitorStart()
        {
            SendVars();
        }

        private void SendStatus()
        {
            // message type
            MessageTypeEnum msgType = MessageTypeEnum.StatusOk;
            // model
            dynamic Model = null;

            switch (Status)
            {
                case StatusEnum.OK:
                    msgType = MessageTypeEnum.StatusOk;
                    Model = new OkStatusModel();
                    break;
                case StatusEnum.ERROR:
                    msgType = MessageTypeEnum.StatusError;
                    Model = new ErrorStatusModel { Text = "Error lol" };
                    break;
            }
            
            // send bytes
            SendBytes(msgType, Model);
        }

        // productive functions
        public void SendVarTypes()
        {
            // message type
            var msgType = MessageTypeEnum.VarList;
            // get var types
            var Model = VarTypeGenerator.GetVarTypes();

            // send bytes
            SendBytes(msgType, Model);
        }

        internal void SendVars()
        {
            if (!runMemes)
            {
                runMemes = true;
                // start queue listener
                Thread t = new Thread(delegate ()
                {
                    SendVarMessage();
                });
                t.IsBackground = true;
                t.Start();
            }
        }

        private void SendVarMessage()
        {
            while(runMemes)
            {
                Thread.Sleep(10);
                SendVarBytes();
            }
        }

        private void SendVarBytes()
        {
            SendBytes(MessageTypeEnum.VarValues, VarTypeGenerator.GetVarBytes());
        }
    }
}

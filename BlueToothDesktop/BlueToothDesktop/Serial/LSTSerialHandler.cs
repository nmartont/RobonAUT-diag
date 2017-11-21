using System;
using BlueToothDesktop.Enums;
using BlueToothDesktop.Models;
using System.Data;
using BlueToothDesktop.Utils;
using GamePad.PadHandler;
using GamePad.Models;

namespace BlueToothDesktop.Serial
{
    class LSTBlueToothHandler: SerialHandler, JoyStickCallback
    {
        private new LSTWindowCallback Callback;
        private StatusEnum Status = StatusEnum.OK;
        internal VarTypeListModel VarTypeList = new VarTypeListModel();
        internal DataTable VarData = new DataTable();
        
        public LSTBlueToothHandler(LSTWindowCallback cb) : base(cb) {
            Callback = cb;
            VarData.Columns.Add("Variables");
        }

        public override void HandleIncomingMessageModel(MessageTypeEnum msgType, dynamic messageModel)
        {
            switch (msgType)
            {
                case MessageTypeEnum.StatusOk:
                    Callback.AppendLog("Controller status: " + msgType.ToString());
                    Callback.SetStatus(msgType.ToString());
                    break;
                case MessageTypeEnum.StatusError:
                    Callback.AppendLog("Controller ERROR status: " + ((ErrorStatusModel)messageModel).Text);
                    Callback.SetStatus(msgType.ToString());
                    break;
                case MessageTypeEnum.VarList:
                    VarTypeListModel M = (VarTypeListModel)messageModel;
                    System.Windows.Application.Current.Dispatcher.Invoke(new Action(() => {
                        // clear lists and tables
                        VarTypeList.VarTypes.Clear();
                        Callback.ClearColumns();
                        VarData.Rows.Clear();

                        foreach (VarTypeModel Model in messageModel.VarTypes)
                        {
                            // update var type list
                            VarTypeList.VarTypes.Add(Model);
                            // update vars table
                            Callback.AddColumn(Model.Name);
                        }
                    }));
                    Callback.AppendLog("Variable list received.");
                    break;
                case MessageTypeEnum.VarValues:
                    try
                    {
                        // decode bytes here:
                        Callback.AddRow(ModelDecoder.GetVarValuesFromBytes(VarTypeList, messageModel));
                    }
                    catch (Exception ex)
                    {
                        Callback.AppendLog(ex.Message);
                    }
                    break;
                case MessageTypeEnum.StatusRequest:
                    SendStatus();
                    break;
                default:
                    Callback.AppendLog("Incoming unknown message: " + msgType.ToString());
                    break;
            }
        }

        // productive functions
        internal void SendStatusRequest()
        {
            // message type
            var msgType = MessageTypeEnum.StatusRequest;
            // get model
            var Model = new StatusRequestModel();

            // send bytes
            SendBytes(msgType, Model);
        }

        internal void SendVarListRequest()
        {
            // message type
            var msgType = MessageTypeEnum.VarListRequest;
            // get model
            var Model = new VarListRequestModel();

            // send bytes
            SendBytes(msgType, Model);
        }

        internal void SendMonitorStartRequest()
        {
            // message type
            var msgType = MessageTypeEnum.MonitorStart;
            // get model
            var Model = new MonitorStartModel();

            // send bytes
            SendBytes(msgType, Model);
        }

        internal void SendMonitorStopRequest()
        {
            // message type
            var msgType = MessageTypeEnum.MonitorStop;
            // get model
            var Model = new MonitorStopModel();

            // send bytes
             SendBytes(msgType, Model);
        }

        private void SendStatus()
        {
            // message type
            MessageTypeEnum msgType = MessageTypeEnum.StatusOk;
            // model
            dynamic Model = null;

            switch (Status){
                case StatusEnum.OK:
                    msgType = MessageTypeEnum.StatusOk;
                    Model = new OkStatusModel();
                    break;
                case StatusEnum.ERROR:
                    msgType = MessageTypeEnum.StatusError;
                    Model = new ErrorStatusModel { Text = "Desktop Error" };
                    break;
            }

            // send bytes
            SendBytes(msgType, Model);
        }
        
        public void JoyStickInput(GamePadInputModel input)
        {
            if (input.Value < 0) input.Value = 1; // remove the negative number from the D-Pad

            // write the info to the GUI somehow
            Callback.SetPadControlText(input.ToString());

            if (IsConnected)
            {
                // get model
                BTInputModel Model = new BTInputModel {
                    Value = Convert.ToUInt16(input.Value),
                    Key = Convert.ToByte(input.InputNumber)
                };
                // message type
                MessageTypeEnum msgType = MessageTypeEnum.BTInput;

                // send UART message
                SendBytes(msgType, Model);
            }
        }
    }
}

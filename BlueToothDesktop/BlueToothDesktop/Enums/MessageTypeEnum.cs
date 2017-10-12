namespace BlueToothDesktop.Enums
{
    public enum MessageTypeEnum : byte
    {
        StatusError = 0,
        StatusOk,
        StatusRequest,
        VarList,
        VarValues,
        VarListRequest,
        MonitorStart,
        MonitorStop,
    }
}

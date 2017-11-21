namespace BlueToothDesktop
{
    public interface WindowCallback
    {
        void AppendLog(string toAppend, bool newLine = true, bool timeStamp = true, bool scrollToEnd = true);
    }

    public interface LSTWindowCallback:WindowCallback
    {
        void SetStatus(string status);
        void AddColumn(string colName);
        void AddRow(params object[] values);
        void ClearColumns();
        void SetPadControlText(string v);
    }
}

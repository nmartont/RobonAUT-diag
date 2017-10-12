namespace BlueToothDesktop.Models
{
    class StatusRequestModel
    {
        public byte[] GetByteArray()
        {
            return new byte[0];
        }

        public static StatusRequestModel DecodeByteArray(byte[] bytes)
        {
            return new StatusRequestModel();
        }
    }
}

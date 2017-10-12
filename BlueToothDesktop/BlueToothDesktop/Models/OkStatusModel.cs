namespace BlueToothDesktop.Models
{
    public class OkStatusModel // empty model
    {
        public byte[] GetByteArray()
        {
            return new byte[0];
        }

        public static OkStatusModel DecodeByteArray(byte[] bytes)
        {
            return new OkStatusModel();
        }
    }
}

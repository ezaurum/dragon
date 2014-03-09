using System;

namespace Dragon
{
    public interface IMessageConverter<in TReq, TAck>
    {
        event Action<TAck, int> ReadCompleted;
        void Convert(byte[] buffer, int offset, int bytesTransferred);
        void GetByte(TReq message, out byte[] messageBytes, out int errorCode);
    }
}
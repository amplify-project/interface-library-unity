using System;

namespace AmplifyPortable.Util
{
    public class StreamExistsException : Exception
    {
        public StreamExistsException(string message) : base(message) {}
        public StreamExistsException(string message, Exception innerException) : base(message, innerException) {}
    }
}

using System;

namespace AmplifyPortable.Util
{
    public class StreamExistsException : Exception
    {
        public StreamExistsException(string streamName) : base($"Stream {streamName} already exists.") {}
    }

    public class StreamNotCreatedException : Exception
    {
        public StreamNotCreatedException(string streamName) : base($"Could not create stream {streamName}.") {}
    }
}

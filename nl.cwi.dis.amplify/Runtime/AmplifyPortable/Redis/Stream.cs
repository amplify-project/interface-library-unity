namespace AmplifyPortable.Redis
{
    public struct StreamType
    {
        public static readonly StreamType Discrete = new StreamType("discrete");
        public static readonly StreamType Continuous = new StreamType("continuous");

        private readonly string _type;

        private StreamType(string type)
        {
            _type = type;
        }

        public override string ToString() => _type;
    }

    public struct StreamDataType
    {
        public static readonly StreamDataType Number = new StreamDataType("number");
        public static readonly StreamDataType String = new StreamDataType("string");
        public static readonly StreamDataType Boolean = new StreamDataType("boolean");

        private readonly string _type;

        private StreamDataType(string type)
        {
            _type = type;
        }

        public override string ToString() => _type;
    }

    public class Stream
    {
        public string Name { get; }

        public Stream(string name)
        {
            Name = name;
        }
    }
}

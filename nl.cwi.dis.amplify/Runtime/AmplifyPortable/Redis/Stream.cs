using AmplifyPortable.Util;
using Newtonsoft.Json;

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

    public class SerializedStream
    {
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("type")] public string Type { get; set; }
        [JsonProperty("dataType")] public string DataType { get; set; }
    }

    public class Stream : IJsonSerializable
    {
        public string Name { get; }
        public StreamType Type { get; }
        public StreamDataType DataType { get; }

        private Connection _connection;

        public Stream(Connection connection, string name, StreamType type, StreamDataType dataType)
        {
            Name = name;
            Type = type;
            DataType = dataType;

            _connection = connection;
        }

        public object Serialize()
        {
            return new SerializedStream
            {
                Name = Name,
                Type = Type.ToString(),
                DataType = DataType.ToString()
            };
        }
    }
}

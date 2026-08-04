using System;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace AmplifyPortable
{
    public enum StreamType
    {
        Discrete,
        Continuous
    }

    public enum StreamDataType
    {
        Number,
        String,
        Boolean
    }

    public class Stream
    {
        private readonly Connection _connection;
        private readonly string _name;
        private readonly StreamType _type;
        private readonly StreamDataType _dataType;
        private bool _isRegistered = true;
        private bool _isSubscribed = false;

        public string Name => _name;
        public StreamType Type => _type;
        public StreamDataType DataType => _dataType;
        public bool IsRegistered => _isRegistered;

        public Stream(Connection connection, string name, StreamType type, StreamDataType dataType)
        {
            _connection = connection;
            _name = name;
            _type = type;
            _dataType = dataType;
        }

        public void Publish(object data)
        {
            if (!_isRegistered)
            {
                throw new InvalidOperationException("Stream is not registered.");
            }

            _connection.Publish(_name, data);
        }

        public void Subscribe<T>(Action<T> callback)
        {
            _connection.Subscribe(_name, (channel, value) =>
            {
                var valueStr = value.ToString();
                if (valueStr != null)
                {
                    var sample = JsonConvert.DeserializeObject<T>(valueStr);
                    callback(sample);
                }
            });
            _isSubscribed = true;
        }

        public void Unsubscribe()
        {
            if (_isSubscribed)
            {
                _connection.Unsubscribe(_name);
                _isSubscribed = false;
            }
        }

        internal void Unregister()
        {
            _isRegistered = false;
        }

        public StreamInfo GetInfo()
        {
            return new StreamInfo
            {
                Name = _name,
                Type = _type.ToString().ToLower(),
                DataType = _dataType.ToString().ToLower()
            };
        }
    }

    public class StreamInfo
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;

        [JsonProperty("dataType")]
        public string DataType { get; set; } = string.Empty;
    }
}

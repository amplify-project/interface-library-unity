using System;
using System.Threading.Tasks;
using AmplifyPortable.Util;
using Newtonsoft.Json;
using StackExchange.Redis;
using UnityEditor.UI;

namespace AmplifyPortable.Redis
{
    public readonly struct StreamType
    {
        public static readonly StreamType Discrete = new StreamType("discrete");
        public static readonly StreamType Continuous = new StreamType("continuous");

        private readonly string _type;

        public StreamType(string type)
        {
            _type = type;
        }

        public override string ToString() => _type;
    }

    public readonly struct StreamDataType
    {
        public static readonly StreamDataType Number = new StreamDataType("number");
        public static readonly StreamDataType String = new StreamDataType("string");
        public static readonly StreamDataType Boolean = new StreamDataType("boolean");

        private readonly string _type;

        public StreamDataType(string type)
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
        public bool IsSubscribed => _callback != null;
        public bool IsRegistered { get; private set; } = true;

        private Connection _connection;
        private Action<RedisChannel, RedisValue> _callback;

        public Stream(Connection connection, string name, StreamType type, StreamDataType dataType)
        {
            Name = name;
            Type = type;
            DataType = dataType;

            _connection = connection;
        }

        public async void Publish(object data)
        {
            if (!IsRegistered) return;
            await _connection.Subscriber.PublishAsync(RedisChannel.Literal(Name), JsonConvert.SerializeObject(data));
        }

        public async Task<bool> Subscribe(Action<RedisChannel, RedisValue> callback)
        {
            _callback = callback;
            await _connection.Subscriber.SubscribeAsync(RedisChannel.Literal(Name), _callback);

            return true;
        }

        public async Task<bool> Unsubscribe()
        {
            await _connection.Subscriber.UnsubscribeAsync(RedisChannel.Literal(Name));
            _callback = null;

            return true;
        }

        public void Unregister()
        {
            IsRegistered = false;
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

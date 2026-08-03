using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;
using StackExchange.Redis;
using UnityEngine;

namespace AmplifyPortable
{
    public enum ConnectionType
    {
        Input,
        Output,
        Io
    }

    public class Connection : IDisposable
    {
        private const string ServiceRegistry = "available_streams";
        private const string DeviceRegistry = "connected_devices";
        private readonly ConnectionMultiplexer _redis;
        private readonly IDatabase _db;
        private readonly ISubscriber _sub;
        private readonly ConnectionType _type;
        private readonly object _deviceInfo;
        private readonly Guid _deviceId;
        private readonly CancellationTokenSource _cts = new();
        private readonly Task _heartbeatTask;
        private readonly Dictionary<string, Stream> _registeredStreams = new();

        public bool IsConnected => _redis.IsConnected;

        public Connection(string host, ConnectionType type, object deviceInfo = null)
        {
            _redis = ConnectionMultiplexer.Connect(host);
            _db = _redis.GetDatabase();
            _sub = _redis.GetSubscriber();
            _type = type;
            _deviceInfo = deviceInfo;
            _deviceId = Guid.NewGuid();

            UpdateHeartbeat();
            RunHeartbeatLoop();
        }

        public void Close()
        {
            Dispose();
        }

        public void Dispose()
        {
            _cts.Cancel();
            try
            {
                _heartbeatTask.Wait(TimeSpan.FromSeconds(1));
            }
            catch (AggregateException) { }

            foreach (var stream in new List<Stream>(_registeredStreams.Values))
            {
                try
                {
                    UnregisterStream(stream);
                }
                catch { }
            }

            try
            {
                _db.HashDelete(DeviceRegistry, _deviceId.ToString());
            }
            catch { }

            _redis.Dispose();
            _cts.Dispose();
        }

        public Stream RegisterStream(string name, StreamType type, StreamDataType dataType)
        {
            if (StreamExists(name))
            {
                throw new InvalidOperationException($"Stream with name {name} already exists");
            }

            var stream = new Stream(this, name, type, dataType);
            _registeredStreams[name] = stream;

            var streamInfo = stream.GetInfo();
            _db.HashSet(ServiceRegistry, name, JsonConvert.SerializeObject(streamInfo));

            return stream;
        }

        public bool UnregisterStream(Stream stream)
        {
            if (!_registeredStreams.Remove(stream.Name))
            {
                return false;
            }

            stream.Unregister();
            return _db.HashDelete(ServiceRegistry, stream.Name);
        }

        public Dictionary<string, Stream> GetAvailableStreams()
        {
            var streamProperties = _db.HashGetAll(ServiceRegistry);
            var streams = new Dictionary<string, Stream>();

            foreach (var property in streamProperties)
            {
                var name = property.Name.ToString();
                var streamInfo = JsonConvert.DeserializeObject<StreamInfo>(property.Value.ToString());

                if (streamInfo != null)
                {
                    streams[name] = new Stream(
                        this,
                        name,
                        Enum.Parse<StreamType>(streamInfo.Type ?? "Discrete", true),
                        Enum.Parse<StreamDataType>(streamInfo.DataType ?? "Number", true)
                    );
                }
            }

            return streams;
        }

        public Stream GetStream(string streamName)
        {
            var value = _db.HashGet(ServiceRegistry, streamName);
            if (value.IsNull)
            {
                throw new KeyNotFoundException($"Stream '{streamName}' does not exist");
            }

            var streamInfo = JsonConvert.DeserializeObject<StreamInfo>(value.ToString());
            if (streamInfo == null)
            {
                throw new InvalidOperationException($"Failed to deserialize stream info for '{streamName}'");
            }

            return new Stream(
                this,
                streamName,
                Enum.Parse<StreamType>(streamInfo.Type ?? "Discrete", true),
                Enum.Parse<StreamDataType>(streamInfo.DataType ?? "Number", true)
            );
        }

        public bool StreamExists(string streamName)
        {
            return _db.HashExists(ServiceRegistry, streamName);
        }

        internal void Publish(string channel, object data)
        {
            _db.Publish(RedisChannel.Literal(channel), JsonConvert.SerializeObject(data));
        }

        internal void Subscribe(string channel, Action<RedisChannel, RedisValue> handler)
        {
            _sub.Subscribe(RedisChannel.Literal(channel), handler);
        }

        internal void Unsubscribe(string channel)
        {
            _sub.Unsubscribe(RedisChannel.Literal(channel));
        }

        private void UpdateHeartbeat()
        {
            var info = new DeviceRegistrationInfo
            {
                Type = _type.ToString().ToLower(),
                DeviceInfo = _deviceInfo,
                LastSeen = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
            _db.HashSet(DeviceRegistry, _deviceId.ToString(), JsonConvert.SerializeObject(info));
        }

        private async Task RunHeartbeatLoop()
        {
            try
            {
                while (!_cts.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromSeconds(10), _cts.Token);

                    var info = new DeviceRegistrationInfo
                    {
                        Type = _type.ToString().ToLower(),
                        DeviceInfo = _deviceInfo,
                        LastSeen = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                    };

                    await _db.HashSetAsync(DeviceRegistry, _deviceId.ToString(), JsonConvert.SerializeObject(info));
                }
            }
            catch (OperationCanceledException)
            {
            }
        }
    }

    public class DeviceRegistrationInfo
    {
        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;

        [JsonProperty("deviceInfo")]
        public object? DeviceInfo { get; set; }

        [JsonProperty("lastSeen")]
        public long LastSeen { get; set; }
    }
}

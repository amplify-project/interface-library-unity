using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StackExchange.Redis;
using Newtonsoft.Json;
using AmplifyPortable.Util;

namespace AmplifyPortable.Redis
{
    public enum ConnectionType
    {
        Input,
        Output,
        Bidirectional
    };

    public class Connection
    {
        private const string ServiceRegistryKey = "available_streams";

        private readonly ConnectionMultiplexer _connection;
        private readonly IDatabase _redis;
        private readonly ISubscriber _subscriber;

        private Dictionary<string, Stream> _registeredStreams = new();
        private ConnectionType _connectionType;

        public bool IsConnected => _connection.IsConnected;
        public ISubscriber Subscriber => _subscriber;

        private Connection(ConnectionMultiplexer connection, ConnectionType connectionType)
        {
            _connection = connection;
            _redis = connection.GetDatabase();
            _subscriber = connection.GetSubscriber();
            _connectionType = connectionType;
        }

        public static async Task<Connection> ConnectAsync(string host = "localhost:6379", ConnectionType connectionType = ConnectionType.Input)
        {
            var connection = await ConnectionMultiplexer.ConnectAsync(host);
            return new Connection(connection, connectionType);
        }

        public void Close()
        {
            _connection.Close();
        }

        public async Task<bool> StreamExists(string streamName)
        {
            return await _redis.HashExistsAsync(ServiceRegistryKey, streamName);
        }

        public async Task<Stream> RegisterStream(string streamName, StreamType streamType, StreamDataType dataType)
        {
            if (await StreamExists(streamName))
            {
                throw new StreamExistsException(streamName);
            }

            var stream = new Stream(this, streamName, streamType, dataType);
            var success = await _redis.HashSetAsync(ServiceRegistryKey, streamName, JsonConvert.SerializeObject(stream.Serialize()));

            if (!success)
            {
                throw new StreamNotCreatedException(streamName);
            }

            _registeredStreams.Add(streamName, stream);
            return stream;
        }

        public async Task<bool> UnregisterStream(string streamName)
        {
            var success = await _redis.HashDeleteAsync(ServiceRegistryKey, streamName);

            if (success)
            {
                _registeredStreams.Remove(streamName);
            }

            return success;
        }

        public async Task<Dictionary<string, Stream>> GetAvailableStreams()
        {
            var streamProperties = await _redis.HashGetAllAsync(ServiceRegistryKey);
            var streams = new Dictionary<string, Stream>();

            foreach (var streamProperty in streamProperties)
            {
                var streamData = JsonConvert.DeserializeObject<SerializedStream>(streamProperty.Value);

                streams.Add(streamProperty.Name, new Stream(
                    this,
                    streamData.Name,
                    new StreamType(streamData.Type),
                    new StreamDataType(streamData.DataType)
                ));
            }

            return streams;
        }

        public async Task<Stream> GetStream(string streamName)
        {
            var streamProperties = await _redis.HashGetAsync(ServiceRegistryKey, streamName);

            if (!streamProperties.HasValue)
            {
                throw new StreamNotFoundException(streamName);
            }

            var streamData = JsonConvert.DeserializeObject<SerializedStream>(streamProperties);
            return new Stream(this, streamData.Name, new StreamType(streamData.Type), new StreamDataType(streamData.DataType));
        }
    }
}

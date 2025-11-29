using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StackExchange.Redis;

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
        private readonly ConnectionMultiplexer _connection;
        private ISubscriber _subscriber;

        private Dictionary<string, Stream> _registeredStreams = new();
        private ConnectionType _connectionType;

        public bool IsConnected => _connection.IsConnected;

        private Connection(ConnectionMultiplexer connection, ConnectionType connectionType)
        {
            _connection = connection;
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

        public void RegisterStream(string streamName)
        {
            _registeredStreams.Add(streamName, new Stream(streamName));
        }

        public void UnregisterStream(string streamName)
        {
            _registeredStreams.Remove(streamName);
        }
    }
}

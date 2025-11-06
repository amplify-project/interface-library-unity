using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StackExchange.Redis;
using UnityEngine;

namespace AmplifyPortable.Redis
{
    public class Connection
    {
        private readonly ConnectionMultiplexer _connection;
        private ISubscriber _subscriber;

        private Dictionary<string, Stream> _streams = new();

        #region events

        public event Action OnConnect;

        #endregion

        public bool IsConnected => _connection.IsConnected;

        private Connection(ConnectionMultiplexer connection)
        {
            _connection = connection;
        }

        public static async Task<Connection> Connect(string redisUrl)
        {
            var connection = await ConnectionMultiplexer.ConnectAsync(redisUrl);
            return new Connection(connection);
        }

        public void RegisterStream(string streamName)
        {
            _streams.Add(streamName, new Stream(streamName));
        }

        public void UnregisterStream(string streamName)
        {
            _streams.Remove(streamName);
        }
    }
}

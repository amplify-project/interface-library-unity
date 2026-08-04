using System;
using UnityEngine;
using AmplifyPortable;

public class AmplifySample : MonoBehaviour
{
    private Connection _connection;
    private Stream _stream;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        AmplifyPortableController.Instance.OnConnect += OnRedisConnected;
    }

    private void OnRedisConnected(Connection connection)
    {
        Debug.Log($"Connected to Redis: {connection.IsConnected}");
        _connection = connection;

        _stream = connection.RegisterStream("test", StreamType.Continuous, StreamDataType.Number);
        _stream.Subscribe<string>((message) => Debug.Log($"Received message: {message}"));

        var availableStreams = connection.GetAvailableStreams();
        var log = "Available streams:\n";

        foreach (var (k, v) in availableStreams)
        {
            log += $"\t{k} => type: {v.Type}, data_type: {v.DataType}";
        }
        Debug.Log(log);

        _stream.Publish("hello world");
        _stream.Publish("blag");

        _connection.UnregisterStream(_stream);

        try
        {
            _stream.Publish("this message will not arrive");
        }
        catch (InvalidOperationException) {}
    }

    public void OnDestroy()
    {
        Debug.Log("Cleaning up stream and closing connection...");

        AmplifyPortableController.Instance.OnConnect -= OnRedisConnected;
        _connection?.Close();
    }
}

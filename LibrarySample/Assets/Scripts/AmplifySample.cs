using UnityEngine;
using AmplifyPortable;
using AmplifyPortable.Redis;

public class AmplifySample : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        AmplifyPortableController.Instance.OnConnect += OnRedisConnected;
    }

    private async void OnRedisConnected(Connection connection)
    {
        Debug.Log($"Connected to Redis: {connection.IsConnected}");

        var stream = await connection.RegisterStream("test", StreamType.Continuous, StreamDataType.Number);
        await stream.Subscribe((channel, message) => Debug.Log($"Received message on {channel}: {message}"));

        var availableStreams = await connection.GetAvailableStreams();
        var log = "Available streams:\n";

        foreach (var (k, v) in availableStreams)
        {
            log += $"\t{k} => type: {v.Type}, data_type: {v.DataType}";
        }
        Debug.Log(log);

        stream.Publish("hello world");
        stream.Publish("blag");

        await connection.UnregisterStream(stream);

        stream.Publish("this message will not arrive");
    }
}

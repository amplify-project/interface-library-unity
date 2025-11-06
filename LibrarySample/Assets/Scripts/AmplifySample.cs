using UnityEngine;
using AmplifyPortable;
using AmplifyPortable.Redis;

public class AmplifySample : MonoBehaviour
{
    private Connection _connection;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        AmplifyPortableController.Instance.OnConnect += OnRedisConnected;
    }

    private void OnRedisConnected(Connection connection)
    {
        _connection = connection;
        Debug.Log($"Connected to Redis: {connection.IsConnected}");
    }
}

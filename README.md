# AMPLIFY Portable Interface - C#/Unity Library

This library is a C#/Unity implementation of the specification found in the
[Interface Spec](https://github.com/amplify-project/interface-library-spec) repository.
It provides a standardized way to interact with AMPLIFY-compatible devices and
services using a Redis-backed communication layer.

For the corresponding Python implementation, see the [interface-library-python](https://github.com/amplify-project/interface-library-python) repository.

## Purpose

The main goal of this library is to facilitate the connection, registration and
data exchange between different components of the AMPLIFY ecosystem within the
Unity engine. It abstracts the underlying Redis operations into a simple API
for managing data streams.

## Features

- **Connection Management**: Connect to Redis hosts and manage device
  registration.
- **Stream Registration**: Register and unregister data streams with metadata
  (type and data type).
- **Pub/Sub Support**: Subscribe to streams to receive real-time updates and
  publish data to streams.
- **Stream Types**:
    - `discrete`: For data that occurs at specific intervals or events.
    - `continuous`: For streaming data.
- **Data Types**: Support for `number`, `string`, and `boolean` values.
- **Unity Integration**: Includes an `AmplifyPortableController` component for
  easy setup and management within the Unity Editor.

## Requirements

- Unity >= 6000.0
- [Newtonsoft.Json](https://docs.unity3d.com/Packages/com.unity.nuget.newtonsoft-json@3.2/manual/index.html) package (com.unity.nuget.newtonsoft-json)
- Redis server

## Usage Example

### Using the Controller (Unity-way)

1. Drag the `AmplifyPortableController` prefab (found in `Runtime/Prefabs`) into your scene.
2. Configure the **Redis Url** and **Connection Type** in the Inspector.
3. Access the connection from your scripts:

```csharp
using System;
using AmplifyPortable;
using UnityEngine;

public class MyComponent : MonoBehaviour
{
    private void Start()
    {
        // Get the connection from the controller (ensure it's connected)
        var connection = AmplifyPortableController.Instance.Connection;

        if (connection != null)
        {
            // Register a new stream
            var stream = connection.RegisterStream(
                "sensor_data",
                StreamType.Continuous,
                StreamDataType.Number
            );

            // Publish data
            stream.Publish(23.5f);
        }
    }
}
```

### Manual Connection (Pure C#)

```csharp
using System;
using AmplifyPortable;
using StackExchange.Redis;
using UnityEngine;

// Connect to the Redis host
var connection = await Connection.ConnectAsync("localhost:6379", ConnectionType.Output);

// Register a new stream
var stream = connection.RegisterStream(
    "sensor_data",
    StreamType.Continuous,
    StreamDataType.Number
);

// Publish data
stream.Publish(23.5f);

// Subscribe to a stream
var otherStream = connection.GetStream("other_sensor");
otherStream.Subscribe<string>(value => {
    Debug.Log($"Received data: {value}");
});
```

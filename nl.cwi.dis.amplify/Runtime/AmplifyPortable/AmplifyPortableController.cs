using System;
using UnityEngine;
using AmplifyPortable.Redis;

namespace AmplifyPortable
{
    public class AmplifyPortableController : MonoBehaviour
    {
        public static AmplifyPortableController Instance { get; private set; }

        public string redisUrl = "localhost";
        public Connection Connection { get; private set; }

        public event Action<Connection> OnConnect;

        private async void Awake()
        {
            if (Instance)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            Connection = await Connection.Connect(redisUrl);
            OnConnect?.Invoke(Connection);
        }
    }
}

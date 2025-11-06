using UnityEngine;

namespace AmplifyPortable
{
    public class AmplifyPortableController : MonoBehaviour
    {
        public static AmplifyPortableController Instance { get; private set; }

        public string RedisUrl = "redis://localhost:6379";

        private void Awake()
        {
            if (Instance)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}

using UnityEngine;


namespace FantasyFin.Unity
{
    public class ffSingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {

        private static T _instance;
        private static readonly object _lock = new object();

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        _instance = FindObjectOfType<T>();

                        if (_instance == null)
                        {
                            GameObject singletonObject = new GameObject(typeof(T).Name);
                            _instance = singletonObject.AddComponent<T>();
                            DontDestroyOnLoad(singletonObject);
                        }
                    }
                }
                return _instance;
            }
        }


        public static InstanceType GetTypeInstance<InstanceType>() where InstanceType : MonoBehaviour
        {
            return Instance as InstanceType;
        }

        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                if (transform.parent == null)
                {
                    DontDestroyOnLoad(gameObject);
                }
            }
            else if (_instance != this)
            {
                Debug.LogWarning($"实例已存在，销毁多余的 {GetType().Name}");
                Destroy(gameObject);
            }

            Init();
        }

        protected virtual void Init()
        {

        }

    }
}


using UnityEngine;
namespace FantasyFin.Unity
{
    public class ffSingleton<T> where T : new()
    {
        
        private static readonly object _lock = new object();
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new T();
                            (_instance as ffSingleton<T>)?.OnCreated(); // 延迟初始化
                        }
                    }
                }
                return _instance;
            }
        }

        // 让构造函数为 protected 避免外部直接实例化
        protected ffSingleton() {}

        protected virtual void OnCreated() { Debug.Log($"Instance of type {typeof(T).Name} created."); }
        public virtual void WakeUp()
        {

        }
       
    }
}

using UnityEngine;

namespace _Scripts.UnityServiceLocator
{
    public abstract class GlobalService<T> : MonoBehaviour where T : class
    {
        private void Awake()
        {
            if (ServiceLocator.Global.TryGet<T>(out _))
            {
                Destroy(gameObject);
                return;
            }

            ServiceLocator.Global.Register(this as T);
            DontDestroyOnLoad(gameObject);
        }
    }
}
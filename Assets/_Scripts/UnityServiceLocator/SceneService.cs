using UnityEngine;

namespace _Scripts.UnityServiceLocator
{
    public abstract class SceneService<T> : MonoBehaviour where T : class
    {
        private void Awake()
        {
            ServiceLocator.ForSceneOf(this).Register(this as T);
        }
    }
}
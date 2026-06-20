using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Scripts.UnityServiceLocator
{
    public class ServiceLocator : MonoBehaviour
    {
        private static ServiceLocator s_Global;
        private static Dictionary<Scene, ServiceLocator> s_SceneContainers;
        private static List<GameObject> s_TempSceneGameObjects;

        private readonly ServiceManager m_Services = new ServiceManager();

        private const string k_GlobalServiceLocatorName = "ServiceLocator [Global]";
        private const string k_SceneServiceLocatorName = "ServiceLocator [Scene]";

        internal void ConfigureAsGlobal(bool dontDestroyOnLoad)
        {
            if (s_Global == this)
            {
                Debug.LogWarning("ServiceLocator.ConfigureAsGlobal: Already configured as global", this);
            }
            else if (s_Global != null)
            {
                Debug.LogError(
                    "ServiceLocator.ConfigureAsGlobal: Another ServiceLocator is already configured as global", this);
            }
            else
            {
                s_Global = this;
                if (dontDestroyOnLoad)
                {
                    DontDestroyOnLoad(gameObject);
                }
            }
        }

        internal void ConfigureForScene()
        {
            var scene = gameObject.scene;

            if (s_SceneContainers.ContainsKey(scene))
            {
                Debug.LogError(
                    "ServiceLocator.ConfigureForScene: Another ServiceLocator is already configured for this scene",
                    this);
                return;
            }

            s_SceneContainers.Add(scene, this);
        }

        /// <summary>
        /// `global`な`ServiceLocator`のインスタンスを取得する。
        /// ない場合は作成する
        /// </summary>
        public static ServiceLocator Global
        {
            get
            {
                if (s_Global != null)
                {
                    return s_Global;
                }

                if (FindAnyObjectByType<ServiceLocatorGlobal>() is { } found)
                {
                    found.BootstrapOnDemand();
                    return s_Global;
                }

                var container = new GameObject(k_GlobalServiceLocatorName, typeof(ServiceLocator));
                container.AddComponent<ServiceLocatorGlobal>().BootstrapOnDemand();

                return s_Global;
            }
        }

        public static ServiceLocator ForSceneOf(MonoBehaviour monoBehaviour)
        {
            var scene = monoBehaviour.gameObject.scene;

            if (s_SceneContainers.TryGetValue(scene, out ServiceLocator container) && container != monoBehaviour)
            {
                return container;
            }

            s_TempSceneGameObjects.Clear();
            scene.GetRootGameObjects(s_TempSceneGameObjects);

            foreach (GameObject go in
                     s_TempSceneGameObjects.Where(go => go.GetComponent<ServiceLocatorScene>() != null))
            {
                if (go.TryGetComponent(out ServiceLocatorScene bootstrapper) && bootstrapper.Container != monoBehaviour)
                {
                    bootstrapper.BootstrapOnDemand();
                    return bootstrapper.Container;
                }
            }

            return s_Global;
        }

        public static ServiceLocator For(MonoBehaviour monoBehaviour)
        {
            return monoBehaviour.GetComponentInParent<ServiceLocator>().OrNull() ??
                   ForSceneOf(monoBehaviour) ?? s_Global;
        }

        public ServiceLocator Register<T>(T service) where T : class
        {
            m_Services.Register(service);
            return this;
        }

        public T Get<T>() where T : class
        {
            T service = null;

            if (TryGetService(out service))
            {
                return service;
            }

            if (TryGetNextInHierarchy(out var container))
            {
                return container.Get<T>();
            }

            throw new ArgumentException($"Could not resolve type '{typeof(T).FullName}'.");
        }

        public bool TryGet<T>(out T service) where T : class
        {
            var type = typeof(T);
            service = null;

            if (TryGetService(out service))
            {
                return true;
            }

            return TryGetNextInHierarchy(out ServiceLocator container) && container.TryGet(out service);
        }

        private bool TryGetService<T>(out T service) where T : class
        {
            return m_Services.TryGet(out service);
        }

        private bool TryGetNextInHierarchy(out ServiceLocator container)
        {
            if (this == s_Global)
            {
                container = null;
                return false;
            }

            container = transform.parent.OrNull()?.GetComponentInParent<ServiceLocator>().OrNull() ?? ForSceneOf(this);
            return container != null;
        }

        private void OnDestroy()
        {
            if (this == s_Global)
            {
                s_Global = null;
            }
            else if (s_SceneContainers.ContainsValue(this))
            {
                s_SceneContainers.Remove(gameObject.scene);
            }
        }

        // https://docs.unity3d.com/ScriptReference/RuntimeInitializeOnLoadMethodAttribute.html
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            s_Global = null;
            s_SceneContainers = new Dictionary<Scene, ServiceLocator>();
            s_TempSceneGameObjects = new List<GameObject>();
        }

#if UNITY_EDITOR
        [MenuItem("GameObject/ServiceLocator/Add Global")]
        private static void AddGlobal()
        {
            var go = new GameObject(k_GlobalServiceLocatorName, typeof(ServiceLocatorGlobal));
        }

        [MenuItem("GameObject/ServiceLocator/Add Scene")]
        private static void AddScene()
        {
            var go = new GameObject(k_SceneServiceLocatorName, typeof(ServiceLocatorScene));
        }
#endif
    }
}
using System.Collections.Generic;
using _Scripts.UnityServiceLocator;
using UnityEngine;

namespace _Scripts.Service
{
    /// <summary>
    /// 文字列キーと型引数を使用して、オブジェクトのプールを管理するサービス。
    /// </summary>
    public class ObjectPoolService : SceneService<ObjectPoolService>
    {
        private readonly Dictionary<string, object> m_Pools = new Dictionary<string, object>();

        /// <summary>
        /// 新しいオブジェクトプールを作成する。
        /// </summary>
        /// <typeparam name="T">プールの対象となるコンポーネント型</typeparam>
        /// <param name="key">プールを識別するための文字列キー</param>
        /// <param name="prefab">生成するオブジェクトのPrefab</param>
        /// <param name="initialSize">初期生成数</param>
        public void CreatePool<T>(string key, T prefab, int initialSize = 8) where T : Component
        {
            if (m_Pools.ContainsKey(key))
            {
                // FIXME? 現状自由に呼びたいので、警告が出るのは面倒
                // Debug.LogWarning($"ObjectPoolService: Pool with key '{key}' already exists.");
                return;
            }

            var pool = new Pool<T>(prefab, transform, initialSize);
            m_Pools.Add(key, pool);
        }

        /// <summary>
        /// 指定したキーのプールからオブジェクトを取得する。
        /// </summary>
        /// <typeparam name="T">取得するコンポーネント型</typeparam>
        /// <param name="key">プールを識別するための文字列キー</param>
        /// <returns>取得したインスタンス</returns>
        public T Get<T>(string key) where T : Component
        {
            if (m_Pools.TryGetValue(key, out var poolObj) && poolObj is Pool<T> pool)
            {
                return pool.Get();
            }

            Debug.LogError($"ObjectPoolService: Pool with key '{key}' of type '{typeof(T).Name}' not found.");
            return null;
        }

        /// <summary>
        /// オブジェクトをプールに戻す。
        /// </summary>
        /// <typeparam name="T">戻すコンポーネント型</typeparam>
        /// <param name="key">プールを識別するための文字列キー</param>
        /// <param name="instance">プールに戻すインスタンス</param>
        public void Return<T>(string key, T instance) where T : Component
        {
            if (m_Pools.TryGetValue(key, out var poolObj) && poolObj is Pool<T> pool)
            {
                pool.Return(instance);
            }
            else
            {
                Debug.LogWarning($"ObjectPoolService: Could not return object to pool '{key}'. Destroying instead.");
                Destroy(instance.gameObject);
            }
        }

        /// <summary>
        /// 内部的なプール管理クラス
        /// </summary>
        private class Pool<T> where T : Component
        {
            private readonly T m_Prefab;
            private readonly Transform m_Parent;
            private readonly Stack<T> m_PoolStack = new Stack<T>();

            public Pool(T prefab, Transform parent, int initialSize)
            {
                m_Prefab = prefab;
                m_Parent = parent;

                for (int i = 0; i < initialSize; i++)
                {
                    CreateNewInstance();
                }
            }

            private T CreateNewInstance()
            {
                T instance = Object.Instantiate(m_Prefab, m_Parent);
                instance.gameObject.SetActive(false);
                m_PoolStack.Push(instance);
                return instance;
            }

            public T Get()
            {
                T instance = m_PoolStack.Count > 0 ? m_PoolStack.Pop() : Object.Instantiate(m_Prefab, m_Parent);
                instance.gameObject.SetActive(true);
                return instance;
            }

            public void Return(T instance)
            {
                instance.gameObject.SetActive(false);
                instance.transform.SetParent(m_Parent);
                m_PoolStack.Push(instance);
            }
        }
    }
}
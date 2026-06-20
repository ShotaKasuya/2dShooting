using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.UnityServiceLocator
{
    public class ServiceManager
    {
        private readonly Dictionary<Type, object> m_Services = new Dictionary<Type, object>();
        public IEnumerable<object> RegisteredServices => m_Services.Values;

        public bool TryGet<T>(out T service) where T : class
        {
            Type type = typeof(T);
            if (m_Services.TryGetValue(type, out object obj))
            {
                service = obj as T;
                return true;
            }

            service = null;
            return false;
        }

        public T Get<T>() where T : class
        {
            var type = typeof(T);
            if (m_Services.TryGetValue(type, out var obj))
            {
                return obj as T;
            }

            throw new ArgumentException($"ServiceManager.Get: Service of type {type.FullName} not registered");
        }

        public ServiceManager Register<T>(T service) where T:class
        {
            var type = typeof(T);

            if (!m_Services.TryAdd(type,service))
            {
                Debug.LogError($"ServiceManager.Register: Service of type {type.FullName} already registered");
            }

            return this;
        }

        public ServiceManager Register(Type type, object service)
        {
            if (!type.IsInstanceOfType(service))
            {
                throw new ArgumentException("Type of service does not match type of service interface",
                    nameof(service));
            }

            if (!m_Services.TryAdd(type, service))
            {
                Debug.LogError($"ServiceManager.Register: Service of type {type.FullName} already registered");
            }

            return this;
        }
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GTAFramework.Core.Services
{
    public class ServiceLocator
    {
        private static ServiceLocator _instance;
        public static ServiceLocator Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ServiceLocator();
                return _instance;
            }
        }

        private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        public void RegisterService<T>(T service) where T : class
        {
            Type type = typeof(T);

            if (_services.ContainsKey(type))
            {
                Debug.LogWarning($"Service of type {type.Name} already registered. Overwriting...");
                _services[type] = service;
            }
            else
            {
                _services.Add(type, service);
                Debug.Log($"Service {type.Name} registered successfully.");
            }
        }

        public T GetService<T>() where T : class
        {
            Type type = typeof(T);

            if (_services.TryGetValue(type, out object service))
            {
                return service as T;
            }

            Debug.LogError($"Service of type {type.Name} not found!");
            return null;
        }

        public bool HasService<T>() where T : class
        {
            return _services.ContainsKey(typeof(T));
        }

        public void UnregisterService<T>() where T : class
        {
            Type type = typeof(T);

            if (_services.ContainsKey(type))
            {
                _services.Remove(type);
                Debug.Log($"Service {type.Name} unregistered.");
            }
        }

        public void Clear()
        {
            _services.Clear();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using GTAFramework.Core.Interfaces;

namespace GTAFramework.Core.Container
{
    /// <summary>
    /// Contenedor DI ligero para Unity:
    /// - Registro de singletons y factories
    /// - Auto-descubrimiento de sistemas con [AutoRegister]
    /// - Inyección por reflexión con [Inject]
    /// - Tick/LateTick/FixedTick y Shutdown centralizados
    /// </summary>
    public sealed class DIContainer : IDisposable
    {
        private static DIContainer _instance;
        public static DIContainer Instance => _instance ??= new DIContainer();

        private readonly Dictionary<Type, object> _services = new();
        private readonly Dictionary<Type, Func<object>> _factories = new();
        private readonly List<IGameSystem> _systems = new();

        private bool _disposed;
        private bool _systemsDiscovered;
        private bool _systemsInitialized;

        private DIContainer() { }

        // =========================
        // Registro / Resolución
        // =========================

        public void RegisterSingleton<T>(T instance)
        {
            _services[typeof(T)] = instance;
        }

        public void RegisterSingleton<TInterface, TImplementation>(TImplementation instance)
            where TImplementation : TInterface
        {
            _services[typeof(TInterface)] = instance;
        }

        public void RegisterFactory<T>(Func<T> factory)
        {
            _factories[typeof(T)] = () => factory();
        }

        public bool IsRegistered<T>()
        {
            var t = typeof(T);
            return _services.ContainsKey(t) || _factories.ContainsKey(t);
        }

        public T Resolve<T>() => (T)Resolve(typeof(T));

        public object Resolve(Type type)
        {
            if (type == null) return null;

            if (_services.TryGetValue(type, out var instance))
                return instance;

            if (_factories.TryGetValue(type, out var factory))
                return factory();

            Debug.LogError($"[DIContainer] Service of type {type.Name} not found!");
            return null;
        }

        // =========================
        // Auto-descubrimiento
        // =========================

        /// <summary>
        /// Encuentra e instancia todos los IGameSystem con [AutoRegister].
        /// Respeta Priority y StartActive.
        /// </summary>
        public List<IGameSystem> DiscoverAndRegisterSystems()
        {
            if (_systemsDiscovered)
                return _systems;

            var systemTypes = FindTypesWithAttribute<AutoRegisterAttribute>()
                .Where(t => typeof(IGameSystem).IsAssignableFrom(t) && !t.IsAbstract && t.GetConstructor(Type.EmptyTypes) != null)
                .Select(t => new { Type = t, Attr = t.GetCustomAttribute<AutoRegisterAttribute>() })
                .OrderBy(x => x.Attr.Priority)
                .ToList();

            foreach (var item in systemTypes)
            {
                try
                {
                    var system = CreateAndInjectSystem(item.Type);
                    if (system == null)
                        continue;

                    // Respetar StartActive del atributo
                    system.IsActive = item.Attr.StartActive;

                    _systems.Add(system);
                    Debug.Log($"[DIContainer] Auto-registered system: {item.Type.Name} (Priority: {item.Attr.Priority}, Active: {item.Attr.StartActive})");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[DIContainer] Failed to create system {item.Type.Name}: {ex}");
                }
            }

            _systemsDiscovered = true;
            return _systems;
        }

        private IGameSystem CreateAndInjectSystem(Type systemType)
        {
            var system = Activator.CreateInstance(systemType) as IGameSystem;
            if (system == null)
                return null;

            InjectDependencies(system);
            return system;
        }

        // =========================
        // Inyección
        // =========================

        public void InjectDependencies(object target)
        {
            if (target == null) return;

            var type = target.GetType();

            // Campos (public + private)
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                if (field.GetCustomAttribute<InjectAttribute>() == null)
                    continue;

                var service = Resolve(field.FieldType);
                if (service != null)
                    field.SetValue(target, service);
            }

            // Propiedades (public + private)
            var props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var prop in props)
            {
                if (prop.GetCustomAttribute<InjectAttribute>() == null)
                    continue;

                if (!prop.CanWrite)
                    continue;

                var service = Resolve(prop.PropertyType);
                if (service != null)
                    prop.SetValue(target, service);
            }
        }

        // =========================
        // Lifecycle de sistemas
        // =========================

        public void InitializeAllSystems()
        {
            if (_systemsInitialized)
                return;

            foreach (var system in _systems)
            {
                try
                {
                    system.Initialize();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[DIContainer] Failed to initialize {system.GetType().Name}: {ex}");
                }
            }

            _systemsInitialized = true;
        }

        public void Tick(float deltaTime)
        {
            for (int i = 0; i < _systems.Count; i++)
            {
                var s = _systems[i];
                if (s.IsActive) s.Tick(deltaTime);
            }
        }

        public void LateTick(float deltaTime)
        {
            for (int i = 0; i < _systems.Count; i++)
            {
                var s = _systems[i];
                if (s.IsActive) s.LateTick(deltaTime);
            }
        }

        public void FixedTick(float fixedDeltaTime)
        {
            for (int i = 0; i < _systems.Count; i++)
            {
                var s = _systems[i];
                if (s.IsActive) s.FixedTick(fixedDeltaTime);
            }
        }

        public void ShutdownAll()
        {
            for (int i = 0; i < _systems.Count; i++)
            {
                var s = _systems[i];
                try { s.Shutdown(); }
                catch (Exception ex)
                {
                    Debug.LogError($"[DIContainer] Failed to shutdown {s.GetType().Name}: {ex}");
                }
            }

            _systems.Clear();
            _services.Clear();
            _factories.Clear();

            _systemsDiscovered = false;
            _systemsInitialized = false;
        }

        public T GetSystem<T>() where T : class, IGameSystem
        {
            return _systems.OfType<T>().FirstOrDefault();
        }

        public IEnumerable<IGameSystem> GetAllSystems() => _systems.AsReadOnly();

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            ShutdownAll();
            _instance = null;
        }

        // =========================
        // Helpers reflection
        // =========================

        private static IEnumerable<Type> FindTypesWithAttribute<TAttribute>() where TAttribute : Attribute
        {
            var result = new List<Type>();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                var assembly = assemblies[i];

                // Filtrar (reduce costo): proyecto normalmente está en Assembly-CSharp / GTAFramework
                var name = assembly.GetName().Name;
                if (name != "Assembly-CSharp" && (name == null || !name.StartsWith("GTAFramework")))
                    continue;

                try
                {
                    var types = assembly.GetTypes();
                    for (int t = 0; t < types.Length; t++)
                    {
                        var type = types[t];
                        if (type.GetCustomAttribute<TAttribute>() != null)
                            result.Add(type);
                    }
                }
                catch (ReflectionTypeLoadException e)
                {
                    // Cargar lo que se pueda sin reventar
                    foreach (var type in e.Types)
                    {
                        if (type == null) continue;
                        if (type.GetCustomAttribute<TAttribute>() != null)
                            result.Add(type);
                    }
                }
                catch
                {
                    // Ignorar assemblies problemáticos
                }
            }

            return result;
        }
    }
}

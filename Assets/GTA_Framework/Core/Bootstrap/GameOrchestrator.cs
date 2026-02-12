using UnityEngine;
using GTAFramework.Core.Container;
using GTAFramework.Core.Interfaces;
using GTAFramework.Core.Services;
using System.Linq;

namespace GTAFramework.Core.Bootstrap
{
    /// <summary>
    /// Orchestrator con DI Container.
    /// Sistemas se auto-descubren con [AutoRegister] y reciben servicios con [Inject].
    /// </summary>
    public class GameOrchestrator : MonoBehaviour
    {
        private static GameOrchestrator _instance;
        public static GameOrchestrator Instance => _instance;

        private DIContainer _container;

        [Header("Debug")]
        [SerializeField] private bool _showDebugLogs = true;

        [Header("Systems")]
        [SerializeField] private bool _autoDiscoverSystems = true;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeFramework();
        }

        private void InitializeFramework()
        {
            if (_showDebugLogs)
                Debug.Log("=== Initializing GTA Framework (DI) ===");

            _container = DIContainer.Instance;

            RegisterCoreServices();

            if (_autoDiscoverSystems)
            {
                if (_showDebugLogs)
                    Debug.Log("[GameOrchestrator] Auto-discovering systems via [AutoRegister]...");

                _container.DiscoverAndRegisterSystems();
            }
            else
            {
                if (_showDebugLogs)
                    Debug.LogWarning("[GameOrchestrator] Auto-discovery disabled. No systems will be registered.");
            }

            _container.InitializeAllSystems();

            if (_showDebugLogs)
                Debug.Log($"=== GTA Framework Initialized ({_container.GetAllSystems().Count()} systems) ===");
        }

        private void RegisterCoreServices()
        {
            // InputService (singleton)
            var inputService = new InputService();
            inputService.Initialize();

            // Registrar por interfaz y por tipo concreto
            _container.RegisterSingleton<IService, InputService>(inputService);
            _container.RegisterSingleton<InputService>(inputService);

            if (_showDebugLogs)
                Debug.Log("[GameOrchestrator] Core services registered: InputService");
        }

        private void Update()
        {
            _container?.Tick(Time.deltaTime);
        }

        private void LateUpdate()
        {
            _container?.LateTick(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            _container?.FixedTick(Time.fixedDeltaTime);
        }

        private void OnDestroy()
        {
            // Limpieza completa del framework
            _container?.Dispose();
        }

        // ===== API Pública =====

        public T GetSystem<T>() where T : class, IGameSystem
        {
            return _container?.GetSystem<T>();
        }

        public T GetService<T>()
        {
            return _container != null ? _container.Resolve<T>() : default;
        }

        public DIContainer Container => _container;
    }
}

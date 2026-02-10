using UnityEngine;
using GTAFramework.Core.Services;
using GTAFramework.Core.Systems;
using GTAFramework.Player.Systems;
using GTAFramework.GTACamera.Systems;
using GTAFramework.GTA_Animation.Systems;

namespace GTAFramework.Core.Bootstrap
{
    public class GameOrchestrator : MonoBehaviour
    {
        private static GameOrchestrator _instance;
        public static GameOrchestrator Instance => _instance;

        private SystemsManager _systemsManager;
        private InputService _inputService;

        [Header("Debug")]
        [SerializeField] private bool _showDebugLogs = true;

        private void Awake()
        {
            // Singleton pattern
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
                Debug.Log("=== Initializing GTA Framework ===");

            // 1. Inicializar Service Locator
            InitializeServices();

            // 2. Inicializar Systems Manager
            _systemsManager = new SystemsManager();

            // 3. Registrar sistemas
            RegisterSystems();

            if (_showDebugLogs)
                Debug.Log("=== GTA Framework Initialized ===");
        }

        private void InitializeServices()
        {
            // Crear e inicializar InputService
            _inputService = new InputService();
            _inputService.Initialize();

            // Registrar en el Service Locator
            ServiceLocator.Instance.RegisterService(_inputService);
        }

        private void RegisterSystems()
        {
            // Registrar PlayerMovementSystem
            var playerMovementSystem = new PlayerMovementSystem();
            _systemsManager.RegisterSystem(playerMovementSystem);

            // 2) Animation (consume state -> animator params)
            var animationSystem = new AnimationSystem();
            _systemsManager.RegisterSystem(animationSystem);

            // Registrar CameraSystem
            var cameraSystem = new CameraSystem();
            _systemsManager.RegisterSystem(cameraSystem);
        }

        private void Update()
        {
            _systemsManager?.Tick(Time.deltaTime);
        }

        private void LateUpdate()
        {
            _systemsManager?.LateTick(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            _systemsManager?.FixedTick(Time.fixedDeltaTime);
        }

        private void OnDestroy()
        {
            _systemsManager?.ShutdownAll();
            _inputService?.Shutdown();
            ServiceLocator.Instance.Clear();
        }

        // Métodos públicos para acceso externo
        public T GetSystem<T>() where T : class, GTAFramework.Core.Interfaces.IGameSystem
        {
            return _systemsManager.GetSystem<T>();
        }
    }
}
using UnityEngine;
using GTAFramework.GTA_Animation.Data;
using GTAFramework.GTA_Animation.Modules;

namespace GTAFramework.GTA_Animation.Components
{
    [DisallowMultipleComponent]
    public sealed class CharacterAnimationAgent : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Animator _animator;
        [SerializeField] private CharacterAnimationConfig _config;

        [Header("Data Source (Gameplay)")]
        [SerializeField] private MonoBehaviour _sourceBehaviour;

        private ICharacterAnimationSource _source;

        private AnimatorDriver _driver;
        private AnimatorParamIds _ids;

        private IAnimationModule _stance;
        private IAnimationModule _airborne;
        private IAnimationModule _locomotion;

        private AnimationBlackboard _bb;

        public CharacterAnimationConfig Config => _config;
        public AnimatorDriver Driver => _driver;
        public AnimationBlackboard Blackboard => _bb;

        private void Awake()
        {
            if (_animator == null)
                _animator = GetComponentInChildren<Animator>(true);

            ResolveSource();
            BuildRuntime();
        }

        private void OnValidate()
        {
            if (_animator == null)
                _animator = GetComponentInChildren<Animator>(true);
        }

        private void ResolveSource()
        {
            if (_sourceBehaviour != null && _sourceBehaviour is ICharacterAnimationSource typed)
            {
                _source = typed;
                return;
            }

            var comps = GetComponents<MonoBehaviour>();
            for (int i = 0; i < comps.Length; i++)
            {
                if (comps[i] is ICharacterAnimationSource s)
                {
                    _source = s;
                    _sourceBehaviour = comps[i];
                    return;
                }
            }

            _source = null;
        }

        private void BuildRuntime()
        {
            if (_config == null)
            {
                Debug.LogWarning($"[{nameof(CharacterAnimationAgent)}] Missing config on {name}.");
                return;
            }

            if (_animator == null)
            {
                Debug.LogWarning($"[{nameof(CharacterAnimationAgent)}] Missing Animator on {name}.");
                return;
            }

            _ids = new AnimatorParamIds(_config);
            _driver = new AnimatorDriver(_animator, _ids);

            _stance = new StanceModule();
            _airborne = new AirborneModule();
            _locomotion = new LocomotionModule();

            _stance.Initialize(this);
            _airborne.Initialize(this);
            _locomotion.Initialize(this);

            _bb = new AnimationBlackboard();
        }

        public void Tick(float dt)
        {
            if (_config == null || _animator == null || _driver == null)
                return;

            if (_source == null)
            {
                ResolveSource();
                if (_source == null)
                {
                    if (_config.logWarnings)
                        Debug.LogWarning($"[{nameof(CharacterAnimationAgent)}] No ICharacterAnimationSource found on {name}.");
                    return;
                }
            }

            _driver.BeginFrame();
            BuildBlackboard(dt);

            _stance.Tick(dt, ref _bb, _driver);
            _airborne.Tick(dt, ref _bb, _driver);
            _locomotion.Tick(dt, ref _bb, _driver);
        }

        public void LateTick(float dt)
        {
            if (_config == null || _animator == null || _driver == null)
                return;

            _stance.LateTick(dt, ref _bb, _driver);
            _airborne.LateTick(dt, ref _bb, _driver);
            _locomotion.LateTick(dt, ref _bb, _driver);

            _bb.ResetOneFrameFlags();
        }

        private void BuildBlackboard(float dt)
        {
            _bb.deltaTime = dt;

            _bb.velocity = _source.Velocity;
            _bb.isGrounded = _source.IsGrounded;
            _bb.isCrouching = _source.IsCrouching;
            _bb.verticalSpeed = _source.VerticalSpeed;
            _bb.isMovementLocked = _source.IsMovementLocked;

            _bb.jumpPressedThisFrame = _source.ConsumeJumpPressedThisFrame();

            if (_bb.isGrounded)
            {
                _bb.timeSinceGrounded = 0f;
                _bb.timeInAir = 0f;
            }
            else
            {
                _bb.timeSinceGrounded += dt;
                _bb.timeInAir += dt;
            }
        }

        public interface ICharacterAnimationSource
        {
            Vector3 Velocity { get; }
            bool IsGrounded { get; }
            bool IsCrouching { get; }
            bool IsMovementLocked { get; }

            float VerticalSpeed { get; }

            // Bool one-frame desde gameplay (sin trigger de Animator)
            bool ConsumeJumpPressedThisFrame();
        }
    }
}

using System.Collections.Generic;
using UnityEngine;
using GTAFramework.GTA_Animation.Data;

namespace GTAFramework.GTA_Animation.Components
{
    /// <summary>
    /// Capa fina para escribir parámetros al Animator de manera segura y escalable.
    /// - Hashes cacheados
    /// - Damping para floats
    /// - Protecciones contra trigger-spam
    /// </summary>
    public sealed class AnimatorDriver
    {
        private readonly Animator _animator;
        private readonly AnimatorParamIds _ids;

        // Para damping manual: Animator.SetFloat(hash, value, dampTime, deltaTime)
        // Para triggers: evitamos re-disparar en el mismo frame / cooldown.
        private readonly HashSet<int> _triggersFiredThisFrame = new HashSet<int>(8);
        private readonly Dictionary<int, float> _triggerCooldownUntil = new Dictionary<int, float>(8);

        public Animator Animator => _animator;
        public AnimatorParamIds Ids => _ids;

        public AnimatorDriver(Animator animator, AnimatorParamIds ids)
        {
            _animator = animator;
            _ids = ids;
        }

        public void BeginFrame()
        {
            _triggersFiredThisFrame.Clear();
        }

        public void SetBool(int id, bool value)
        {
            if (_animator == null) return;
            _animator.SetBool(id, value);
        }

        public void SetFloat(int id, float value)
        {
            if (_animator == null) return;
            _animator.SetFloat(id, value);
        }

        public void SetFloatDamped(int id, float value, float dampTime, float deltaTime)
        {
            if (_animator == null) return;
            _animator.SetFloat(id, value, dampTime, deltaTime);
        }

        public bool TrySetTrigger(int id, float cooldownSeconds)
        {
            if (_animator == null) return false;

            // No 2 triggers del mismo id en el mismo frame
            if (_triggersFiredThisFrame.Contains(id))
                return false;

            // Cooldown (por ejemplo para evitar spam)
            float now = Time.time;
            if (_triggerCooldownUntil.TryGetValue(id, out float until) && now < until)
                return false;

            _animator.SetTrigger(id);
            _triggersFiredThisFrame.Add(id);

            if (cooldownSeconds > 0f)
                _triggerCooldownUntil[id] = now + cooldownSeconds;

            return true;
        }
    }
}

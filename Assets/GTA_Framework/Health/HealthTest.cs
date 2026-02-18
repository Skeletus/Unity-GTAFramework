using GTAFramework.Health.Components;
using UnityEngine;
using UnityEngine.InputSystem;

public class HealthTest : MonoBehaviour
{
    [SerializeField] private InputAction _damageAction;
    [SerializeField] private InputAction _healAction;
    [SerializeField] private float _damageAmount = 25f;
    [SerializeField] private float _healAmount = 30f;

    private HealthComponent _health;

    private void Awake()
    {
        _health = GetComponent<HealthComponent>();
        _damageAction.performed += ctx => _health?.TakeDamage(_damageAmount);
        _healAction.performed += ctx => _health?.Heal(_healAmount);
    }

    private void OnEnable()
    {
        _damageAction.Enable();
        _healAction.Enable();
    }

    private void OnDisable()
    {
        _damageAction.Disable();
        _healAction.Disable();
    }
}

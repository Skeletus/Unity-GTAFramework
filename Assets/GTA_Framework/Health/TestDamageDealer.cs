using UnityEngine;
using UnityEngine.InputSystem;
using GTAFramework.Health.Data;
using GTAFramework.Health.Components;

public class TestDamageDealer : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private float damageAmount = 25f;
    [SerializeField] private DamageType damageType = DamageType.Bullet;

    [Header("Input")]
    [SerializeField] private InputAction damageAction;

    private HealthComponent _target;

    private void Awake()
    {
        damageAction.performed += OnDamageInput;
    }

    private void OnEnable()
    {
        damageAction.Enable();
    }

    private void OnDisable()
    {
        damageAction.Disable();
    }

    private void OnDestroy()
    {
        damageAction.performed -= OnDamageInput;
    }

    private void Start()
    {
        _target = FindObjectOfType<HealthComponent>();
    }

    private void OnDamageInput(InputAction.CallbackContext context)
    {
        if (_target != null)
        {
            DamageInfo damage = new DamageInfo
            {
                Amount = damageAmount,
                Type = damageType,
                Source = gameObject
            };
            _target.TakeDamage(damage);
            Debug.Log($"Dealt {damageAmount} damage. Health: {_target.CurrentHealth}");
        }
    }
}
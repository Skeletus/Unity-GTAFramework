using UnityEngine;
using GTAFramework.Weapons.Data;

namespace GTAFramework.Weapons.Components
{
    /// <summary>
    /// Pickup interactuable de arma.
    /// Se registra en el WeaponInteractor del jugador cuando entra en el trigger.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class WeaponPickup : MonoBehaviour
    {
        [Header("Weapon Settings")]
        [SerializeField] private WeaponData _weaponData;

        [Header("Pickup Settings")]
        [SerializeField] private string _receiverTag = "Player";
        [SerializeField] private float _respawnTime = 0f; // 0 = no respawnea

        [Header("Visuals")]
        [SerializeField] private GameObject _visualModel;
        [SerializeField] private bool _rotateItem = true;
        [SerializeField] private float _rotationSpeed = 90f;

        private Collider _collider;
        private bool _isPickedUp;

        public bool IsAvailable => !_isPickedUp && _weaponData != null;

        private void Awake()
        {
            _collider = GetComponent<Collider>();
            _collider.isTrigger = true;
        }

        private void Update()
        {
            if (_rotateItem && !_isPickedUp && _visualModel != null)
            {
                _visualModel.transform.Rotate(0f, _rotationSpeed * Time.deltaTime, 0f);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsAvailable)
                return;

            if (!other.CompareTag(_receiverTag))
                return;

            var interactor = other.GetComponentInParent<WeaponInteractor>();
            if (interactor != null)
                interactor.RegisterPickup(this);

            // Auto-pickup solo si el jugador no tiene un arma de este tipo.
            var inventory = other.GetComponentInParent<WeaponInventory>();
            if (inventory == null)
                return;

            if (!inventory.HasWeaponType(_weaponData.type))
            {
                bool received = inventory.TryAddOrReplace(_weaponData);
                if (received)
                {
                    if (interactor != null)
                        interactor.UnregisterPickup(this);

                    OnPickedUp();
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag(_receiverTag))
                return;

            var interactor = other.GetComponentInParent<WeaponInteractor>();
            if (interactor != null)
                interactor.UnregisterPickup(this);
        }

        /// <summary>
        /// Intenta ser recogida por un WeaponInventory.
        /// </summary>
        public bool TryPickup(WeaponInventory inventory)
        {
            if (!IsAvailable || inventory == null)
                return false;

            bool received = inventory.TryAddOrReplace(_weaponData);
            if (received)
                OnPickedUp();

            return received;
        }

        private void OnPickedUp()
        {
            _isPickedUp = true;

            if (_visualModel != null)
                _visualModel.SetActive(false);

            _collider.enabled = false;

            if (_respawnTime > 0f)
                Invoke(nameof(Respawn), _respawnTime);
            else
                Destroy(gameObject);
        }

        private void Respawn()
        {
            _isPickedUp = false;

            if (_visualModel != null)
                _visualModel.SetActive(true);

            _collider.enabled = true;
        }

        private void OnDrawGizmos()
        {
            if (_weaponData == null) return;

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
}

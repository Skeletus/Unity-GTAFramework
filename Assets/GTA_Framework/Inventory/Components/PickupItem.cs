using GTAFramework.Inventory.Data;
using GTAFramework.Inventory.Interfaces;
using UnityEngine;

namespace GTAFramework.Inventory.Components
{
    /// <summary>
    /// Componente para objetos que pueden ser recogidos del mundo.
    /// KISS: detección por trigger, aplicación inmediata.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class PickupItem : MonoBehaviour
    {
        [Header("Item Settings")]
        [SerializeField] private ItemData _itemData;
        [SerializeField] private int _quantity = 1;

        [Header("Pickup Settings")]
        [SerializeField] private string _receiverTag = "Player";
        [SerializeField] private float _respawnTime = 0f; // 0 = no respawnea

        [Header("Visuals")]
        [SerializeField] private GameObject _visualModel;
        [SerializeField] private bool _rotateItem = true;
        [SerializeField] private float _rotationSpeed = 90f;

        private Collider _collider;
        private bool _isPickedUp;

        private void Awake()
        {
            _collider = GetComponent<Collider>();
            // Asegurar que sea trigger
            _collider.isTrigger = true;
        }

        private void Update()
        {
            // Rotación visual del item
            if (_rotateItem && !_isPickedUp && _visualModel != null)
            {
                _visualModel.transform.Rotate(0f, _rotationSpeed * Time.deltaTime, 0f);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log($"[PickupItem] Trigger detectado: {other.name} (Tag: {other.tag})");

            if (_isPickedUp)
            {
                Debug.Log("[PickupItem] Ya fue recogido, ignorando.");
                return;
            }

            if (_itemData == null)
            {
                Debug.LogError("[PickupItem] No hay ItemData asignado!");
                return;
            }

            if (!other.CompareTag(_receiverTag))
            {
                Debug.Log($"[PickupItem] Tag incorrecto. Esperado: '{_receiverTag}', Recibido: '{other.tag}'");
                return;
            }

            Debug.Log($"[PickupItem] Tag correcto '{_receiverTag}' detectado.");

            // Buscar IPickupReceiver en el objeto o padres
            var receiver = other.GetComponentInParent<IPickupReceiver>();
            if (receiver == null)
            {
                Debug.LogError($"[PickupItem] No se encontró IPickupReceiver en {other.name} ni en sus padres.");
                return;
            }

            Debug.Log($"[PickupItem] IPickupReceiver encontrado: {receiver.GetType().Name}");

            // Intentar entregar el item
            bool received = receiver.ReceiveItem(_itemData, _quantity);
            Debug.Log($"[PickupItem] ReceiveItem resultado: {received}");

            if (received)
            {
                Debug.Log($"[PickupItem] Item '{_itemData.itemName}' recogido exitosamente!");
                OnPickedUp();
            }
            else
            {
                Debug.LogWarning($"[PickupItem] No se pudo recibir el item '{_itemData.itemName}'. ¿Inventario lleno?");
            }
        }

        /// <summary>
        /// Configura el pickup programáticamente (para spawning).
        /// </summary>
        public void Setup(ItemData item, int quantity)
        {
            _itemData = item;
            _quantity = quantity;
        }

        private void OnPickedUp()
        {
            _isPickedUp = true;

            // Desactivar visual
            if (_visualModel != null)
                _visualModel.SetActive(false);

            // Desactivar collider
            _collider.enabled = false;

            // Respawn o destrucción
            if (_respawnTime > 0f)
            {
                Invoke(nameof(Respawn), _respawnTime);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Respawn()
        {
            _isPickedUp = false;

            if (_visualModel != null)
                _visualModel.SetActive(true);

            _collider.enabled = true;
        }

        // Gizmo para visualizar en editor según tipo de item
        private void OnDrawGizmos()
        {
            if (_itemData == null) return;

            Gizmos.color = _itemData.type switch
            {
                ItemType.Health => Color.green,
                ItemType.Armor => Color.blue,
                ItemType.Weapon => Color.red,
                ItemType.Ammo => Color.yellow,
                _ => Color.white
            };

            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
}
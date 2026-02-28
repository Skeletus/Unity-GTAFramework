using UnityEngine;
using GTAFramework.Core.Container;
using GTAFramework.Core.Services;
using Unity.Cinemachine;
using UnityEngine.InputSystem;

public class ThirdPersonCameraController : MonoBehaviour
{
    [Header("Cinemachine")]
    [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
    public GameObject CinemachineCameraTarget;

    [Tooltip("How far in degrees can you move the camera up")]
    public float TopClamp = 70.0f;

    [Tooltip("How far in degrees can you move the camera down")]
    public float BottomClamp = -30.0f;

    [Tooltip("Additional degrees to override the camera. Useful for fine tuning camera position when locked")]
    public float CameraAngleOverride = 0.0f;

    [Tooltip("For locking the camera position on all axis")]
    public bool LockCameraPosition = false;

    [Tooltip("Sensitivity for mouse input")]
    public float MouseSensitivity = 1.0f;

    [Tooltip("Sensitivity for gamepad/controller input")]
    public float GamepadSensitivity = 2.0f;

    [Header("Aim Camera")]
    [Tooltip("Cinemachine virtual camera to use when aiming")]
    [SerializeField] private CinemachineCamera _aimCamera;

    [Tooltip("Priority to set when aiming")]
    [SerializeField] private int _aimPriority = 10;

    [Header("Aim Sensitivity")]
    [Tooltip("Sensitivity for mouse input while aiming")]
    [SerializeField] private float _aimMouseSensitivity = 0.5f;

    [Tooltip("Sensitivity for gamepad/controller input while aiming")]
    [SerializeField] private float _aimGamepadSensitivity = 1.0f;

    [Header("Mouse Raycast")]
    [Tooltip("Layer mask used to raycast from mouse position into the world")]
    [SerializeField] private LayerMask _mouseRaycastMask = ~0;

    [Tooltip("Optional transform to move to the current mouse world hit point")]
    [SerializeField] private Transform _mouseWorldDebugTransform;

    [Header("Shooting")]
    [Tooltip("Particle effect prefab instantiated when a valid bullet target is hit")]
    [SerializeField] private GameObject _hitEffect;

    [Tooltip("Layer mask used by shooting hitscan")]
    [SerializeField] private LayerMask _shootHitMask = ~0;

    // Track previous aim state to detect changes
    private bool _wasAiming;
    private bool _wasShooting;

    // Store original priorities
    private int _normalCameraOriginalPriority = 0;
    private int _aimCameraOriginalPriority = 0;

    // Cinemachine target values
    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;
    private Vector3 _mouseWorldPosition;
    private bool _hasMouseWorldPosition;

    public Vector3 MouseWorldPosition => _mouseWorldPosition;

    // Input reference
    [Inject] private InputService _inputService;

    // Threshold for input detection
    private const float _threshold = 0.01f;

    // Check if current device is mouse
    private bool IsCurrentDeviceMouse
    {
        get
        {
            // You can implement device detection based on your InputSystem configuration
            // For now, we'll use a simple approach
            return false;
        }
    }

    private void Start()
    {
        // Get reference to InputService
        _inputService = DIContainer.Instance.Resolve<InputService>();
        
        // Store original priorities
        if (CinemachineCameraTarget != null)
        {
            CinemachineCamera normalCam = CinemachineCameraTarget.GetComponent<CinemachineCamera>();
            if (normalCam != null)
                _normalCameraOriginalPriority = normalCam.Priority;
            
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
        }
        
        if (_aimCamera != null)
        {
            _aimCameraOriginalPriority = _aimCamera.Priority;
            _aimCamera.Priority = 0; // Set to 0 initially
        }
        else
        {
            Debug.LogWarning("Aim Camera is not assigned in the inspector!");
        }
    }

    private void LateUpdate()
    {
        HandleCameraSwitch();
        CameraRotation();
        UpdateMouseWorldPosition();
        HandleShooting();
    }

    private void HandleCameraSwitch()
    {
        bool isAiming = _inputService?.IsAimPressed ?? false;

        // Only process if the aim state has changed
        if (isAiming != _wasAiming)
        {
            _wasAiming = isAiming;
            
            CinemachineCamera normalCam = CinemachineCameraTarget?.GetComponent<CinemachineCamera>();

            if (isAiming && _aimCamera != null)
            {
                // Set aim camera priority to chosen value
                _aimCamera.Priority = _aimPriority;
                
                // Set normal camera priority to 0
                if (normalCam != null)
                    normalCam.Priority = 0;
            }
            else if (!isAiming)
            {
                // Reset aim camera priority to 0
                if (_aimCamera != null)
                    _aimCamera.Priority = 0;
                
                // Restore normal camera priority
                if (normalCam != null)
                    normalCam.Priority = _normalCameraOriginalPriority;
            }
        }
    }

    private void CameraRotation()
    {
        // If there is no input service or target, return
        if (_inputService == null || CinemachineCameraTarget == null)
            return;

        // Get the look input from InputService
        Vector2 lookInput = _inputService.LookInput;
        bool isAiming = _inputService.IsAimPressed;

        // If there is an input and camera position is not fixed
        if (lookInput.sqrMagnitude >= _threshold && !LockCameraPosition)
        {
            // Determine sensitivity based on input device
            float sensitivity;
            if (IsCurrentDeviceMouse)
                sensitivity = isAiming ? _aimMouseSensitivity : MouseSensitivity;
            else
                sensitivity = isAiming ? _aimGamepadSensitivity : GamepadSensitivity;

            // Don't multiply mouse input by Time.deltaTime
            float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

            _cinemachineTargetYaw += lookInput.x * sensitivity * deltaTimeMultiplier;
            _cinemachineTargetPitch += lookInput.y * sensitivity * deltaTimeMultiplier;
        }

        // Clamp our rotations so our values are limited to 360 degrees
        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

        // Apply rotation to CinemachineCameraTarget
        CinemachineCameraTarget.transform.rotation = Quaternion.Euler(
            _cinemachineTargetPitch + CameraAngleOverride,
            _cinemachineTargetYaw,
            0.0f
        );
    }

    private void UpdateMouseWorldPosition()
    {
        Camera worldCamera = Camera.main;
        if (worldCamera == null || Mouse.current == null)
            return;

        Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();
        Ray mouseRay = worldCamera.ScreenPointToRay(mouseScreenPosition);

        if (Physics.Raycast(mouseRay, out RaycastHit hitInfo, Mathf.Infinity, _mouseRaycastMask, QueryTriggerInteraction.Ignore))
        {
            _mouseWorldPosition = hitInfo.point;
            _hasMouseWorldPosition = true;

            if (_mouseWorldDebugTransform != null)
                _mouseWorldDebugTransform.position = _mouseWorldPosition;
        }
        else
        {
            _hasMouseWorldPosition = false;
        }
    }

    private void HandleShooting()
    {
        if (_inputService == null)
            return;

        bool isShooting = _inputService.IsShootPressed;
        bool shootPressedThisFrame = isShooting && !_wasShooting;
        _wasShooting = isShooting;

        if (!shootPressedThisFrame || !_hasMouseWorldPosition)
            return;

        Camera worldCamera = Camera.main;
        if (worldCamera == null)
            return;

        Vector3 rayOrigin = worldCamera.transform.position;
        Vector3 rayDirection = (_mouseWorldPosition - rayOrigin).normalized;
        if (rayDirection.sqrMagnitude <= Mathf.Epsilon)
            return;

        if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hitInfo, Mathf.Infinity, _shootHitMask, QueryTriggerInteraction.Ignore))
        {
            BulletTarget bulletTarget = hitInfo.collider.GetComponentInParent<BulletTarget>();
            if (bulletTarget != null && _hitEffect != null)
            {
                Quaternion hitRotation = Quaternion.LookRotation(hitInfo.normal);
                Instantiate(_hitEffect, hitInfo.point, hitRotation);
            }
        }
    }

    /// <summary>
    /// Clamps an angle between a minimum and maximum value
    /// </summary>
    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }
}

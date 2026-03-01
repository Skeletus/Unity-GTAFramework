using UnityEngine;
using UnityEngine.InputSystem;

public class CursorLock : MonoBehaviour
{
    [Header("Debug Aiming")]
    public Animator animator;
    public bool forceAiming = false;

    [SerializeField] bool lockOnStart = true;
    [SerializeField] InputActionReference escapeAction; // asigna tu acción aquí

    void OnValidate()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }
    
    void OnEnable()
    {
        if (escapeAction != null)
            escapeAction.action.performed += OnEscape;
    }

    void OnDisable()
    {
        if (escapeAction != null)
            escapeAction.action.performed -= OnEscape;
    }

    void Start()
    {
        if (lockOnStart) LockCursor();
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && lockOnStart) LockCursor();
    }

    void OnEscape(InputAction.CallbackContext ctx)
    {
        UnlockCursor();
    }

    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
    }

    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        //Cursor.visible = true;
    }

    void Update()
    {
        if (forceAiming)
        {
            animator.Play("Aiming", animator.GetLayerIndex("upperbody_rifle"));
        }
    }
}

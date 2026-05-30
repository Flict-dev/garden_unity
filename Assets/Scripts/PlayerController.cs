using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[DisallowMultipleComponent]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3.5f;
    public float sprintMultiplier = 1.6f;
    public float jumpForce = 5f;
    public float gamepadLookSensitivity = 140f;
    public float cameraHeight = 0.55f;
    public float groundCheckDistance = 0.3f;

    [Header("Stats")]
    public float maxHealth = 100f;

    public static event Action<float, float> OnHealthChanged;
    public float CurrentHealth => _currentHealth;

    private Rigidbody _rigidbody;
    private CapsuleCollider _capsuleCollider;
    private Camera _playerCamera;
    private StickWeapon _stickWeapon;
    private Vector2 _moveInput;
    private float _cameraPitch;
    private float _currentHealth;
    private bool _jumpQueued;
    private Vector3 _spawnPosition;

    private void Awake()
    {
        _spawnPosition = transform.position;
        SetupPhysics();
        SetupView();
        SetupWeapon();

        _currentHealth = maxHealth;
        OnHealthChanged?.Invoke(_currentHealth, maxHealth);

        if (transform.position.y < _capsuleCollider.height * 0.5f)
        {
            transform.position = new Vector3(
                transform.position.x,
                _capsuleCollider.height * 0.5f,
                transform.position.z
            );
        }
    }

    private void OnEnable()
    {
        LockCursor();
    }

    private void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Update()
    {
        ReadMoveInput();
        HandleCursor();
        HandleLook();
        HandleJumpInput();
        HandleAttackInput();
    }

    private void FixedUpdate()
    {
        Move();
        ApplyJump();
    }

    public void TakeDamage(float damage)
    {
        if (damage <= 0f)
        {
            return;
        }

        _currentHealth -= damage;
        GameFeedback.PlayPlayerDamaged(transform.position, damage);
        OnHealthChanged?.Invoke(_currentHealth, maxHealth);
        if (_currentHealth > 0f)
        {
            return;
        }

        GardenManager manager = GardenManager.Instance;
        if (manager != null)
        {
            manager.CompleteDefeat("The spiders got you.");
            return;
        }

        GameData.SetRunResult(false, "The spiders got you.");
        Time.timeScale = 1f;
        SceneManager.LoadScene("GameOver");
    }

    private void SetupPhysics()
    {
        gameObject.layer = 2;

        foreach (Collider col in GetComponents<Collider>())
        {
            if (!(col is CapsuleCollider))
            {
                col.enabled = false;
            }
        }

        _capsuleCollider = GetComponent<CapsuleCollider>();
        if (_capsuleCollider == null)
        {
            _capsuleCollider = gameObject.AddComponent<CapsuleCollider>();
        }

        _capsuleCollider.height = 1.2f;
        _capsuleCollider.radius = 0.25f;
        _capsuleCollider.center = new Vector3(0f, 0.6f, 0f);

        _rigidbody = GetComponent<Rigidbody>();
        if (_rigidbody == null)
        {
            _rigidbody = gameObject.AddComponent<Rigidbody>();
        }

        _rigidbody.useGravity = true;
        _rigidbody.freezeRotation = true;
        _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.enabled = false;
        }
    }

    private void SetupView()
    {
        _playerCamera = GetComponentInChildren<Camera>(true);
        if (_playerCamera == null)
        {
            _playerCamera = Camera.main;
        }

        if (_playerCamera == null)
        {
            return;
        }

        Transform cameraTransform = _playerCamera.transform;
        cameraTransform.SetParent(transform);
        cameraTransform.localPosition = new Vector3(0f, cameraHeight, 0f);
        cameraTransform.localRotation = Quaternion.identity;
        _playerCamera.nearClipPlane = Mathf.Min(_playerCamera.nearClipPlane, 0.05f);
        _cameraPitch = 0f;
    }

    private void SetupWeapon()
    {
        Transform weaponParent = _playerCamera != null ? _playerCamera.transform : transform;
        GameObject weaponGo = new GameObject("FlySwatterWeapon");
        weaponGo.transform.SetParent(weaponParent, false);
        _stickWeapon = weaponGo.AddComponent<StickWeapon>();
    }

    private void ReadMoveInput()
    {
        Vector2 keyboardMove = Vector2.zero;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) keyboardMove.y += 1f;
            if (Keyboard.current.sKey.isPressed) keyboardMove.y -= 1f;
            if (Keyboard.current.dKey.isPressed) keyboardMove.x += 1f;
            if (Keyboard.current.aKey.isPressed) keyboardMove.x -= 1f;
        }

        Vector2 stickMove = Gamepad.current != null
            ? Gamepad.current.leftStick.ReadValue()
            : Vector2.zero;

        _moveInput = Vector2.ClampMagnitude(keyboardMove + stickMove, 1f);
    }

    private void HandleCursor()
    {
        if (Time.timeScale <= 0f)
        {
            return;
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame
            && Cursor.lockState != CursorLockMode.Locked)
        {
            LockCursor();
        }
    }

    private void HandleLook()
    {
        if (_playerCamera == null)
        {
            return;
        }

        if (Cursor.lockState != CursorLockMode.Locked)
        {
            return;
        }

        Vector2 mouseLook = Mouse.current != null
            ? Mouse.current.delta.ReadValue() * GameData.MouseSensitivity
            : Vector2.zero;
        Vector2 gamepadLook = Gamepad.current != null
            ? Gamepad.current.rightStick.ReadValue() * gamepadLookSensitivity * Time.deltaTime
            : Vector2.zero;
        Vector2 lookDelta = mouseLook + gamepadLook;

        transform.Rotate(Vector3.up * lookDelta.x, Space.World);

        _cameraPitch = Mathf.Clamp(_cameraPitch - lookDelta.y, -85f, 85f);
        _playerCamera.transform.localRotation = Quaternion.Euler(_cameraPitch, 0f, 0f);
    }

    private void HandleJumpInput()
    {
        if ((Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            || (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame))
        {
            _jumpQueued = true;
        }

    }

    private void HandleAttackInput()
    {
        if (Time.timeScale <= 0f || _stickWeapon == null)
        {
            return;
        }

        bool mouseAttack = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame
            && Cursor.lockState == CursorLockMode.Locked;
        bool gamepadAttack = Gamepad.current != null && Gamepad.current.rightTrigger.wasPressedThisFrame;

        if (mouseAttack || gamepadAttack)
        {
            _stickWeapon.TrySwing();
        }
    }

    private void Move()
    {
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        Vector3 desiredDirection = forward * _moveInput.y + right * _moveInput.x;
        float currentSpeed = moveSpeed;
        if (IsSprintPressed())
        {
            currentSpeed *= sprintMultiplier;
        }

        Vector3 horizontalVelocity = desiredDirection.normalized * currentSpeed;
        _rigidbody.linearVelocity = new Vector3(
            horizontalVelocity.x,
            _rigidbody.linearVelocity.y,
            horizontalVelocity.z
        );
    }

    private void ApplyJump()
    {
        if (!_jumpQueued)
        {
            return;
        }

        _jumpQueued = false;
        if (!IsGrounded())
        {
            return;
        }

        Vector3 velocity = _rigidbody.linearVelocity;
        velocity.y = 0f;
        _rigidbody.linearVelocity = velocity;
        _rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
    }

    private bool IsGrounded()
    {
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        float rayDistance = (_capsuleCollider.height * 0.5f) + groundCheckDistance;
        int layerMask = ~(1 << 2);
        return Physics.SphereCast(
            origin,
            _capsuleCollider.radius * 0.9f,
            Vector3.down,
            out _,
            rayDistance,
            layerMask,
            QueryTriggerInteraction.Ignore
        );
    }

    private bool IsSprintPressed()
    {
        bool keyboardSprint = Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;
        bool gamepadSprint = Gamepad.current != null && Gamepad.current.leftStickButton.isPressed;
        return keyboardSprint || gamepadSprint;
    }

    private static void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}

using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
[DisallowMultipleComponent]
public class MeteorMover : MonoBehaviour
{
    public float moveSpeed = 2.8f;
    public float contactDamage = 12f;
    public float damageCooldown = 1f;
    public float maxHealth = 3f;
    public BugType bugType = BugType.Ant;

    private Rigidbody _rigidbody;
    private SphereCollider _sphereCollider;
    private BugAnimationController _animationController;
    private Transform _target;
    private float _currentHealth;
    private float _damageTimer;
    private bool _isDestroyed;

    private void Awake()
    {
        SetupPhysics();
        _currentHealth = maxHealth;
    }

    private void Start()
    {
        if (!HasModelRoot())
        {
            BugModelBuilder.Build(gameObject, bugType);
            SetupAnimation();
        }

        TryAcquireTarget();
    }

    private void Update()
    {
        _damageTimer -= Time.deltaTime;

        if (!TryAcquireTarget())
        {
            return;
        }

        // Rotate toward target
        Vector3 dir = _target.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, targetRot, 360f * Time.deltaTime);
        }
    }

    private void FixedUpdate()
    {
        if (!TryAcquireTarget())
        {
            return;
        }

        Vector3 dir = _target.position - transform.position;
        dir.y = 0f;
        float dist = dir.magnitude;

        if (dist > 0.8f)
        {
            Vector3 velocity = dir.normalized * moveSpeed;
            _rigidbody.linearVelocity = new Vector3(velocity.x, _rigidbody.linearVelocity.y, velocity.z);
        }
        else
        {
            _rigidbody.linearVelocity = new Vector3(0f, _rigidbody.linearVelocity.y, 0f);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (_damageTimer > 0f)
        {
            return;
        }

        if (bugType == BugType.Spider)
        {
            if (!collision.gameObject.TryGetComponent(out PlayerController player))
            {
                return;
            }

            player.TakeDamage(contactDamage);
            _damageTimer = damageCooldown;
            return;
        }

        Vegetable vegetable = collision.gameObject.GetComponentInParent<Vegetable>();
        if (vegetable == null || !vegetable.IsAlive)
        {
            return;
        }

        vegetable.TakeDamage(contactDamage);
        _damageTimer = damageCooldown;
    }

    private void OnDestroy()
    {
        if (!_isDestroyed)
        {
            MeteorSpawner.Instance?.NotifyBugDestroyed(this);
        }
    }

    public void SetTarget(Transform target)
    {
        _target = target;
    }

    public void Configure(BugType type, float speed, float health, float damage)
    {
        bugType = type;
        moveSpeed = speed;
        maxHealth = health;
        contactDamage = damage;
        _currentHealth = maxHealth;
        BugModelBuilder.Build(gameObject, bugType);
        SetupAnimation();
    }

    public void TakeDamage(float damage)
    {
        if (damage <= 0f)
        {
            return;
        }

        _currentHealth -= damage;
        GameFeedback.PlayBugHit(transform.position, bugType, damage);
        if (_currentHealth <= 0f)
        {
            GameData.AddKill();
            DestroyBug();
        }
    }

    private void SetupPhysics()
    {
        gameObject.layer = 2;

        _sphereCollider = GetComponent<SphereCollider>();
        if (_sphereCollider == null)
        {
            _sphereCollider = gameObject.AddComponent<SphereCollider>();
        }
        _sphereCollider.radius = 0.4f;
        _sphereCollider.center = new Vector3(0f, 0.2f, 0f);

        _rigidbody = GetComponent<Rigidbody>();
        if (_rigidbody == null)
        {
            _rigidbody = gameObject.AddComponent<Rigidbody>();
        }
        _rigidbody.useGravity = true;
        _rigidbody.freezeRotation = true;
        _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    private void SetupAnimation()
    {
        if (_animationController == null)
        {
            _animationController = GetComponent<BugAnimationController>();
            if (_animationController == null)
            {
                _animationController = gameObject.AddComponent<BugAnimationController>();
            }
        }

        _animationController.Configure(bugType, _rigidbody);
    }

    private bool HasModelRoot()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child.name.EndsWith("Model"))
            {
                return true;
            }
        }

        return false;
    }

    private bool TryAcquireTarget()
    {
        if (_target != null && IsTargetValid())
        {
            return true;
        }

        if (bugType == BugType.Spider)
        {
            PlayerController player = FindFirstObjectByType<PlayerController>();
            if (player == null)
            {
                return false;
            }

            _target = player.transform;
            return true;
        }

        Vegetable vegetable = GardenManager.Instance?.GetNearestVegetable(transform.position);
        if (vegetable == null)
        {
            return false;
        }

        _target = vegetable.transform;
        return true;
    }

    private bool IsTargetValid()
    {
        if (bugType == BugType.Spider)
        {
            return _target.GetComponent<PlayerController>() != null;
        }

        Vegetable vegetable = _target.GetComponent<Vegetable>();
        return vegetable != null && vegetable.IsAlive;
    }

    private void DestroyBug()
    {
        if (_isDestroyed)
        {
            return;
        }

        _isDestroyed = true;
        GameFeedback.PlayBugDestroyed(transform.position, bugType);
        MeteorSpawner.Instance?.NotifyBugDestroyed(this);
        Destroy(gameObject);
    }
}

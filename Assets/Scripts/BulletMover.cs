using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[DisallowMultipleComponent]
public class BulletMover : MonoBehaviour
{
    private static readonly Vector3 BulletScale = new Vector3(0.08f, 0.08f, 0.08f);

    public float speed = 28f;
    public float maxDistance = 45f;
    public float damage = 1f;

    private CapsuleCollider _capsuleCollider;
    private Rigidbody _rigidbody;
    private Vector3 _startPosition;

    private void Awake()
    {
        transform.localScale = BulletScale;
        SetupPhysics();
    }

    private void OnEnable()
    {
        _startPosition = transform.position;
    }

    private void Update()
    {
        float distance = Vector3.Distance(_startPosition, transform.position);
        if (distance > maxDistance)
        {
            Destroy(gameObject);
        }
    }

    public void Launch(Vector3 direction)
    {
        SetupPhysics();

        Vector3 launchDirection = direction.sqrMagnitude > 0.001f
            ? direction.normalized
            : transform.forward;
        launchDirection.y = 0f;
        if (launchDirection.sqrMagnitude < 0.001f)
        {
            launchDirection = Vector3.forward;
        }

        transform.rotation = Quaternion.FromToRotation(Vector3.up, launchDirection.normalized);
        _startPosition = transform.position;
        _rigidbody.angularVelocity = Vector3.zero;
        _rigidbody.linearVelocity = launchDirection.normalized * speed;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger)
        {
            return;
        }

        if (other.TryGetComponent(out PlayerController _))
        {
            return;
        }

        if (other.TryGetComponent(out MeteorMover meteorMover))
        {
            meteorMover.TakeDamage(damage);
        }

        Destroy(gameObject);
    }

    private void SetupPhysics()
    {
        _capsuleCollider = GetComponent<CapsuleCollider>();
        if (_capsuleCollider == null)
        {
            _capsuleCollider = gameObject.AddComponent<CapsuleCollider>();
        }

        _capsuleCollider.isTrigger = true;
        _capsuleCollider.direction = 1;
        _capsuleCollider.radius = 0.3f;
        _capsuleCollider.height = 1.6f;

        _rigidbody = GetComponent<Rigidbody>();
        if (_rigidbody == null)
        {
            _rigidbody = gameObject.AddComponent<Rigidbody>();
        }

        _rigidbody.useGravity = false;
        _rigidbody.isKinematic = false;
        _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }
}

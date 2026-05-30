using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[DisallowMultipleComponent]
public class BugAnimationController : MonoBehaviour
{
    private struct LegPose
    {
        public Transform Transform;
        public Quaternion BaseRotation;
        public float Phase;
        public float Side;
    }

    private readonly List<LegPose> _legs = new List<LegPose>();

    private Animator _animator;
    private Rigidbody _rigidbody;
    private Transform _modelRoot;
    private Vector3 _baseModelPosition;
    private Quaternion _baseModelRotation;
    private ParticleSystem _dustTrail;
    private BugType _type;
    private float _walkPhase;

    public void Configure(BugType type, Rigidbody rigidbody)
    {
        _type = type;
        _rigidbody = rigidbody;
        SetupAnimator();
        CacheModel();

        if (_dustTrail == null)
        {
            _dustTrail = GameFeedback.AttachBugDustTrail(transform, type);
        }
    }

    private void Awake()
    {
        SetupAnimator();
    }

    private void LateUpdate()
    {
        if (_modelRoot == null)
        {
            CacheModel();
            if (_modelRoot == null)
            {
                return;
            }
        }

        Vector3 velocity = _rigidbody != null ? _rigidbody.linearVelocity : Vector3.zero;
        velocity.y = 0f;
        float speed = velocity.magnitude;
        float speed01 = Mathf.Clamp01(speed / 4.5f);
        float stepFrequency = _type == BugType.Spider ? 16f : (_type == BugType.Ant ? 13f : 9f);
        float legSwing = Mathf.Lerp(8f, _type == BugType.Spider ? 28f : 22f, speed01);

        _walkPhase += Time.deltaTime * Mathf.Lerp(1.5f, stepFrequency, speed01);

        float bob = Mathf.Sin(_walkPhase * 2f) * Mathf.Lerp(0.004f, 0.055f, speed01);
        float roll = Mathf.Sin(_walkPhase) * Mathf.Lerp(0.5f, 4.5f, speed01);
        float pitch = Mathf.Cos(_walkPhase * 0.75f) * Mathf.Lerp(0.2f, 2.8f, speed01);

        _modelRoot.localPosition = _baseModelPosition + Vector3.up * bob;
        _modelRoot.localRotation = _baseModelRotation * Quaternion.Euler(pitch, 0f, roll);

        for (int i = 0; i < _legs.Count; i++)
        {
            LegPose leg = _legs[i];
            float phase = _walkPhase + leg.Phase;
            float forwardSwing = Mathf.Sin(phase) * legSwing;
            float lift = Mathf.Max(0f, Mathf.Sin(phase + Mathf.PI * 0.5f)) * legSwing * 0.34f;
            leg.Transform.localRotation = leg.BaseRotation * Quaternion.Euler(lift, 0f, forwardSwing * leg.Side);
        }

        if (_animator != null)
        {
            _animator.speed = Mathf.Lerp(0.65f, 1.45f, speed01);
        }

        if (_dustTrail != null)
        {
            ParticleSystem.EmissionModule emission = _dustTrail.emission;
            emission.rateOverTime = Mathf.Lerp(0f, _type == BugType.Beetle ? 10f : 18f, speed01);
        }
    }

    private void SetupAnimator()
    {
        _animator = GetComponent<Animator>();
        _animator.applyRootMotion = false;
        _animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
    }

    private void CacheModel()
    {
        _modelRoot = null;
        _legs.Clear();

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (child.name.EndsWith("Model"))
            {
                _modelRoot = child;
                break;
            }
        }

        if (_modelRoot == null)
        {
            return;
        }

        _baseModelPosition = _modelRoot.localPosition;
        _baseModelRotation = _modelRoot.localRotation;
        CacheLegs(_modelRoot);
    }

    private void CacheLegs(Transform root)
    {
        foreach (Transform child in root.GetComponentsInChildren<Transform>())
        {
            if (!child.name.StartsWith("Leg") || child.parent == null || child.parent.name.StartsWith("Leg"))
            {
                continue;
            }

            float side = child.localPosition.x < 0f ? 1f : -1f;
            float phase = (_legs.Count % 2 == 0 ? 0f : Mathf.PI) + Mathf.Abs(child.localPosition.z) * 1.7f;
            _legs.Add(new LegPose
            {
                Transform = child,
                BaseRotation = child.localRotation,
                Phase = phase,
                Side = side
            });
        }
    }
}

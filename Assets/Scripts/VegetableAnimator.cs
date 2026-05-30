using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Animation))]
[DisallowMultipleComponent]
public class VegetableAnimator : MonoBehaviour
{
    private const string VisualRootName = "VisualRoot";
    private const string IdleClipName = "VegetableIdle";

    private Animator _animator;
    private Animation _animation;
    private Vegetable _vegetable;
    private Transform _visualRoot;
    private Vector3 _baseLocalPosition;
    private Quaternion _baseLocalRotation;
    private Vector3 _baseLocalScale = Vector3.one;
    private float _seed;
    private float _hitPulse;
    private VegetableType _type;

    public void Configure(VegetableType type)
    {
        _type = type;
        _seed = Random.Range(0f, 100f);
        _vegetable = GetComponent<Vegetable>();
        CacheVisualRoot();
        SetupAnimator();
        BuildKeyframedIdleClip();
    }

    public void PlayHitReact()
    {
        _hitPulse = 1f;
    }

    private void Awake()
    {
        _vegetable = GetComponent<Vegetable>();
        _seed = Random.Range(0f, 100f);
        SetupAnimator();
    }

    private void OnEnable()
    {
        CacheVisualRoot();
    }

    private void LateUpdate()
    {
        if (_visualRoot == null)
        {
            CacheVisualRoot();
            if (_visualRoot == null)
            {
                return;
            }
        }

        float healthRatio = _vegetable != null && _vegetable.maxHealth > 0f
            ? Mathf.Clamp01(_vegetable.CurrentHealth / _vegetable.maxHealth)
            : 1f;

        float typeOffset = _type == VegetableType.Carrot ? 0.45f : (_type == VegetableType.Potato ? 0.2f : 0f);
        float sway = Mathf.Sin(Time.time * (1.55f + typeOffset) + _seed) * Mathf.Lerp(1.8f, 4.4f, healthRatio);
        float nod = Mathf.Sin(Time.time * 1.1f + _seed * 0.7f) * Mathf.Lerp(0.8f, 2.2f, healthRatio);

        _hitPulse = Mathf.MoveTowards(_hitPulse, 0f, Time.deltaTime * 5f);
        float hitDrop = EaseOutCubic(_hitPulse) * 0.08f;
        float hitTilt = EaseOutCubic(_hitPulse) * 8f;

        _visualRoot.localPosition = _baseLocalPosition + Vector3.down * hitDrop;
        _visualRoot.localRotation = _baseLocalRotation * Quaternion.Euler(nod - hitTilt, 0f, sway);

        if (_animator != null)
        {
            _animator.speed = Mathf.Lerp(0.65f, 1.2f, healthRatio);
        }
    }

    private void CacheVisualRoot()
    {
        Transform root = transform.Find(VisualRootName);
        if (root == null)
        {
            return;
        }

        if (_visualRoot == root)
        {
            return;
        }

        _visualRoot = root;
        _baseLocalPosition = _visualRoot.localPosition;
        _baseLocalRotation = _visualRoot.localRotation;
        _baseLocalScale = _visualRoot.localScale;
    }

    private void SetupAnimator()
    {
        _animator = GetComponent<Animator>();
        _animator.applyRootMotion = false;
        _animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

        _animation = GetComponent<Animation>();
        _animation.playAutomatically = true;
    }

    private void BuildKeyframedIdleClip()
    {
        if (_animation == null || _visualRoot == null)
        {
            return;
        }

        AnimationClip clip = new AnimationClip
        {
            name = IdleClipName,
            legacy = true,
            frameRate = 30f,
            wrapMode = WrapMode.Loop
        };

        float ySquash = _type == VegetableType.Potato ? 0.025f : 0.045f;
        float xStretch = _type == VegetableType.Carrot ? 0.018f : 0.03f;
        clip.SetCurve(VisualRootName, typeof(Transform), "localScale.x",
            LoopCurve(_baseLocalScale.x, _baseLocalScale.x * xStretch, 0f));
        clip.SetCurve(VisualRootName, typeof(Transform), "localScale.y",
            LoopCurve(_baseLocalScale.y, -_baseLocalScale.y * ySquash, 0.25f));
        clip.SetCurve(VisualRootName, typeof(Transform), "localScale.z",
            LoopCurve(_baseLocalScale.z, _baseLocalScale.z * xStretch, 0.5f));

        if (_animation.GetClip(IdleClipName) != null)
        {
            _animation.RemoveClip(IdleClipName);
        }

        _animation.AddClip(clip, IdleClipName);
        _animation.clip = clip;
        _animation.Play(IdleClipName);
    }

    private static AnimationCurve LoopCurve(float center, float amplitude, float phase)
    {
        Keyframe[] keys =
        {
            new Keyframe(0f, center + Mathf.Sin(phase * Mathf.PI * 2f) * amplitude),
            new Keyframe(0.5f, center + Mathf.Sin((phase + 0.5f) * Mathf.PI * 2f) * amplitude),
            new Keyframe(1f, center + Mathf.Sin((phase + 1f) * Mathf.PI * 2f) * amplitude)
        };

        AnimationCurve curve = new AnimationCurve(keys);
        for (int i = 0; i < keys.Length; i++)
        {
            curve.SmoothTangents(i, 0f);
        }

        return curve;
    }

    private static float EaseOutCubic(float t)
    {
        return 1f - Mathf.Pow(1f - Mathf.Clamp01(t), 3f);
    }
}

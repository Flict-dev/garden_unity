using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class GameFeedback : MonoBehaviour
{
    private const int SampleRate = 22050;

    private static GameFeedback _instance;
    private static readonly Dictionary<string, string> UserClipResourcePaths = new Dictionary<string, string>
    {
        { "swing", "Audio/SFX/stick" },
        { "vegetable_hit", "Audio/SFX/bug_eat" },
        { "vegetable_lost", "Audio/SFX/bug_eat" },
        { "player_hit", "Audio/SFX/palyer_hit" }
    };

    private readonly Dictionary<string, AudioClip> _clips = new Dictionary<string, AudioClip>();
    private Material _dustParticleMaterial;
    private Material _leafParticleMaterial;
    private Material _sparkParticleMaterial;
    private AudioSource _ambientSource;

    public static void EnsureInScene()
    {
        Ensure();
    }

    public static ParticleSystem AttachBugDustTrail(Transform parent, BugType type)
    {
        if (parent == null)
        {
            return null;
        }

        GameFeedback feedback = Ensure();
        GameObject trailGo = new GameObject("BugDustTrail");
        trailGo.transform.SetParent(parent, false);
        trailGo.transform.localPosition = new Vector3(0f, 0.04f, 0f);

        ParticleSystem particles = trailGo.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule main = particles.main;
        main.loop = true;
        main.playOnAwake = true;
        main.maxParticles = 60;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.22f, 0.5f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.15f, 0.75f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.16f);
        main.startColor = BugDustColor(type);

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.rateOverTime = 0f;

        ParticleSystem.ShapeModule shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.35f;

        ParticleSystem.VelocityOverLifetimeModule velocity = particles.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.World;
        velocity.y = new ParticleSystem.MinMaxCurve(0.08f, 0.22f);

        ParticleSystem.ColorOverLifetimeModule color = particles.colorOverLifetime;
        color.enabled = true;
        color.color = FadeGradient(BugDustColor(type), 0.58f);

        ParticleSystemRenderer renderer = trailGo.GetComponent<ParticleSystemRenderer>();
        renderer.material = feedback.GetDustParticleMaterial();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;

        particles.Play();
        return particles;
    }

    public static void PlayBugSpawn(Vector3 position, BugType type)
    {
        GameFeedback feedback = Ensure();
        Color color = BugDustColor(type);
        feedback.SpawnBurst(position + Vector3.up * 0.12f, color, 22, 0.12f, 0.42f, 0.2f, 0.55f, feedback.GetDustParticleMaterial());
        feedback.PlaySpatial(feedback.GetClip("spawn"), position, 0.28f, type == BugType.Spider ? 1.35f : 0.95f);
    }

    public static void PlayBugHit(Vector3 position, BugType type, float damage)
    {
        GameFeedback feedback = Ensure();
        Color color = type == BugType.Spider
            ? new Color(0.35f, 0.92f, 0.28f, 1f)
            : new Color(0.9f, 0.68f, 0.28f, 1f);
        feedback.SpawnBurst(position + Vector3.up * 0.25f, color, 14, 0.07f, 0.22f, 0.35f, 0.75f, feedback.GetSparkParticleMaterial());
        feedback.PlaySpatial(feedback.GetClip("hit"), position, Mathf.Clamp01(0.22f + damage * 0.08f), 0.9f + damage * 0.05f);
    }

    public static void PlayBugDestroyed(Vector3 position, BugType type)
    {
        GameFeedback feedback = Ensure();
        feedback.SpawnBurst(position + Vector3.up * 0.18f, BugDustColor(type), 36, 0.12f, 0.55f, 0.55f, 1.05f, feedback.GetDustParticleMaterial());
        feedback.SpawnBurst(position + Vector3.up * 0.28f, new Color(1f, 0.76f, 0.24f, 1f), 10, 0.05f, 0.18f, 0.45f, 0.9f, feedback.GetSparkParticleMaterial());
        feedback.PlaySpatial(feedback.GetClip("bug_down"), position, 0.42f, type == BugType.Beetle ? 0.7f : 1.05f);
    }

    public static void PlayVegetableDamage(Vector3 position, VegetableType type, float damage)
    {
        GameFeedback feedback = Ensure();
        feedback.SpawnBurst(position + Vector3.up * 0.18f, VegetableLeafColor(type), 12, 0.08f, 0.22f, 0.25f, 0.52f, feedback.GetLeafParticleMaterial());
        feedback.PlaySpatial(feedback.GetClip("vegetable_hit"), position, Mathf.Clamp01(0.18f + damage * 0.06f), 0.85f + Random.value * 0.25f);
    }

    public static void PlayVegetableDestroyed(Vector3 position, VegetableType type)
    {
        GameFeedback feedback = Ensure();
        feedback.SpawnBurst(position + Vector3.up * 0.22f, VegetableLeafColor(type), 28, 0.1f, 0.48f, 0.42f, 0.92f, feedback.GetLeafParticleMaterial());
        feedback.SpawnBurst(position + Vector3.up * 0.04f, new Color(0.48f, 0.31f, 0.16f, 1f), 20, 0.08f, 0.28f, 0.25f, 0.62f, feedback.GetDustParticleMaterial());
        feedback.PlaySpatial(feedback.GetClip("vegetable_lost"), position, 0.44f, 0.72f);
    }

    public static void PlayStickSwing(Vector3 position)
    {
        GameFeedback feedback = Ensure();
        feedback.PlaySpatial(feedback.GetClip("swing"), position, 0.24f, 0.95f + Random.value * 0.18f, 0.35f);
    }

    public static void PlayPlayerDamaged(Vector3 position, float damage)
    {
        GameFeedback feedback = Ensure();
        feedback.PlaySpatial(feedback.GetClip("player_hit"), position, Mathf.Clamp01(0.2f + damage * 0.025f), 0.9f);
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        SetupAmbientSource();
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    private void Update()
    {
        AudioListener.volume = GameData.Volume;

        if (_ambientSource == null)
        {
            return;
        }

        float nightProgress = GameData.NightDuration > 0f
            ? 1f - Mathf.Clamp01(GameData.NightTimeRemaining / GameData.NightDuration)
            : 0f;
        float danger = Mathf.Clamp01(GameData.AliveMeteorites / 10f);
        _ambientSource.volume = Mathf.Lerp(0.08f, 0.22f, Mathf.Max(nightProgress * 0.65f, danger));
        _ambientSource.pitch = Mathf.Lerp(0.88f, 1.08f, danger);
    }

    private static GameFeedback Ensure()
    {
        if (_instance != null)
        {
            return _instance;
        }

        GameFeedback existing = FindFirstObjectByType<GameFeedback>();
        if (existing != null)
        {
            _instance = existing;
            return _instance;
        }

        GameObject go = new GameObject("GameFeedback");
        _instance = go.AddComponent<GameFeedback>();
        return _instance;
    }

    private void SetupAmbientSource()
    {
        _ambientSource = gameObject.AddComponent<AudioSource>();
        _ambientSource.clip = GetClip("ambient");
        _ambientSource.loop = true;
        _ambientSource.playOnAwake = true;
        _ambientSource.spatialBlend = 0f;
        _ambientSource.volume = 0.12f;
        _ambientSource.Play();
    }

    private void SpawnBurst(
        Vector3 position,
        Color color,
        int count,
        float minSize,
        float maxSize,
        float minSpeed,
        float maxSpeed,
        Material material)
    {
        GameObject go = new GameObject("ParticleBurst");
        go.transform.position = position;

        ParticleSystem particles = go.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule main = particles.main;
        main.loop = false;
        main.playOnAwake = false;
        main.maxParticles = count;
        main.duration = 0.08f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.35f, 0.85f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(minSpeed, maxSpeed);
        main.startSize = new ParticleSystem.MinMaxCurve(minSize, maxSize);
        main.startColor = color;
        main.gravityModifier = 0.18f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[]
        {
            new ParticleSystem.Burst(0f, (short)count)
        });

        ParticleSystem.ShapeModule shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.18f;

        ParticleSystem.ColorOverLifetimeModule colorOverLifetime = particles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        colorOverLifetime.color = FadeGradient(color, 0.72f);

        ParticleSystemRenderer renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.material = material;
        renderer.renderMode = ParticleSystemRenderMode.Billboard;

        particles.Play();
        Destroy(go, 1.4f);
    }

    private void PlaySpatial(AudioClip clip, Vector3 position, float volume, float pitch, float spatialBlend = 0.85f)
    {
        if (clip == null)
        {
            return;
        }

        GameObject go = new GameObject("OneShotAudio_" + clip.name);
        go.transform.position = position;

        AudioSource source = go.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = Mathf.Clamp01(volume);
        source.pitch = Mathf.Clamp(pitch, 0.35f, 2.2f);
        source.spatialBlend = spatialBlend;
        source.minDistance = 2f;
        source.maxDistance = 22f;
        source.rolloffMode = AudioRolloffMode.Logarithmic;
        source.Play();

        Destroy(go, clip.length / Mathf.Max(0.1f, source.pitch) + 0.15f);
    }

    private AudioClip GetClip(string clipName)
    {
        if (_clips.TryGetValue(clipName, out AudioClip clip))
        {
            return clip;
        }

        if (UserClipResourcePaths.TryGetValue(clipName, out string resourcePath))
        {
            clip = Resources.Load<AudioClip>(resourcePath);
            if (clip != null)
            {
                _clips[clipName] = clip;
                return clip;
            }
        }

        switch (clipName)
        {
            case "ambient":
                clip = CreateAmbientClip();
                break;
            case "swing":
                clip = CreateToneClip("swing", 0.18f, 520f, 160f, 0.008f, 8.5f, 0.15f);
                break;
            case "hit":
                clip = CreateToneClip("hit", 0.16f, 160f, 70f, 0.002f, 18f, 0.45f);
                break;
            case "bug_down":
                clip = CreateToneClip("bug_down", 0.36f, 210f, 45f, 0.004f, 9f, 0.5f);
                break;
            case "spawn":
                clip = CreateToneClip("spawn", 0.22f, 90f, 220f, 0.01f, 5.5f, 0.35f);
                break;
            case "vegetable_hit":
                clip = CreateToneClip("vegetable_hit", 0.18f, 260f, 120f, 0.004f, 10f, 0.28f);
                break;
            case "vegetable_lost":
                clip = CreateToneClip("vegetable_lost", 0.44f, 180f, 42f, 0.008f, 7f, 0.4f);
                break;
            case "player_hit":
                clip = CreateToneClip("player_hit", 0.28f, 95f, 55f, 0.002f, 9f, 0.55f);
                break;
            default:
                clip = CreateToneClip(clipName, 0.15f, 220f, 110f, 0.004f, 12f, 0.25f);
                break;
        }

        _clips[clipName] = clip;
        return clip;
    }

    private Material GetDustParticleMaterial()
    {
        if (_dustParticleMaterial == null)
        {
            _dustParticleMaterial = CreateParticleMaterial(new Color(0.54f, 0.38f, 0.2f, 0.75f));
        }

        return _dustParticleMaterial;
    }

    private Material GetLeafParticleMaterial()
    {
        if (_leafParticleMaterial == null)
        {
            _leafParticleMaterial = CreateParticleMaterial(new Color(0.25f, 0.58f, 0.18f, 0.82f));
        }

        return _leafParticleMaterial;
    }

    private Material GetSparkParticleMaterial()
    {
        if (_sparkParticleMaterial == null)
        {
            _sparkParticleMaterial = CreateParticleMaterial(new Color(1f, 0.72f, 0.22f, 0.9f));
        }

        return _sparkParticleMaterial;
    }

    private static Material CreateParticleMaterial(Color tint)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (shader == null)
        {
            shader = Shader.Find("Particles/Standard Unlit");
        }
        if (shader == null)
        {
            shader = Shader.Find("Sprites/Default");
        }

        Material material = new Material(shader);
        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", tint);
        }
        if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", tint);
        }
        return material;
    }

    private static Gradient FadeGradient(Color color, float startAlpha)
    {
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(color, 0f),
                new GradientColorKey(color, 1f)
            },
            new[]
            {
                new GradientAlphaKey(startAlpha, 0f),
                new GradientAlphaKey(0f, 1f)
            });
        return gradient;
    }

    private static AudioClip CreateToneClip(
        string clipName,
        float duration,
        float startFrequency,
        float endFrequency,
        float attack,
        float decay,
        float noiseAmount)
    {
        int sampleCount = Mathf.Max(1, Mathf.CeilToInt(duration * SampleRate));
        float[] data = new float[sampleCount];
        System.Random random = new System.Random(clipName.GetHashCode());
        float phase = 0f;

        for (int i = 0; i < sampleCount; i++)
        {
            float t = i / (float)SampleRate;
            float normalized = Mathf.Clamp01(t / duration);
            float frequency = Mathf.Lerp(startFrequency, endFrequency, normalized);
            phase += frequency * Mathf.PI * 2f / SampleRate;

            float attackEnvelope = attack <= 0f ? 1f : Mathf.Clamp01(t / attack);
            float envelope = attackEnvelope * Mathf.Exp(-normalized * decay);
            float tone = Mathf.Sin(phase) * (1f - noiseAmount);
            float noise = ((float)random.NextDouble() * 2f - 1f) * noiseAmount;
            data[i] = Mathf.Clamp((tone + noise) * envelope * 0.7f, -1f, 1f);
        }

        AudioClip clip = AudioClip.Create(clipName, sampleCount, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private static AudioClip CreateAmbientClip()
    {
        const float duration = 4f;
        int sampleCount = Mathf.CeilToInt(duration * SampleRate);
        float[] data = new float[sampleCount];
        System.Random random = new System.Random(71931);
        float smoothedNoise = 0f;

        for (int i = 0; i < sampleCount; i++)
        {
            float t = i / (float)SampleRate;
            float slowWave = Mathf.Sin(t * Mathf.PI * 2f * 0.23f) * 0.12f;
            float midWave = Mathf.Sin(t * Mathf.PI * 2f * 1.1f + 0.8f) * 0.035f;
            smoothedNoise = Mathf.Lerp(smoothedNoise, (float)random.NextDouble() * 2f - 1f, 0.012f);
            data[i] = (slowWave + midWave + smoothedNoise * 0.12f) * 0.55f;
        }

        AudioClip clip = AudioClip.Create("ambient", sampleCount, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private static Color BugDustColor(BugType type)
    {
        switch (type)
        {
            case BugType.Beetle:
                return new Color(0.44f, 0.42f, 0.22f, 0.86f);
            case BugType.Spider:
                return new Color(0.24f, 0.24f, 0.2f, 0.82f);
            default:
                return new Color(0.48f, 0.31f, 0.16f, 0.82f);
        }
    }

    private static Color VegetableLeafColor(VegetableType type)
    {
        switch (type)
        {
            case VegetableType.Tomato:
                return new Color(0.68f, 0.12f, 0.08f, 0.88f);
            case VegetableType.Potato:
                return new Color(0.55f, 0.38f, 0.18f, 0.86f);
            case VegetableType.Carrot:
                return new Color(0.92f, 0.38f, 0.08f, 0.9f);
            default:
                return new Color(0.22f, 0.58f, 0.16f, 0.86f);
        }
    }
}

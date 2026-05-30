using UnityEngine;

public enum VegetableType
{
    Tomato,
    Potato,
    Carrot
}

[RequireComponent(typeof(SphereCollider))]
[DisallowMultipleComponent]
public class Vegetable : MonoBehaviour
{
    private const string VisualRootName = "VisualRoot";
    private const float StandardVisualScale = 3.15f;
    private const float TomatoVisualScale = 4.9f;
    private const float StandardColliderRadius = 1.12f;
    private const float TomatoColliderRadius = 1.32f;
    private const float StandardColliderCenterY = 1.02f;
    private const float TomatoColliderCenterY = 1.22f;
    private const float CarrotBurialDepth = 0.16f;

    public VegetableType type = VegetableType.Tomato;
    public float maxHealth = 55f;

    private float _currentHealth;
    private bool _isAlive = true;
    private Transform _modelRoot;

    public float CurrentHealth => _currentHealth;
    public bool IsAlive => _isAlive;

    private void Awake()
    {
        _currentHealth = maxHealth;
        SetupPhysics();
    }

    private void Start()
    {
        GardenManager.Instance?.RegisterVegetable(this);
    }

    public void Initialize(VegetableType vegetableType)
    {
        type = vegetableType;
        name = type + "_Vegetable";
        maxHealth = GetMaxHealthForType(type);
        _currentHealth = maxHealth;
        SetupPhysics();
        BuildModel();

        VegetableAnimator animator = GetComponent<VegetableAnimator>();
        if (animator == null)
        {
            animator = gameObject.AddComponent<VegetableAnimator>();
        }
        animator.Configure(type);
    }

    public void TakeDamage(float damage)
    {
        if (!_isAlive || damage <= 0f)
        {
            return;
        }

        _currentHealth -= damage;
        VegetableAnimator animator = GetComponent<VegetableAnimator>();
        if (animator != null)
        {
            animator.PlayHitReact();
        }

        if (_currentHealth > 0f)
        {
            GameFeedback.PlayVegetableDamage(transform.position, type, damage);
            return;
        }

        _isAlive = false;
        GameFeedback.PlayVegetableDestroyed(transform.position, type);
        GardenManager.Instance?.NotifyVegetableDestroyed(this);
        Destroy(gameObject);
    }

    private void SetupPhysics()
    {
        SphereCollider sphereCollider = GetComponent<SphereCollider>();
        sphereCollider.radius = GetColliderRadiusForType(type);
        sphereCollider.center = new Vector3(0f, GetColliderCenterYForType(type), 0f);
    }

    private void BuildModel()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        GameObject modelRootGo = new GameObject(VisualRootName);
        _modelRoot = modelRootGo.transform;
        _modelRoot.SetParent(transform, false);
        _modelRoot.localPosition = Vector3.zero;
        _modelRoot.localRotation = Quaternion.identity;
        _modelRoot.localScale = Vector3.one * GetVisualScaleForType(type);

        switch (type)
        {
            case VegetableType.Tomato:
                BuildTomato();
                break;
            case VegetableType.Potato:
                BuildPotato();
                break;
            case VegetableType.Carrot:
                BuildCarrot();
                break;
        }

        AlignModelToBase();
        if (type == VegetableType.Carrot)
        {
            _modelRoot.localPosition += Vector3.down * CarrotBurialDepth;
        }
    }

    private static float GetMaxHealthForType(VegetableType vegetableType)
    {
        switch (vegetableType)
        {
            case VegetableType.Tomato:
                return 55f;
            case VegetableType.Potato:
                return 70f;
            case VegetableType.Carrot:
                return 60f;
            default:
                return 55f;
        }
    }

    private static float GetVisualScaleForType(VegetableType vegetableType)
    {
        return vegetableType == VegetableType.Tomato ? TomatoVisualScale : StandardVisualScale;
    }

    private static float GetColliderRadiusForType(VegetableType vegetableType)
    {
        return vegetableType == VegetableType.Tomato ? TomatoColliderRadius : StandardColliderRadius;
    }

    private static float GetColliderCenterYForType(VegetableType vegetableType)
    {
        return vegetableType == VegetableType.Tomato ? TomatoColliderCenterY : StandardColliderCenterY;
    }

    private void BuildTomato()
    {
        Material red = MakeMat(new Color(0.75f, 0.05f, 0.04f));
        Material leaf = MakeMat(new Color(0.08f, 0.45f, 0.08f));
        Material stem = MakeMat(new Color(0.13f, 0.34f, 0.06f));

        GameObject body = Part("TomatoBody", PrimitiveType.Sphere, new Vector3(0f, 0.29f, 0f),
            new Vector3(0.58f, 0.56f, 0.58f), red);
        body.transform.SetParent(_modelRoot, false);
        body.transform.localRotation = Quaternion.Euler(0f, 18f, 0f);

        for (int i = 0; i < 5; i++)
        {
            GameObject top = Part("TomatoSepal", PrimitiveType.Sphere, new Vector3(0f, 0.63f, 0f),
                new Vector3(0.16f, 0.025f, 0.34f), leaf);
            top.transform.SetParent(_modelRoot, false);
            top.transform.localRotation = Quaternion.Euler(13f, i * 72f, 0f);
            top.transform.localPosition += top.transform.forward * 0.14f;
        }

        GameObject stemGo = Part("TomatoStem", PrimitiveType.Cylinder, new Vector3(0f, 0.74f, 0f),
            new Vector3(0.07f, 0.12f, 0.07f), stem);
        stemGo.transform.SetParent(_modelRoot, false);
        stemGo.transform.localRotation = Quaternion.Euler(12f, 0f, -14f);
    }

    private void BuildPotato()
    {
        Material potato = MakeMat(new Color(0.56f, 0.39f, 0.19f));
        Material eye = MakeMat(new Color(0.24f, 0.14f, 0.07f));
        Material sprout = MakeMat(new Color(0.18f, 0.42f, 0.12f));

        GameObject body = Part("PotatoBody", PrimitiveType.Sphere, new Vector3(0f, 0.35f, 0f),
            new Vector3(0.65f, 0.38f, 0.5f), potato);
        body.transform.SetParent(_modelRoot, false);
        body.transform.localRotation = Quaternion.Euler(0f, 25f, 8f);

        Vector3[] eyes =
        {
            new Vector3(-0.28f, 0.47f, 0.24f),
            new Vector3(0.2f, 0.56f, 0.18f),
            new Vector3(0.34f, 0.32f, -0.12f),
            new Vector3(-0.1f, 0.22f, -0.3f),
            new Vector3(-0.38f, 0.35f, -0.03f)
        };
        for (int i = 0; i < eyes.Length; i++)
        {
            GameObject dot = Part("PotatoEye", PrimitiveType.Sphere, eyes[i],
                new Vector3(0.055f, 0.025f, 0.055f), eye);
            dot.transform.SetParent(_modelRoot, false);
        }

        for (int i = 0; i < 5; i++)
        {
            GameObject sprig = Part("PotatoSprout", PrimitiveType.Cube,
                new Vector3((i - 2) * 0.13f, 0.72f, Random.Range(-0.08f, 0.08f)),
                new Vector3(0.05f, 0.35f, 0.05f), sprout);
            sprig.transform.SetParent(_modelRoot, false);
            sprig.transform.localRotation = Quaternion.Euler(Random.Range(-18f, 18f), i * 72f, Random.Range(-18f, 18f));
        }
    }

    private void BuildCarrot()
    {
        Material orange = MakeMat(new Color(0.95f, 0.35f, 0.05f));
        Material line = MakeMat(new Color(0.65f, 0.18f, 0.02f));
        Material leaf = MakeMat(new Color(0.12f, 0.55f, 0.12f));

        GameObject root = new GameObject("CarrotRoot");
        root.transform.SetParent(_modelRoot, false);
        root.transform.localPosition = new Vector3(0f, 0.42f, 0f);
        root.AddComponent<MeshFilter>().sharedMesh = CreateCarrotRootMesh();
        root.AddComponent<MeshRenderer>().material = orange;

        for (int i = 0; i < 5; i++)
        {
            GameObject ring = Part("CarrotGrowthLine", PrimitiveType.Cube,
                new Vector3(0f, 0.25f + i * 0.13f, 0.25f - i * 0.035f),
                new Vector3(0.32f - i * 0.035f, 0.018f, 0.018f), line);
            ring.transform.SetParent(_modelRoot, false);
            ring.transform.localRotation = Quaternion.Euler(Random.Range(-7f, 7f), Random.Range(-18f, 18f), Random.Range(-5f, 5f));
        }

        for (int i = 0; i < 8; i++)
        {
            GameObject top = Part("CarrotTop", PrimitiveType.Cube, new Vector3(0f, 0.85f, 0f),
                new Vector3(0.045f, Random.Range(0.42f, 0.75f), 0.045f), leaf);
            top.transform.SetParent(_modelRoot, false);
            top.transform.localRotation = Quaternion.Euler(Random.Range(-32f, 32f), i * 45f, Random.Range(-28f, 28f));
        }
    }

    private static Mesh CreateCarrotRootMesh()
    {
        const int segments = 18;
        Mesh mesh = new Mesh();
        mesh.name = "Tapered Carrot Root";

        Vector3[] vertices = new Vector3[segments * 3 + 2];
        int[] triangles = new int[segments * 18];
        int topCenter = vertices.Length - 2;
        int bottomTip = vertices.Length - 1;

        for (int ring = 0; ring < 3; ring++)
        {
            float y = Mathf.Lerp(0.38f, -0.48f, ring / 2f);
            float radius = Mathf.Lerp(0.32f, 0.08f, ring / 2f);
            for (int segment = 0; segment < segments; segment++)
            {
                float angle = segment / (float)segments * Mathf.PI * 2f;
                vertices[ring * segments + segment] = new Vector3(Mathf.Cos(angle) * radius, y, Mathf.Sin(angle) * radius);
            }
        }

        vertices[topCenter] = new Vector3(0f, 0.44f, 0f);
        vertices[bottomTip] = new Vector3(0f, -0.72f, 0f);

        int t = 0;
        for (int segment = 0; segment < segments; segment++)
        {
            int next = (segment + 1) % segments;
            for (int ring = 0; ring < 2; ring++)
            {
                int a = ring * segments + segment;
                int b = ring * segments + next;
                int c = (ring + 1) * segments + segment;
                int d = (ring + 1) * segments + next;
                triangles[t++] = a;
                triangles[t++] = b;
                triangles[t++] = c;
                triangles[t++] = c;
                triangles[t++] = b;
                triangles[t++] = d;
            }

            triangles[t++] = topCenter;
            triangles[t++] = segment;
            triangles[t++] = next;

            triangles[t++] = 2 * segments + next;
            triangles[t++] = 2 * segments + segment;
            triangles[t++] = bottomTip;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    private void AlignModelToBase()
    {
        if (_modelRoot == null)
        {
            return;
        }

        float minY = float.MaxValue;
        MeshFilter[] meshFilters = _modelRoot.GetComponentsInChildren<MeshFilter>();
        foreach (MeshFilter meshFilter in meshFilters)
        {
            Mesh mesh = meshFilter.sharedMesh;
            if (mesh == null)
            {
                continue;
            }

            minY = Mathf.Min(minY, GetMeshMinYInVegetableSpace(meshFilter, mesh.bounds));
        }

        if (minY < float.MaxValue)
        {
            _modelRoot.localPosition -= Vector3.up * minY;
        }
    }

    private float GetMeshMinYInVegetableSpace(MeshFilter meshFilter, Bounds bounds)
    {
        float minY = float.MaxValue;
        Vector3 min = bounds.min;
        Vector3 max = bounds.max;

        for (int x = 0; x < 2; x++)
        {
            for (int y = 0; y < 2; y++)
            {
                for (int z = 0; z < 2; z++)
                {
                    Vector3 meshCorner = new Vector3(
                        x == 0 ? min.x : max.x,
                        y == 0 ? min.y : max.y,
                        z == 0 ? min.z : max.z);
                    Vector3 worldCorner = meshFilter.transform.TransformPoint(meshCorner);
                    minY = Mathf.Min(minY, transform.InverseTransformPoint(worldCorner).y);
                }
            }
        }

        return minY;
    }

    private static GameObject Part(string partName, PrimitiveType primitiveType, Vector3 localPosition,
        Vector3 localScale, Material material)
    {
        GameObject go = GameObject.CreatePrimitive(primitiveType);
        go.name = partName;
        go.transform.localPosition = localPosition;
        go.transform.localScale = localScale;
        go.GetComponent<MeshRenderer>().material = material;
        Destroy(go.GetComponent<Collider>());
        return go;
    }

    private static Material MakeMat(Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        Material material = new Material(shader);
        material.color = color;
        return material;
    }
}

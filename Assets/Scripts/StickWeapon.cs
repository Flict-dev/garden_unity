using UnityEngine;

[DisallowMultipleComponent]
public class StickWeapon : MonoBehaviour
{
    public float swingDuration = 0.25f;
    public float swingCooldown = 0.45f;
    public float damage = 1.5f;
    public float hitRadius = 1.25f;
    public float hitForwardOffset = 1f;

    private Transform _stickPivot;
    private Transform _stickModel;
    private float _swingTimer;
    private float _cooldownTimer;
    private bool _isSwinging;
    private bool _hasHitThisSwing;

    private const int IgnoreRaycastLayer = 2;
    private static readonly Vector3 IdleEuler = new Vector3(-9f, -16f, -6f);

    private void Awake()
    {
        BuildStickModel();
    }

    private void Update()
    {
        _cooldownTimer -= Time.deltaTime;

        if (_isSwinging)
        {
            _swingTimer += Time.deltaTime;
            float t = Mathf.Clamp01(_swingTimer / swingDuration);

            // Swing arc: from up-right to down-left while keeping the weapon angled in hand.
            float angle = Mathf.Lerp(32f, -54f, EaseOutCubic(t));
            SetStickPitch(angle);

            // Hit detection at the peak of the swing (middle of arc)
            if (!_hasHitThisSwing && t > 0.3f)
            {
                DetectHits();
                _hasHitThisSwing = true;
            }

            if (t >= 1f)
            {
                _isSwinging = false;
                // Return to idle position
                _stickPivot.localRotation = Quaternion.Euler(IdleEuler);
            }
        }
    }

    public bool TrySwing()
    {
        if (_isSwinging || _cooldownTimer > 0f)
        {
            return false;
        }

        _isSwinging = true;
        _hasHitThisSwing = false;
        _swingTimer = 0f;
        _cooldownTimer = swingCooldown;
        GameFeedback.PlayStickSwing(transform.position);
        return true;
    }

    private void DetectHits()
    {
        // Hit sphere in front of player
        Vector3 hitCenter = transform.position + transform.forward * hitForwardOffset;
        Collider[] hits = Physics.OverlapSphere(hitCenter, hitRadius, ~0, QueryTriggerInteraction.Ignore);

        foreach (Collider hit in hits)
        {
            MeteorMover bug = hit.GetComponentInParent<MeteorMover>();
            if (bug != null)
            {
                bug.TakeDamage(damage);
            }
        }
    }

    private void BuildStickModel()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        Material blackMat = MakeOpaqueMaterial(shader, new Color(0.006f, 0.006f, 0.005f), 0.44f);
        Material softEdgeMat = MakeOpaqueMaterial(shader, new Color(0.018f, 0.018f, 0.016f), 0.38f);

        GameObject pivotGo = new GameObject("FlySwatterPivot");
        _stickPivot = pivotGo.transform;
        _stickPivot.SetParent(transform, false);
        _stickPivot.localPosition = new Vector3(0.35f, -0.34f, 0.5f);
        _stickPivot.localRotation = Quaternion.Euler(IdleEuler);

        Vector3 handleBase = new Vector3(0.1f, -0.18f, 0.06f);
        Vector3 headSocket = new Vector3(-0.22f, 0.03f, 1.14f);
        Vector3 handleAxis = (headSocket - handleBase).normalized;
        Vector3 headWidthAxis = Vector3.Cross(Vector3.up, handleAxis).normalized;
        Vector3 gripEnd = Vector3.Lerp(handleBase, headSocket, 0.26f);
        Vector3 shaftEnd = headSocket - handleAxis * 0.12f;
        Vector3 headCenter = headSocket + handleAxis * 0.31f;

        CreateSegment(_stickPivot, "SwatterGrip", handleBase, gripEnd, 0.075f, softEdgeMat);
        CreateCap(_stickPivot, "SwatterGripEndCap", handleBase, 0.079f, softEdgeMat);
        CreateCap(_stickPivot, "SwatterGripTopCap", gripEnd, 0.071f, softEdgeMat);
        CreateSegment(_stickPivot, "SwatterShaft", gripEnd, shaftEnd, 0.026f, blackMat);
        CreateSegment(_stickPivot, "SwatterCollar", shaftEnd, headSocket, 0.052f, blackMat);
        CreateCap(_stickPivot, "SwatterCollarCap", headSocket, 0.055f, blackMat);

        CreateFlySwatterHead(_stickPivot, "SwatterHead", headCenter, 0.54f, 0.62f, 0.014f,
            blackMat, headWidthAxis, handleAxis);

        _stickModel = _stickPivot;
    }

    private void SetStickPitch(float pitch)
    {
        _stickPivot.localRotation = Quaternion.Euler(pitch, IdleEuler.y, IdleEuler.z);
    }

    private static GameObject CreateSegment(Transform parent, string name, Vector3 start, Vector3 end,
        float radius, Material material)
    {
        GameObject segment = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        segment.name = name;
        segment.transform.SetParent(parent, false);

        Vector3 direction = end - start;
        segment.transform.localPosition = (start + end) * 0.5f;
        segment.transform.localRotation = Quaternion.FromToRotation(Vector3.up, direction.normalized);
        segment.transform.localScale = new Vector3(radius, direction.magnitude * 0.5f, radius);

        PrepareVisual(segment, material);
        return segment;
    }

    private static void CreateCap(Transform parent, string name, Vector3 localPosition,
        float radius, Material material)
    {
        GameObject cap = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        cap.name = name;
        cap.transform.SetParent(parent, false);
        cap.transform.localPosition = localPosition;
        cap.transform.localScale = Vector3.one * radius;
        PrepareVisual(cap, material);
    }

    private static void CreateFlySwatterHead(Transform parent, string name, Vector3 center, float width,
        float height, float lineRadius, Material material, Vector3 widthAxis, Vector3 heightAxis)
    {
        const int cornerSegments = 7;
        float cornerRadius = Mathf.Min(width, height) * 0.22f;
        float zOffset = -0.012f;
        widthAxis.Normalize();
        heightAxis.Normalize();
        Vector3 normalAxis = Vector3.Cross(widthAxis, heightAxis).normalized;
        Vector2[] outline = CreateRoundedHeadOutline(width, height, cornerRadius, cornerSegments);

        for (int i = 0; i < outline.Length; i++)
        {
            Vector2 a = outline[i];
            Vector2 b = outline[(i + 1) % outline.Length];
            CreateSegment(
                parent,
                name + "Rim" + i,
                HeadPoint(center, widthAxis, heightAxis, normalAxis, a.x, a.y, zOffset),
                HeadPoint(center, widthAxis, heightAxis, normalAxis, b.x, b.y, zOffset),
                lineRadius * 1.6f,
                material
            );
        }

        float inset = 0.045f;
        float spacing = 0.045f;
        int verticalIndex = 0;
        for (float x = -width * 0.5f + inset; x <= width * 0.5f - inset; x += spacing)
        {
            float yExtent = RoundedRectYExtent(x, width, height, cornerRadius, inset);
            CreateSegment(
                parent,
                name + "GridVertical" + verticalIndex,
                HeadPoint(center, widthAxis, heightAxis, normalAxis, x, -yExtent, zOffset),
                HeadPoint(center, widthAxis, heightAxis, normalAxis, x, yExtent, zOffset),
                lineRadius,
                material
            );
            verticalIndex++;
        }

        int horizontalIndex = 0;
        for (float y = -height * 0.5f + inset; y <= height * 0.5f - inset; y += spacing)
        {
            float xExtent = RoundedRectXExtent(y, width, height, cornerRadius, inset);
            CreateSegment(
                parent,
                name + "GridHorizontal" + horizontalIndex,
                HeadPoint(center, widthAxis, heightAxis, normalAxis, -xExtent, y, zOffset),
                HeadPoint(center, widthAxis, heightAxis, normalAxis, xExtent, y, zOffset),
                lineRadius,
                material
            );
            horizontalIndex++;
        }
    }

    private static Vector3 HeadPoint(Vector3 center, Vector3 widthAxis, Vector3 heightAxis, Vector3 normalAxis,
        float x, float y, float z)
    {
        return center + widthAxis * x + heightAxis * y + normalAxis * z;
    }

    private static Vector2[] CreateRoundedHeadOutline(float width, float height, float radius, int segmentsPerCorner)
    {
        Vector2[] points = new Vector2[segmentsPerCorner * 4];
        float halfWidth = width * 0.5f;
        float halfHeight = height * 0.5f;
        int index = 0;

        AddRoundedCorner(points, ref index, new Vector2(halfWidth - radius, halfHeight - radius), radius, 0f, 90f, segmentsPerCorner);
        AddRoundedCorner(points, ref index, new Vector2(-halfWidth + radius, halfHeight - radius), radius, 90f, 180f, segmentsPerCorner);
        AddRoundedCorner(points, ref index, new Vector2(-halfWidth + radius, -halfHeight + radius), radius, 180f, 270f, segmentsPerCorner);
        AddRoundedCorner(points, ref index, new Vector2(halfWidth - radius, -halfHeight + radius), radius, 270f, 360f, segmentsPerCorner);

        return points;
    }

    private static void AddRoundedCorner(Vector2[] points, ref int index, Vector2 center,
        float radius, float startAngle, float endAngle, int segments)
    {
        for (int i = 0; i < segments; i++)
        {
            float t = segments <= 1 ? 0f : i / (segments - 1f);
            float angle = Mathf.Lerp(startAngle, endAngle, t) * Mathf.Deg2Rad;
            points[index] = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            index++;
        }
    }

    private static float RoundedRectYExtent(float x, float width, float height, float radius, float inset)
    {
        float halfWidth = width * 0.5f - inset;
        float halfHeight = height * 0.5f - inset;
        radius = Mathf.Min(radius, halfWidth, halfHeight);
        float absX = Mathf.Abs(x);
        float innerX = halfWidth - radius;

        if (absX <= innerX)
        {
            return halfHeight;
        }

        float dx = Mathf.Min(absX - innerX, radius);
        return halfHeight - radius + Mathf.Sqrt(Mathf.Max(0f, radius * radius - dx * dx));
    }

    private static float RoundedRectXExtent(float y, float width, float height, float radius, float inset)
    {
        float halfWidth = width * 0.5f - inset;
        float halfHeight = height * 0.5f - inset;
        radius = Mathf.Min(radius, halfWidth, halfHeight);
        float absY = Mathf.Abs(y);
        float innerY = halfHeight - radius;

        if (absY <= innerY)
        {
            return halfWidth;
        }

        float dy = Mathf.Min(absY - innerY, radius);
        return halfWidth - radius + Mathf.Sqrt(Mathf.Max(0f, radius * radius - dy * dy));
    }

    private static void PrepareVisual(GameObject go, Material material)
    {
        MeshRenderer renderer = go.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.material = material;
            renderer.receiveShadows = false;
        }

        go.layer = IgnoreRaycastLayer;

        Collider collider = go.GetComponent<Collider>();
        if (collider != null)
        {
            Object.Destroy(collider);
        }
    }

    private static Material MakeOpaqueMaterial(Shader shader, Color color, float smoothness)
    {
        Material material = new Material(shader);
        material.color = color;

        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }

        if (material.HasProperty("_Surface"))
        {
            material.SetFloat("_Surface", 0f);
        }

        if (material.HasProperty("_Blend"))
        {
            material.SetFloat("_Blend", 0f);
        }

        if (material.HasProperty("_AlphaClip"))
        {
            material.SetFloat("_AlphaClip", 0f);
        }

        if (material.HasProperty("_ZWrite"))
        {
            material.SetFloat("_ZWrite", 1f);
        }

        if (material.HasProperty("_Smoothness"))
        {
            material.SetFloat("_Smoothness", smoothness);
        }

        material.SetOverrideTag("RenderType", "Opaque");
        material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = 2000;
        return material;
    }

    private static float EaseOutCubic(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3f);
    }
}

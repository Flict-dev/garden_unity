using UnityEngine;

public static class RemotePlayerVisualBuilder
{
    public static GameObject Build(ulong clientId)
    {
        GameObject root = new GameObject("RemotePlayer_" + clientId);

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        Material skin = MakeMaterial(shader, new Color(0.92f, 0.68f, 0.48f));
        Material shirt = MakeMaterial(shader, new Color(0.18f, 0.52f, 0.32f));
        Material overalls = MakeMaterial(shader, new Color(0.1f, 0.24f, 0.42f));
        Material boots = MakeMaterial(shader, new Color(0.07f, 0.055f, 0.045f));
        Material hat = MakeMaterial(shader, new Color(0.78f, 0.58f, 0.26f));
        Material tool = MakeMaterial(shader, new Color(0.02f, 0.02f, 0.02f));

        CreateBox(root.transform, "Body", new Vector3(0f, 0.95f, 0f),
            new Vector3(0.48f, 0.62f, 0.28f), shirt);
        CreateBox(root.transform, "Overalls", new Vector3(0f, 0.86f, -0.01f),
            new Vector3(0.36f, 0.48f, 0.3f), overalls);
        CreateSphere(root.transform, "Head", new Vector3(0f, 1.42f, 0f),
            new Vector3(0.34f, 0.34f, 0.34f), skin);

        CreateBox(root.transform, "HatBrim", new Vector3(0f, 1.6f, 0.04f),
            new Vector3(0.54f, 0.06f, 0.42f), hat);
        CreateCylinder(root.transform, "HatTop", new Vector3(0f, 1.68f, 0f),
            Quaternion.identity, new Vector3(0.18f, 0.08f, 0.18f), hat);

        CreateBox(root.transform, "LeftArm", new Vector3(-0.34f, 1.02f, 0f),
            new Vector3(0.16f, 0.48f, 0.16f), skin);
        CreateBox(root.transform, "RightArm", new Vector3(0.34f, 1.02f, 0f),
            new Vector3(0.16f, 0.48f, 0.16f), skin);
        CreateBox(root.transform, "LeftLeg", new Vector3(-0.13f, 0.38f, 0f),
            new Vector3(0.18f, 0.52f, 0.18f), overalls);
        CreateBox(root.transform, "RightLeg", new Vector3(0.13f, 0.38f, 0f),
            new Vector3(0.18f, 0.52f, 0.18f), overalls);
        CreateBox(root.transform, "LeftBoot", new Vector3(-0.13f, 0.1f, 0.05f),
            new Vector3(0.2f, 0.12f, 0.28f), boots);
        CreateBox(root.transform, "RightBoot", new Vector3(0.13f, 0.1f, 0.05f),
            new Vector3(0.2f, 0.12f, 0.28f), boots);

        CreateCylinder(root.transform, "SwatterHandle", new Vector3(0.55f, 0.98f, 0.12f),
            Quaternion.Euler(28f, 0f, -24f), new Vector3(0.025f, 0.46f, 0.025f), tool);
        CreateBox(root.transform, "SwatterHead", new Vector3(0.72f, 1.22f, 0.22f),
            new Vector3(0.28f, 0.04f, 0.34f), tool);

        return root;
    }

    private static GameObject CreateBox(Transform parent, string name, Vector3 localPosition,
        Vector3 localScale, Material material)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Prepare(go, parent, name, localPosition, Quaternion.identity, localScale, material);
        return go;
    }

    private static GameObject CreateSphere(Transform parent, string name, Vector3 localPosition,
        Vector3 localScale, Material material)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Prepare(go, parent, name, localPosition, Quaternion.identity, localScale, material);
        return go;
    }

    private static GameObject CreateCylinder(Transform parent, string name, Vector3 localPosition,
        Quaternion localRotation, Vector3 localScale, Material material)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        Prepare(go, parent, name, localPosition, localRotation, localScale, material);
        return go;
    }

    private static void Prepare(GameObject go, Transform parent, string name, Vector3 localPosition,
        Quaternion localRotation, Vector3 localScale, Material material)
    {
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPosition;
        go.transform.localRotation = localRotation;
        go.transform.localScale = localScale;

        MeshRenderer renderer = go.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.material = material;
            renderer.receiveShadows = false;
        }

        Collider collider = go.GetComponent<Collider>();
        if (collider != null)
        {
            Object.Destroy(collider);
        }
    }

    private static Material MakeMaterial(Shader shader, Color color)
    {
        Material material = new Material(shader);
        material.color = color;
        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }

        if (material.HasProperty("_Smoothness"))
        {
            material.SetFloat("_Smoothness", 0.35f);
        }

        return material;
    }
}

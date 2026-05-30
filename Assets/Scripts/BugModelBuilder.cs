using UnityEngine;

public enum BugType
{
    Ant,
    Beetle,
    Spider
}

public static class BugModelBuilder
{
    public static void Build(GameObject root, BugType type)
    {
        for (int i = root.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = root.transform.GetChild(i);
            if (child.name == "BugDustTrail")
            {
                continue;
            }

            Object.Destroy(child.gameObject);
        }

        MeshRenderer rootMr = root.GetComponent<MeshRenderer>();
        if (rootMr != null)
        {
            rootMr.enabled = false;
        }

        MeshFilter rootMf = root.GetComponent<MeshFilter>();
        if (rootMf != null)
        {
            rootMf.mesh = null;
        }

        switch (type)
        {
            case BugType.Ant:
                BuildAnt(root);
                break;
            case BugType.Beetle:
                BuildBeetle(root);
                break;
            case BugType.Spider:
                BuildSpider(root);
                break;
        }
    }

    private static void BuildAnt(GameObject root)
    {
        Material body = MakeMat(new Color(0.05f, 0.03f, 0.02f));
        Material legs = MakeMat(new Color(0.08f, 0.05f, 0.03f));
        Material eyes = MakeEyeMat(new Color(0.6f, 0.1f, 0.05f));

        GameObject model = new GameObject("AntModel");
        model.transform.SetParent(root.transform, false);
        model.transform.localScale = Vector3.one * 0.5f;

        Part(model, "Head", PrimitiveType.Sphere, new Vector3(0, 0.2f, 0.7f),
            new Vector3(0.3f, 0.25f, 0.3f), body);
        Part(model, "EyeL", PrimitiveType.Sphere, new Vector3(-0.12f, 0.28f, 0.85f),
            new Vector3(0.08f, 0.08f, 0.08f), eyes);
        Part(model, "EyeR", PrimitiveType.Sphere, new Vector3(0.12f, 0.28f, 0.85f),
            new Vector3(0.08f, 0.08f, 0.08f), eyes);

        GameObject antL = Part(model, "AntennaL", PrimitiveType.Capsule,
            new Vector3(-0.08f, 0.35f, 0.9f), new Vector3(0.02f, 0.25f, 0.02f), legs);
        antL.transform.localRotation = Quaternion.Euler(-40f, -20f, 0f);
        GameObject antR = Part(model, "AntennaR", PrimitiveType.Capsule,
            new Vector3(0.08f, 0.35f, 0.9f), new Vector3(0.02f, 0.25f, 0.02f), legs);
        antR.transform.localRotation = Quaternion.Euler(-40f, 20f, 0f);

        Part(model, "Thorax", PrimitiveType.Sphere, new Vector3(0, 0.2f, 0.2f),
            new Vector3(0.25f, 0.22f, 0.4f), body);
        Part(model, "PetioleFront", PrimitiveType.Sphere, new Vector3(0, 0.2f, -0.14f),
            new Vector3(0.13f, 0.12f, 0.13f), body);
        Part(model, "Abdomen", PrimitiveType.Sphere, new Vector3(0, 0.22f, -0.5f),
            new Vector3(0.3f, 0.28f, 0.45f), body);
        Part(model, "AbdomenHighlight", PrimitiveType.Sphere, new Vector3(0, 0.39f, -0.48f),
            new Vector3(0.21f, 0.05f, 0.33f), MakeMat(new Color(0.11f, 0.07f, 0.04f)));

        float[] legZ = { 0.35f, 0.15f, -0.05f };
        for (int i = 0; i < 3; i++)
        {
            MakeLeg(model, "L" + i, new Vector3(-0.15f, 0.1f, legZ[i]), 50f + i * 10f, true, legs);
            MakeLeg(model, "R" + i, new Vector3(0.15f, 0.1f, legZ[i]), -(50f + i * 10f), false, legs);
        }
    }

    private static void BuildBeetle(GameObject root)
    {
        Material shell = MakeMat(new Color(0.12f, 0.18f, 0.06f));
        Material under = MakeMat(new Color(0.08f, 0.06f, 0.03f));
        Material legs = MakeMat(new Color(0.1f, 0.07f, 0.03f));
        Material eyes = MakeEyeMat(new Color(0.8f, 0.6f, 0.1f));

        GameObject model = new GameObject("BeetleModel");
        model.transform.SetParent(root.transform, false);
        model.transform.localScale = Vector3.one * 0.7f;

        Part(model, "Shell", PrimitiveType.Sphere, new Vector3(0, 0.3f, 0),
            new Vector3(0.8f, 0.45f, 1.0f), shell);
        Part(model, "ShellRidgeL", PrimitiveType.Cube, new Vector3(-0.22f, 0.56f, -0.04f),
            new Vector3(0.025f, 0.02f, 0.78f), under);
        Part(model, "ShellRidgeR", PrimitiveType.Cube, new Vector3(0.22f, 0.56f, -0.04f),
            new Vector3(0.025f, 0.02f, 0.78f), under);
        Part(model, "Under", PrimitiveType.Sphere, new Vector3(0, 0.1f, 0),
            new Vector3(0.7f, 0.2f, 0.9f), under);
        Part(model, "Head", PrimitiveType.Sphere, new Vector3(0, 0.2f, 0.55f),
            new Vector3(0.35f, 0.25f, 0.3f), under);
        Part(model, "EyeL", PrimitiveType.Sphere, new Vector3(-0.15f, 0.28f, 0.7f),
            new Vector3(0.08f, 0.08f, 0.08f), eyes);
        Part(model, "EyeR", PrimitiveType.Sphere, new Vector3(0.15f, 0.28f, 0.7f),
            new Vector3(0.08f, 0.08f, 0.08f), eyes);
        Part(model, "ShellLine", PrimitiveType.Cube, new Vector3(0, 0.52f, 0),
            new Vector3(0.03f, 0.02f, 0.85f), under);

        Material spot = MakeMat(new Color(0.03f, 0.08f, 0.025f));
        for (int i = 0; i < 4; i++)
        {
            float side = i % 2 == 0 ? -1f : 1f;
            float z = i < 2 ? -0.22f : 0.18f;
            GameObject dot = Part(model, "ShellSpot" + i, PrimitiveType.Sphere,
                new Vector3(side * 0.32f, 0.58f, z), new Vector3(0.11f, 0.025f, 0.13f), spot);
            dot.transform.localRotation = Quaternion.Euler(0f, 0f, side * 8f);
        }

        GameObject mL = Part(model, "MandL", PrimitiveType.Capsule,
            new Vector3(-0.1f, 0.15f, 0.72f), new Vector3(0.05f, 0.1f, 0.05f), legs);
        mL.transform.localRotation = Quaternion.Euler(60f, -20f, 0f);
        GameObject mR = Part(model, "MandR", PrimitiveType.Capsule,
            new Vector3(0.1f, 0.15f, 0.72f), new Vector3(0.05f, 0.1f, 0.05f), legs);
        mR.transform.localRotation = Quaternion.Euler(60f, 20f, 0f);

        float[] legZ = { 0.25f, 0f, -0.25f };
        for (int i = 0; i < 3; i++)
        {
            MakeShortLeg(model, "L" + i, new Vector3(-0.35f, 0.05f, legZ[i]), 60f, true, legs);
            MakeShortLeg(model, "R" + i, new Vector3(0.35f, 0.05f, legZ[i]), -60f, false, legs);
        }
    }

    private static void BuildSpider(GameObject root)
    {
        Material body = MakeMat(new Color(0.2f, 0.18f, 0.15f));
        Material legs = MakeMat(new Color(0.15f, 0.12f, 0.08f));
        Material eyes = MakeEyeMat(new Color(0.1f, 0.9f, 0.2f));

        GameObject model = new GameObject("SpiderModel");
        model.transform.SetParent(root.transform, false);
        model.transform.localScale = Vector3.one * 0.45f;

        Part(model, "Body", PrimitiveType.Sphere, new Vector3(0, 0.25f, 0.15f),
            new Vector3(0.5f, 0.35f, 0.5f), body);
        Part(model, "Abdomen", PrimitiveType.Sphere, new Vector3(0, 0.3f, -0.45f),
            new Vector3(0.55f, 0.45f, 0.6f), body);

        Material stripe = MakeMat(new Color(0.35f, 0.31f, 0.23f));
        for (int i = 0; i < 3; i++)
        {
            GameObject band = Part(model, "AbdomenBand" + i, PrimitiveType.Cube,
                new Vector3(0f, 0.56f, -0.65f + i * 0.18f), new Vector3(0.42f, 0.018f, 0.045f), stripe);
            band.transform.localRotation = Quaternion.Euler(0f, 0f, i == 1 ? 0f : 8f);
        }

        Part(model, "Eye1", PrimitiveType.Sphere, new Vector3(-0.08f, 0.35f, 0.4f),
            new Vector3(0.07f, 0.07f, 0.07f), eyes);
        Part(model, "Eye2", PrimitiveType.Sphere, new Vector3(0.08f, 0.35f, 0.4f),
            new Vector3(0.07f, 0.07f, 0.07f), eyes);
        Part(model, "Eye3", PrimitiveType.Sphere, new Vector3(-0.15f, 0.32f, 0.38f),
            new Vector3(0.05f, 0.05f, 0.05f), eyes);
        Part(model, "Eye4", PrimitiveType.Sphere, new Vector3(0.15f, 0.32f, 0.38f),
            new Vector3(0.05f, 0.05f, 0.05f), eyes);

        GameObject fL = Part(model, "FangL", PrimitiveType.Capsule,
            new Vector3(-0.06f, 0.12f, 0.42f), new Vector3(0.03f, 0.08f, 0.03f), legs);
        fL.transform.localRotation = Quaternion.Euler(30f, 0f, 0f);
        GameObject fR = Part(model, "FangR", PrimitiveType.Capsule,
            new Vector3(0.06f, 0.12f, 0.42f), new Vector3(0.03f, 0.08f, 0.03f), legs);
        fR.transform.localRotation = Quaternion.Euler(30f, 0f, 0f);
        Part(model, "Spinnerets", PrimitiveType.Sphere, new Vector3(0f, 0.22f, -0.96f),
            new Vector3(0.1f, 0.07f, 0.08f), legs);

        float[] legZ = { 0.3f, 0.1f, -0.1f, -0.3f };
        float[] legAngles = { 35f, 55f, 55f, 35f };
        for (int i = 0; i < 4; i++)
        {
            MakeSpiderLeg(model, "L" + i, new Vector3(-0.22f, 0.2f, legZ[i]), legAngles[i], true, legs);
            MakeSpiderLeg(model, "R" + i, new Vector3(0.22f, 0.2f, legZ[i]), legAngles[i], false, legs);
        }
    }

    private static GameObject Part(GameObject parent, string name, PrimitiveType type,
        Vector3 localPos, Vector3 localScale, Material mat)
    {
        GameObject go = GameObject.CreatePrimitive(type);
        go.name = name;
        go.transform.SetParent(parent.transform, false);
        go.transform.localPosition = localPos;
        go.transform.localScale = localScale;
        go.GetComponent<MeshRenderer>().material = mat;
        go.layer = 2;
        Object.Destroy(go.GetComponent<Collider>());
        return go;
    }

    private static void MakeLeg(GameObject parent, string name, Vector3 attach,
        float zAngle, bool isLeft, Material mat)
    {
        GameObject upper = Part(parent, "Leg" + name, PrimitiveType.Capsule,
            attach, new Vector3(0.03f, 0.2f, 0.03f), mat);
        upper.transform.localRotation = Quaternion.Euler(0, 0, zAngle);

        GameObject lower = Part(upper, "Lower", PrimitiveType.Capsule,
            new Vector3(0, -0.85f, 0), new Vector3(0.7f, 0.75f, 0.7f), mat);
        lower.transform.localRotation = Quaternion.Euler(0, 0, isLeft ? -35f : 35f);
    }

    private static void MakeShortLeg(GameObject parent, string name, Vector3 attach,
        float zAngle, bool isLeft, Material mat)
    {
        GameObject leg = Part(parent, "Leg" + name, PrimitiveType.Capsule,
            attach, new Vector3(0.06f, 0.12f, 0.06f), mat);
        leg.transform.localRotation = Quaternion.Euler(0, 0, isLeft ? zAngle : -zAngle);
    }

    private static void MakeSpiderLeg(GameObject parent, string name, Vector3 attach,
        float spreadAngle, bool isLeft, Material mat)
    {
        float side = isLeft ? 1f : -1f;

        GameObject upper = Part(parent, "Leg" + name, PrimitiveType.Capsule,
            attach, new Vector3(0.025f, 0.28f, 0.025f), mat);
        upper.transform.localRotation = Quaternion.Euler(0, 0, side * spreadAngle);

        GameObject lower = Part(upper, "Lower", PrimitiveType.Capsule,
            new Vector3(0, -0.85f, 0), new Vector3(0.8f, 0.9f, 0.8f), mat);
        lower.transform.localRotation = Quaternion.Euler(0, 0, -side * 50f);
    }

    private static Material MakeMat(Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        Material mat = new Material(shader);
        mat.color = color;
        return mat;
    }

    private static Material MakeEyeMat(Color color)
    {
        Material mat = MakeMat(color);
        mat.SetColor("_EmissionColor", color * 0.5f);
        mat.EnableKeyword("_EMISSION");
        return mat;
    }
}

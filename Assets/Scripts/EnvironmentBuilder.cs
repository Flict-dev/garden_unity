using UnityEngine;

[DisallowMultipleComponent]
public class EnvironmentBuilder : MonoBehaviour
{
    public int grassBladeCount = 65;
    public int pebbleCount = 18;
    public int twigCount = 8;
    public int mushroomCount = 4;
    public int dirtPatchCount = 12;
    public int thickStemPlantCount = 12;
    public float arenaRadius = 22f;

    private const string PolyHavenTexturePath = "Textures/PolyHaven/";
    private const int GroundSegments = 72;
    private const int GroundRings = 22;
    private const float GroundTextureMeters = 4.25f;
    private const int GardenBedXSegments = 34;
    private const int GardenBedZSegments = 10;
    private const int GardenBedRowCount = 5;
    private const int VegetablesPerGardenBed = 6;
    private const float GardenBedHalfLength = 16.2f;
    private const float GardenBedRowSpacing = 6f;
    private const float GardenBedBaseLift = 0.025f;
    private const float GardenBedFootprintPadding = 1.6f;
    private const float GardenBedFootprintHalfWidth = 2.15f;
    private const float VegetableEdgePadding = 1.3f;
    private const float BackdropStartRadius = 28f;
    private const float PlayableDecorMargin = 2.8f;
    private const float GoldenAngleRadians = 2.39996323f;

    private static Material _groundMat;
    private static Material _dirtMat;
    private static Material _grassMat;
    private static Material _grassDarkMat;
    private static Material _pebleMat;
    private static Material _twigMat;
    private static Material _mushroomCapMat;
    private static Material _mushroomStemMat;
    private static Material _leafMat;
    private static Material _bedMat;
    private static Material _boardMat;
    private static Material _skyGardenMat;
    private static Material _distantLeafMat;
    private static Material _flowerPetalMat;
    private static Material _flowerCenterMat;
    private static Material _tomatoFruitMat;
    private static Material _carrotMat;

    public static EnvironmentBuilder Active { get; private set; }
    public float ArenaRadius => arenaRadius;

    private void Awake()
    {
        Active = this;
        EnsureMaterials();
        EnsureGardenManager();
        SetupGround();
        SpawnBareSoilPatches();
        SpawnGardenBeds();
        SpawnWoodenBorders();
        SpawnGrassBlades();
        SpawnPebbles();
        SpawnTwigs();
        SpawnMushrooms();
        SpawnLeaves();
        SpawnThickStemPlants();
        SpawnEnlargedGardenBackdrop();
        SetupAtmosphere();
    }

    private void OnDestroy()
    {
        if (Active == this)
        {
            Active = null;
        }
    }

    private void SetupGround()
    {
        GameObject ground = GameObject.Find("Ground");
        if (ground == null)
        {
            ground = new GameObject("Ground");
        }

        ground.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        ground.transform.localScale = Vector3.one;

        MeshFilter filter = GetOrAddComponent<MeshFilter>(ground);
        MeshRenderer renderer = GetOrAddComponent<MeshRenderer>(ground);
        MeshCollider collider = GetOrAddComponent<MeshCollider>(ground);

        Mesh groundMesh = CreateIrregularGroundMesh(arenaRadius + 7f);
        filter.sharedMesh = groundMesh;
        collider.sharedMesh = null;
        collider.sharedMesh = groundMesh;
        renderer.material = _groundMat;

        // Low blended mounds break up the playable silhouette without blocking movement.
        for (int i = 0; i < 4; i++)
        {
            GameObject mound = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            mound.name = "Mound_" + i;
            float angle = (i * 90f + 30f) * Mathf.Deg2Rad;
            float dist = Random.Range(8f, 16f);
            float s = Random.Range(3f, 6f);
            float moundHeight = Random.Range(0.4f, 0.8f);
            float x = Mathf.Cos(angle) * dist;
            float z = Mathf.Sin(angle) * dist;
            mound.transform.position = new Vector3(
                x, SampleGroundHeight(x, z) - moundHeight * 0.42f, z);
            mound.transform.localScale = new Vector3(s, moundHeight, s);
            mound.GetComponent<MeshRenderer>().material = _groundMat;
            mound.isStatic = true;
            StripCollider(mound);
        }
    }

    private void SpawnGardenBeds()
    {
        for (int i = 0; i < GardenBedRowCount; i++)
        {
            float rowZ = GetGardenBedRowZ(i);
            GameObject bed = new GameObject("GardenBed_" + (i + 1));
            bed.name = "GardenBed_" + (i + 1);
            bed.transform.position = new Vector3(0f, 0f, rowZ);
            bed.transform.localScale = Vector3.one;

            MeshFilter filter = bed.AddComponent<MeshFilter>();
            filter.sharedMesh = CreateGardenBedMesh(i, rowZ, arenaRadius + 7f);

            MeshRenderer renderer = bed.AddComponent<MeshRenderer>();
            renderer.material = _bedMat;
            bed.isStatic = true;

            SpawnVegetablesForBed(bed.transform, rowZ, i);
        }
    }

    private void SpawnBareSoilPatches()
    {
        for (int i = 0; i < dirtPatchCount; i++)
        {
            Vector3 position = RandomPos(0f);
            position.y = SampleGroundHeight(position.x, position.z) + 0.025f;

            GameObject patch = new GameObject("BareSoilPatch_" + i);
            patch.transform.position = position;
            patch.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

            MeshFilter filter = patch.AddComponent<MeshFilter>();
            filter.sharedMesh = CreateSoilPatchMesh(Random.Range(1.5f, 3.2f), position, i, arenaRadius + 7f);

            MeshRenderer renderer = patch.AddComponent<MeshRenderer>();
            renderer.material = _dirtMat;
            patch.isStatic = true;
        }
    }

    private void SpawnVegetablesForBed(Transform parent, float z, int rowIndex)
    {
        VegetableType[] mixedTypes =
        {
            VegetableType.Tomato,
            VegetableType.Potato,
            VegetableType.Carrot,
            VegetableType.Tomato,
            VegetableType.Potato,
            VegetableType.Carrot
        };

        for (int i = 0; i < VegetablesPerGardenBed; i++)
        {
            float xT = VegetablesPerGardenBed == 1 ? 0.5f : i / (VegetablesPerGardenBed - 1f);
            float vegetableX = Mathf.Lerp(
                -GardenBedHalfLength + VegetableEdgePadding,
                GardenBedHalfLength - VegetableEdgePadding,
                xT);
            int typeIndex = (i + rowIndex * 2) % mixedTypes.Length;
            GameObject vegetableGo = new GameObject("Vegetable");
            float vegetableZ = z + Random.Range(-0.45f, 0.45f);
            float surfaceY = SampleGardenBedSurfaceHeight(vegetableX, vegetableZ, rowIndex, z);
            vegetableGo.transform.SetPositionAndRotation(
                new Vector3(vegetableX, surfaceY, vegetableZ),
                Quaternion.Euler(0f, Random.Range(-8f, 8f), 0f));
            vegetableGo.transform.SetParent(parent, true);

            Vegetable vegetable = vegetableGo.AddComponent<Vegetable>();
            vegetable.Initialize(mixedTypes[typeIndex]);
        }
    }

    private void SpawnWoodenBorders()
    {
        CreateBoard("NorthBoard", new Vector3(0f, 0.45f, arenaRadius), new Vector3(arenaRadius * 2f, 0.9f, 0.45f));
        CreateBoard("SouthBoard", new Vector3(0f, 0.45f, -arenaRadius), new Vector3(arenaRadius * 2f, 0.9f, 0.45f));
        CreateBoard("EastBoard", new Vector3(arenaRadius, 0.45f, 0f), new Vector3(0.45f, 0.9f, arenaRadius * 2f));
        CreateBoard("WestBoard", new Vector3(-arenaRadius, 0.45f, 0f), new Vector3(0.45f, 0.9f, arenaRadius * 2f));
    }

    private static void CreateBoard(string boardName, Vector3 position, Vector3 scale)
    {
        GameObject board = GameObject.CreatePrimitive(PrimitiveType.Cube);
        board.name = boardName;
        board.transform.position = position;
        board.transform.localScale = scale;
        board.GetComponent<MeshRenderer>().material = _boardMat;
        board.isStatic = true;
    }

    private void SpawnGrassBlades()
    {
        for (int i = 0; i < grassBladeCount; i++)
        {
            Vector3 pos = RandomPos(0f);
            pos.y = SampleGroundHeight(pos.x, pos.z);
            GameObject grass = new GameObject("Grass_" + i);
            grass.transform.position = pos;
            grass.isStatic = true;

            int blades = Random.Range(2, 5);
            for (int j = 0; j < blades; j++)
            {
                GameObject blade = new GameObject("Blade");
                blade.name = "Blade";
                blade.transform.SetParent(grass.transform, false);
                float height = Random.Range(2.8f, 7.5f);
                float width = Random.Range(0.16f, 0.34f);
                blade.transform.localPosition = new Vector3(
                    Random.Range(-0.18f, 0.18f), 0f, Random.Range(-0.18f, 0.18f));
                blade.transform.localRotation = Quaternion.Euler(
                    Random.Range(-10f, 10f), Random.Range(0f, 360f), Random.Range(-18f, 18f));
                blade.AddComponent<MeshFilter>().sharedMesh = CreateGrassBladeMesh(height, width, Random.Range(0.1f, 0.45f));
                blade.AddComponent<MeshRenderer>().material =
                    Random.value > 0.4f ? _grassMat : _grassDarkMat;
                blade.isStatic = true;
            }
        }
    }

    private void SpawnPebbles()
    {
        for (int i = 0; i < pebbleCount; i++)
        {
            Vector3 pos = RandomPos(0f);
            pos.y = SampleGroundHeight(pos.x, pos.z) + 0.03f;
            GameObject pebble = new GameObject("Pebble_" + i);
            pebble.transform.position = pos;
            pebble.isStatic = true;

            int parts = Random.Range(1, 3);
            for (int j = 0; j < parts; j++)
            {
                GameObject part = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                part.name = "Stone";
                part.transform.SetParent(pebble.transform, false);
                part.transform.localPosition = new Vector3(
                    Random.Range(-0.3f, 0.3f), 0f, Random.Range(-0.3f, 0.3f));
                float s = Random.Range(0.8f, 2.5f);
                part.transform.localScale = new Vector3(
                    s * Random.Range(0.9f, 1.3f),
                    s * Random.Range(0.4f, 0.7f),
                    s * Random.Range(0.9f, 1.3f));
                part.transform.localRotation = Quaternion.Euler(
                    Random.Range(-10f, 10f), Random.Range(0f, 360f), Random.Range(-10f, 10f));
                part.GetComponent<MeshRenderer>().material = _pebleMat;
                part.isStatic = true;
                StripCollider(part);
            }
        }
    }

    private void SpawnTwigs()
    {
        for (int i = 0; i < twigCount; i++)
        {
            Vector3 pos = RandomDecorPositionOutsideBeds();
            pos.y = SampleGroundHeight(pos.x, pos.z) + 0.035f;
            GameObject twig = new GameObject("Twig_" + i);
            twig.transform.position = pos;
            twig.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            twig.isStatic = true;

            GameObject main = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            main.name = "Branch";
            main.transform.SetParent(twig.transform, false);
            float length = Random.Range(3f, 7f);
            main.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            main.transform.localScale = new Vector3(0.12f, length * 0.5f, 0.12f);
            main.GetComponent<MeshRenderer>().material = _twigMat;
            main.isStatic = true;
            StripCollider(main);

            if (Random.value > 0.4f)
            {
                GameObject side = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                side.name = "Side";
                side.transform.SetParent(main.transform, false);
                side.transform.localPosition = new Vector3(0.3f, Random.Range(-0.3f, 0.3f), 0f);
                side.transform.localRotation = Quaternion.Euler(0f, 0f, Random.Range(25f, 50f));
                side.transform.localScale = new Vector3(0.5f, 0.4f, 0.5f);
                side.GetComponent<MeshRenderer>().material = _twigMat;
                side.isStatic = true;
                StripCollider(side);
            }
        }
    }

    private void SpawnMushrooms()
    {
        for (int i = 0; i < mushroomCount; i++)
        {
            Vector3 pos = RandomPos(0f);
            pos.y = SampleGroundHeight(pos.x, pos.z);
            GameObject shroom = new GameObject("Mushroom_" + i);
            shroom.transform.position = pos;
            shroom.isStatic = true;

            GameObject stem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            stem.name = "Stem";
            stem.transform.SetParent(shroom.transform, false);
            float stemH = Random.Range(1f, 2f);
            stem.transform.localPosition = new Vector3(0f, stemH * 0.5f, 0f);
            stem.transform.localScale = new Vector3(0.25f, stemH * 0.5f, 0.25f);
            stem.GetComponent<MeshRenderer>().material = _mushroomStemMat;
            stem.isStatic = true;
            StripCollider(stem);

            GameObject cap = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            cap.name = "Cap";
            cap.transform.SetParent(shroom.transform, false);
            float capSize = Random.Range(0.6f, 1.2f);
            cap.transform.localPosition = new Vector3(0f, stemH + capSize * 0.15f, 0f);
            cap.transform.localScale = new Vector3(capSize, capSize * 0.45f, capSize);
            cap.GetComponent<MeshRenderer>().material = _mushroomCapMat;
            cap.isStatic = true;
            StripCollider(cap);
        }
    }

    private void SpawnLeaves()
    {
        for (int i = 0; i < 8; i++)
        {
            Vector3 pos = RandomPos(0.02f);
            pos.y = SampleGroundHeight(pos.x, pos.z);
            GameObject leaf = new GameObject("Leaf_" + i);
            leaf.name = "Leaf_" + i;
            leaf.transform.position = pos;
            float s = Random.Range(1.5f, 3.5f);
            leaf.transform.localScale = new Vector3(s, 1f, s * Random.Range(0.5f, 0.8f));
            leaf.transform.rotation = Quaternion.Euler(
                Random.Range(-3f, 3f), Random.Range(0f, 360f), Random.Range(-3f, 3f));
            leaf.AddComponent<MeshFilter>().sharedMesh = CreateFallenLeafMesh();
            leaf.AddComponent<MeshRenderer>().material = _leafMat;
            leaf.isStatic = true;
        }
    }

    private void SpawnThickStemPlants()
    {
        for (int i = 0; i < thickStemPlantCount; i++)
        {
            Vector3 basePos = EvenDecorPositionOutsideBeds(i, thickStemPlantCount);
            basePos.y = SampleGroundHeight(basePos.x, basePos.z);

            GameObject plant = new GameObject("ThickStemPlant_" + i);
            plant.transform.position = basePos;
            plant.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            plant.isStatic = true;

            float stemHeight = Random.Range(2.7f, 5.2f);
            float stemThickness = Random.Range(0.18f, 0.34f);
            GameObject stem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            stem.name = "ThickStem";
            stem.transform.SetParent(plant.transform, false);
            stem.transform.localPosition = Vector3.up * (stemHeight * 0.5f);
            stem.transform.localRotation = Quaternion.Euler(Random.Range(-5f, 5f), 0f, Random.Range(-6f, 6f));
            stem.transform.localScale = new Vector3(stemThickness, stemHeight * 0.5f, stemThickness);
            stem.GetComponent<MeshRenderer>().material = Random.value > 0.35f ? _grassDarkMat : _twigMat;
            stem.isStatic = true;
            StripCollider(stem);

            int branchCount = Random.Range(3, 6);
            for (int j = 0; j < branchCount; j++)
            {
                float yaw = j * (360f / branchCount) + Random.Range(-18f, 18f);
                float branchHeight = Random.Range(stemHeight * 0.45f, stemHeight * 0.95f);
                float branchLength = Random.Range(0.55f, 1.1f);
                float side = j % 2 == 0 ? 1f : -1f;

                GameObject branch = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                branch.name = "ThickStemBranch";
                branch.transform.SetParent(plant.transform, false);
                branch.transform.localPosition = Vector3.up * branchHeight;
                branch.transform.localRotation = Quaternion.Euler(0f, yaw, side * Random.Range(48f, 66f));
                branch.transform.localScale = new Vector3(stemThickness * 0.36f, branchLength * 0.5f, stemThickness * 0.36f);
                branch.GetComponent<MeshRenderer>().material = _grassDarkMat;
                branch.isStatic = true;
                StripCollider(branch);

                Vector3 leafOffset = Quaternion.Euler(0f, yaw, 0f) * new Vector3(branchLength * 0.55f, 0f, 0f);
                GameObject leaf = new GameObject("BroadStemLeaf");
                leaf.transform.SetParent(plant.transform, false);
                leaf.transform.localPosition = Vector3.up * (branchHeight + Random.Range(0.05f, 0.32f)) + leafOffset;
                leaf.transform.localRotation = Quaternion.Euler(Random.Range(-18f, 18f), yaw, side * Random.Range(22f, 38f));
                leaf.transform.localScale = new Vector3(Random.Range(0.55f, 0.95f), 1f, Random.Range(0.8f, 1.55f));
                leaf.AddComponent<MeshFilter>().sharedMesh = CreateGrassBladeMesh(
                    Random.Range(1.25f, 2.1f),
                    Random.Range(0.18f, 0.32f),
                    Random.Range(0.12f, 0.34f));
                leaf.AddComponent<MeshRenderer>().material = Random.value > 0.25f ? _grassMat : _distantLeafMat;
                leaf.isStatic = true;
            }
        }
    }

    private void SpawnEnlargedGardenBackdrop()
    {
        GameObject backdrop = new GameObject("EnlargedGardenBackdrop");
        backdrop.isStatic = true;

        CreateSkyGardenWall(backdrop.transform);
        SpawnDistantFence(backdrop.transform);
        SpawnGiantStems(backdrop.transform);
        SpawnHangingLeaves(backdrop.transform);
    }

    private void CreateSkyGardenWall(Transform parent)
    {
        for (int i = 0; i < 18; i++)
        {
            float angle = i / 18f * Mathf.PI * 2f;
            float radius = arenaRadius + 19f;
            GameObject panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            panel.name = "SoftGardenSkyPanel_" + i;
            panel.transform.SetParent(parent, false);
            panel.transform.position = new Vector3(Mathf.Cos(angle) * radius, 15f, Mathf.Sin(angle) * radius);
            panel.transform.rotation = Quaternion.LookRotation(new Vector3(-panel.transform.position.x, 0f, -panel.transform.position.z).normalized, Vector3.up);
            panel.transform.localScale = new Vector3(15f, 24f, 0.16f);
            MeshRenderer panelRenderer = panel.GetComponent<MeshRenderer>();
            panelRenderer.material = _skyGardenMat;
            panelRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            panelRenderer.receiveShadows = false;
            panel.isStatic = true;
            Object.Destroy(panel.GetComponent<Collider>());
        }
    }

    private void SpawnDistantFence(Transform parent)
    {
        for (int i = 0; i < 28; i++)
        {
            float angle = i / 28f * Mathf.PI * 2f;
            float radius = arenaRadius + 14f + Mathf.Sin(i * 1.9f) * 1.4f;
            GameObject plank = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plank.name = "HugeFencePlank_" + i;
            plank.transform.SetParent(parent, false);
            plank.transform.position = new Vector3(Mathf.Cos(angle) * radius, 5.2f, Mathf.Sin(angle) * radius);
            plank.transform.rotation = Quaternion.LookRotation(new Vector3(-plank.transform.position.x, 0f, -plank.transform.position.z).normalized, Vector3.up);
            plank.transform.localScale = new Vector3(1.25f, Random.Range(8f, 12.5f), 0.35f);
            plank.GetComponent<MeshRenderer>().material = _boardMat;
            plank.isStatic = true;
            Object.Destroy(plank.GetComponent<Collider>());
        }
    }

    private void SpawnGiantStems(Transform parent)
    {
        const int stemCount = 20;
        for (int i = 0; i < stemCount; i++)
        {
            float angle = i / (float)stemCount * Mathf.PI * 2f + Random.Range(-0.12f, 0.12f);
            float radius = arenaRadius + 3.5f + i % 4 * 1.7f + Random.Range(-0.35f, 0.35f);
            Vector3 basePos = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
            basePos.y = SampleGroundHeight(basePos.x, basePos.z);

            GameObject stem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            stem.name = "GiantGardenStem_" + i;
            stem.transform.SetParent(parent, false);
            float height = Random.Range(8f, 18f);
            stem.transform.position = basePos + Vector3.up * (height * 0.5f);
            stem.transform.rotation = Quaternion.Euler(Random.Range(-8f, 8f), Random.Range(0f, 360f), Random.Range(-10f, 10f));
            float thickness = Random.Range(0.32f, 0.85f);
            stem.transform.localScale = new Vector3(thickness, height * 0.5f, thickness);
            stem.GetComponent<MeshRenderer>().material = Random.value > 0.35f ? _grassDarkMat : _distantLeafMat;
            stem.isStatic = true;
            Object.Destroy(stem.GetComponent<Collider>());

            if (Random.value > 0.35f)
            {
                CreateGiantLeaf(parent, basePos + Vector3.up * Random.Range(height * 0.52f, height * 0.9f), angle);
            }
        }
    }

    private void SpawnDistantVegetableRows(Transform parent)
    {
        for (int row = 0; row < 3; row++)
        {
            float z = -arenaRadius - 10f - row * 5.5f;
            for (int i = 0; i < 9; i++)
            {
                float x = -18f + i * 4.5f + Random.Range(-0.6f, 0.6f);
                CreateDistantVegetable(parent, new Vector3(x, SampleGroundHeight(Mathf.Clamp(x, -arenaRadius, arenaRadius), -arenaRadius + 1f), z), i + row);
            }
        }
    }

    private void SpawnHangingLeaves(Transform parent)
    {
        for (int i = 0; i < 5; i++)
        {
            float angle = i / 10f * Mathf.PI * 2f + Random.Range(-0.2f, 0.2f);
            float radius = arenaRadius + Random.Range(6f, 12f);
            Vector3 position = new Vector3(Mathf.Cos(angle) * radius, Random.Range(8f, 15f), Mathf.Sin(angle) * radius);
            CreateGiantLeaf(parent, position, angle + Mathf.PI);
        }
    }

    private void SpawnGardenFlowers(Transform parent)
    {
        for (int i = 0; i < 8; i++)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float radius = Random.Range(arenaRadius + 5f, arenaRadius + 13f);
            Vector3 basePos = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
            float stemHeight = Random.Range(7f, 13f);

            GameObject stem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            stem.name = "DistantFlowerStem_" + i;
            stem.transform.SetParent(parent, false);
            stem.transform.position = basePos + Vector3.up * (stemHeight * 0.5f);
            stem.transform.localScale = new Vector3(0.18f, stemHeight * 0.5f, 0.18f);
            stem.GetComponent<MeshRenderer>().material = _grassDarkMat;
            Object.Destroy(stem.GetComponent<Collider>());

            GameObject flower = new GameObject("DistantFlower_" + i);
            flower.transform.SetParent(parent, false);
            flower.transform.position = basePos + Vector3.up * stemHeight;
            flower.transform.rotation = Quaternion.LookRotation(new Vector3(-basePos.x, 0f, -basePos.z).normalized, Vector3.up);

            for (int p = 0; p < 6; p++)
            {
                GameObject petal = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                petal.name = "Petal";
                petal.transform.SetParent(flower.transform, false);
                petal.transform.localPosition = Quaternion.Euler(0f, 0f, p * 60f) * Vector3.up * 0.75f;
                petal.transform.localScale = new Vector3(0.6f, 1.1f, 0.12f);
                petal.GetComponent<MeshRenderer>().material = _flowerPetalMat;
                Object.Destroy(petal.GetComponent<Collider>());
            }

            GameObject center = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            center.name = "FlowerCenter";
            center.transform.SetParent(flower.transform, false);
            center.transform.localScale = Vector3.one * 0.55f;
            center.GetComponent<MeshRenderer>().material = _flowerCenterMat;
            Object.Destroy(center.GetComponent<Collider>());
        }
    }

    private static Mesh CreateGrassBladeMesh(float height, float baseWidth, float bend)
    {
        Mesh mesh = new Mesh();
        mesh.name = "Curved Grass Blade";

        float midWidth = baseWidth * 0.64f;
        float tipOffset = bend;
        Vector3[] vertices =
        {
            new Vector3(-baseWidth, 0f, 0f),
            new Vector3(baseWidth, 0f, 0f),
            new Vector3(-midWidth, height * 0.55f, tipOffset * 0.35f),
            new Vector3(midWidth, height * 0.55f, tipOffset * 0.35f),
            new Vector3(0f, height, tipOffset)
        };
        int[] triangles =
        {
            0, 2, 1,
            1, 2, 3,
            2, 4, 3,
            1, 2, 0,
            3, 2, 1,
            3, 4, 2
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = new[]
        {
            new Vector2(0f, 0f),
            new Vector2(1f, 0f),
            new Vector2(0.22f, 0.55f),
            new Vector2(0.78f, 0.55f),
            new Vector2(0.5f, 1f)
        };
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    private static Mesh CreateFallenLeafMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = "Fallen Leaf";

        Vector3[] vertices =
        {
            new Vector3(0f, 0.025f, 1f),
            new Vector3(-0.42f, 0f, 0.45f),
            new Vector3(0.42f, 0f, 0.45f),
            new Vector3(-0.55f, 0.015f, -0.15f),
            new Vector3(0.55f, 0.015f, -0.15f),
            new Vector3(-0.28f, 0f, -0.72f),
            new Vector3(0.28f, 0f, -0.72f),
            new Vector3(0f, 0.02f, -1f)
        };
        int[] triangles =
        {
            0, 1, 2,
            1, 3, 2,
            2, 3, 4,
            3, 5, 4,
            4, 5, 6,
            5, 7, 6,
            2, 1, 0,
            2, 3, 1,
            4, 3, 2,
            4, 5, 3,
            6, 5, 4,
            6, 7, 5
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = new[]
        {
            new Vector2(0.5f, 1f),
            new Vector2(0.1f, 0.72f),
            new Vector2(0.9f, 0.72f),
            new Vector2(0f, 0.42f),
            new Vector2(1f, 0.42f),
            new Vector2(0.2f, 0.13f),
            new Vector2(0.8f, 0.13f),
            new Vector2(0.5f, 0f)
        };
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    private void CreateGiantLeaf(Transform parent, Vector3 position, float yaw)
    {
        GameObject leaf = new GameObject("HugeOverheadLeaf");
        leaf.transform.SetParent(parent, false);
        leaf.transform.position = position;
        leaf.transform.rotation = Quaternion.Euler(Random.Range(15f, 55f), yaw * Mathf.Rad2Deg, Random.Range(-18f, 18f));
        leaf.transform.localScale = new Vector3(Random.Range(3f, 5.5f), 1f, Random.Range(4f, 7f));
        leaf.AddComponent<MeshFilter>().sharedMesh = CreateFallenLeafMesh();
        leaf.AddComponent<MeshRenderer>().material = _distantLeafMat;
        leaf.isStatic = true;
    }

    private void CreateDistantVegetable(Transform parent, Vector3 position, int seed)
    {
        GameObject plant = new GameObject("HugeBackgroundVegetable_" + seed);
        plant.transform.SetParent(parent, false);
        plant.transform.position = position;
        plant.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

        if (seed % 3 == 0)
        {
            GameObject fruit = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            fruit.name = "HugeTomato";
            fruit.transform.SetParent(plant.transform, false);
            fruit.transform.localPosition = new Vector3(0f, 3.15f, 0f);
            fruit.transform.localScale = new Vector3(3.25f, 2.75f, 3.25f);
            fruit.GetComponent<MeshRenderer>().material = _tomatoFruitMat;
            Object.Destroy(fruit.GetComponent<Collider>());
        }
        else if (seed % 3 == 1)
        {
            GameObject carrot = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            carrot.name = "HugeCarrotShoulder";
            carrot.transform.SetParent(plant.transform, false);
            carrot.transform.localPosition = new Vector3(0f, 1.65f, 0f);
            carrot.transform.localRotation = Quaternion.Euler(180f, 0f, 0f);
            carrot.transform.localScale = new Vector3(1.75f, 3.1f, 1.75f);
            carrot.GetComponent<MeshRenderer>().material = _carrotMat;
            Object.Destroy(carrot.GetComponent<Collider>());
        }
        else
        {
            GameObject potato = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            potato.name = "HugePotatoMound";
            potato.transform.SetParent(plant.transform, false);
            potato.transform.localPosition = new Vector3(0f, 1.2f, 0f);
            potato.transform.localScale = new Vector3(3.8f, 2.05f, 2.75f);
            potato.GetComponent<MeshRenderer>().material = _bedMat;
            Object.Destroy(potato.GetComponent<Collider>());
        }

        for (int i = 0; i < 7; i++)
        {
            GameObject top = new GameObject("HugeVegetableLeaf");
            top.transform.SetParent(plant.transform, false);
            top.transform.localPosition = Vector3.up * Random.Range(3.1f, 4.4f);
            top.transform.localRotation = Quaternion.Euler(Random.Range(-30f, 30f), i * 51.4f, Random.Range(-42f, 42f));
            top.transform.localScale = new Vector3(Random.Range(1.05f, 1.8f), 1f, Random.Range(2.1f, 3.55f));
            top.AddComponent<MeshFilter>().sharedMesh = CreateGrassBladeMesh(Random.Range(3.6f, 6.4f), Random.Range(0.35f, 0.6f), Random.Range(0.32f, 0.82f));
            top.AddComponent<MeshRenderer>().material = _distantLeafMat;
        }
    }

    private void SetupAtmosphere()
    {
        NormalizeSceneLighting();
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = 18f;
        RenderSettings.fogEndDistance = 62f;
        RenderSettings.fogColor = new Color(0.31f, 0.39f, 0.27f);
        RenderSettings.ambientLight = new Color(0.28f, 0.33f, 0.24f);
        GameFeedback.EnsureInScene();
    }

    private static void NormalizeSceneLighting()
    {
        Light[] lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
        Light mainDirectional = PickMainDirectionalLight(lights);
        foreach (Light light in lights)
        {
            if (light == null)
            {
                continue;
            }

            if (light == mainDirectional)
            {
                continue;
            }

            StripExtraLight(light);
        }

        if (mainDirectional == null)
        {
            GameObject lightGo = new GameObject("Directional Light");
            mainDirectional = lightGo.AddComponent<Light>();
            mainDirectional.type = LightType.Directional;
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        mainDirectional.enabled = true;
        mainDirectional.color = new Color(0.92f, 0.86f, 0.72f);
        mainDirectional.intensity = 0.36f;
        mainDirectional.shadows = LightShadows.Soft;
        mainDirectional.shadowStrength = 0.58f;
        RenderSettings.sun = mainDirectional;
    }

    private static Light PickMainDirectionalLight(Light[] lights)
    {
        Light best = null;
        float bestScore = float.MaxValue;

        foreach (Light light in lights)
        {
            if (light == null || light.type != LightType.Directional)
            {
                continue;
            }

            Vector3 position = light.transform.position;
            float flatDistance = new Vector2(position.x, position.z).magnitude;
            float score = flatDistance;
            if (light.name == "Directional Light")
            {
                score -= 100f;
            }

            if (flatDistance >= BackdropStartRadius)
            {
                score += 1000f;
            }

            if (score < bestScore)
            {
                best = light;
                bestScore = score;
            }
        }

        return best;
    }

    private static void StripExtraLight(Light light)
    {
        if (light == null)
        {
            return;
        }

        light.enabled = false;
        if (IsBackdropLight(light) || light.type != LightType.Directional)
        {
            Object.Destroy(light);
        }
    }

    private static bool IsBackdropLight(Light light)
    {
        Vector3 position = light.transform.position;
        return new Vector2(position.x, position.z).magnitude >= BackdropStartRadius;
    }

    private Vector3 RandomPos(float y)
    {
        float angle = Random.Range(0f, Mathf.PI * 2f);
        float minRadius = Mathf.Min(3f, arenaRadius * 0.25f);
        float maxRadius = Mathf.Max(minRadius, arenaRadius - PlayableDecorMargin);
        float dist = Mathf.Sqrt(Random.Range(minRadius * minRadius, maxRadius * maxRadius));
        return new Vector3(Mathf.Cos(angle) * dist, y, Mathf.Sin(angle) * dist);
    }

    private Vector3 RandomDecorPositionOutsideBeds()
    {
        for (int i = 0; i < 24; i++)
        {
            Vector3 position = RandomPos(0f);
            if (!IsInsideGardenBedFootprint(position))
            {
                return position;
            }
        }

        return RandomPos(0f);
    }

    private Vector3 EvenDecorPositionOutsideBeds(int index, int total)
    {
        int safeTotal = Mathf.Max(1, total);
        float maxRadius = Mathf.Max(5.5f, arenaRadius - PlayableDecorMargin);
        float minRadius = Mathf.Min(5.5f, maxRadius);

        for (int attempt = 0; attempt < 24; attempt++)
        {
            float angle = (index + 0.5f) / safeTotal * Mathf.PI * 2f
                + attempt * GoldenAngleRadians
                + Random.Range(-0.1f, 0.1f);
            float ringT = ((index + attempt) % 3 + 0.5f) / 3f;
            float radius = Mathf.Lerp(minRadius, maxRadius, ringT) + Random.Range(-0.65f, 0.65f);
            Vector3 position = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);

            if (!IsInsideGardenBedFootprint(position))
            {
                return position;
            }
        }

        return RandomDecorPositionOutsideBeds();
    }

    private static bool IsInsideGardenBedFootprint(Vector3 position)
    {
        if (Mathf.Abs(position.x) > GardenBedHalfLength + GardenBedFootprintPadding)
        {
            return false;
        }

        for (int i = 0; i < GardenBedRowCount; i++)
        {
            if (Mathf.Abs(position.z - GetGardenBedRowZ(i)) < GardenBedFootprintHalfWidth)
            {
                return true;
            }
        }

        return false;
    }

    private static float GetGardenBedRowZ(int rowIndex)
    {
        return (rowIndex - (GardenBedRowCount - 1) * 0.5f) * GardenBedRowSpacing;
    }

    private static void EnsureMaterials()
    {
        if (_groundMat != null) return;

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");

        _groundMat = CreateTexturedMaterial(
            shader,
            "leafy_grass_diff_1k",
            "leafy_grass_nor_gl_1k",
            new Color(0.78f, 0.82f, 0.68f),
            Vector2.one,
            0.55f,
            0.08f);

        _dirtMat = CreateTexturedMaterial(
            shader,
            "brown_mud_diff_1k",
            "brown_mud_nor_gl_1k",
            new Color(0.75f, 0.66f, 0.55f),
            Vector2.one,
            0.7f,
            0.06f);

        _grassMat = new Material(shader);
        SetMaterialColor(_grassMat, new Color(0.2f, 0.55f, 0.12f)); // bright green
        SetMaterialSmoothness(_grassMat, 0.08f);

        _grassDarkMat = new Material(shader);
        SetMaterialColor(_grassDarkMat, new Color(0.12f, 0.35f, 0.08f)); // dark green
        SetMaterialSmoothness(_grassDarkMat, 0.06f);

        _pebleMat = new Material(shader);
        _pebleMat.color = new Color(0.42f, 0.4f, 0.38f); // grey stone

        _twigMat = CreateTexturedMaterial(
            shader,
            "wood_planks_diff_1k",
            "wood_planks_nor_gl_1k",
            new Color(0.55f, 0.36f, 0.2f),
            new Vector2(1.5f, 1.5f),
            0.5f,
            0.12f);

        _mushroomCapMat = new Material(shader);
        _mushroomCapMat.color = new Color(0.7f, 0.15f, 0.1f); // red cap

        _mushroomStemMat = new Material(shader);
        _mushroomStemMat.color = new Color(0.85f, 0.82f, 0.7f); // pale stem

        _leafMat = new Material(shader);
        _leafMat.color = new Color(0.35f, 0.5f, 0.1f); // yellow-green fallen leaf

        _bedMat = CreateTexturedMaterial(
            shader,
            "brown_mud_diff_1k",
            "brown_mud_nor_gl_1k",
            new Color(0.55f, 0.39f, 0.26f),
            new Vector2(1.2f, 1.2f),
            0.8f,
            0.04f);

        _boardMat = CreateTexturedMaterial(
            shader,
            "wood_planks_diff_1k",
            "wood_planks_nor_gl_1k",
            new Color(0.8f, 0.58f, 0.36f),
            new Vector2(4f, 1.4f),
            0.7f,
            0.16f);

        _skyGardenMat = new Material(shader);
        SetMaterialColor(_skyGardenMat, new Color(0.12f, 0.2f, 0.12f, 1f));
        SetMaterialSmoothness(_skyGardenMat, 0f);
        DisableMaterialHighlights(_skyGardenMat);

        _distantLeafMat = new Material(shader);
        SetMaterialColor(_distantLeafMat, new Color(0.11f, 0.34f, 0.1f));
        SetMaterialSmoothness(_distantLeafMat, 0.03f);
        DisableMaterialHighlights(_distantLeafMat);

        _flowerPetalMat = new Material(shader);
        SetMaterialColor(_flowerPetalMat, new Color(0.95f, 0.72f, 0.2f));
        SetMaterialSmoothness(_flowerPetalMat, 0.18f);

        _flowerCenterMat = new Material(shader);
        SetMaterialColor(_flowerCenterMat, new Color(0.35f, 0.19f, 0.05f));
        SetMaterialSmoothness(_flowerCenterMat, 0.12f);

        _tomatoFruitMat = new Material(shader);
        SetMaterialColor(_tomatoFruitMat, new Color(0.74f, 0.05f, 0.035f));
        SetMaterialSmoothness(_tomatoFruitMat, 0.22f);

        _carrotMat = new Material(shader);
        SetMaterialColor(_carrotMat, new Color(0.94f, 0.34f, 0.04f));
        SetMaterialSmoothness(_carrotMat, 0.16f);
    }

    private static Mesh CreateIrregularGroundMesh(float radius)
    {
        Mesh mesh = new Mesh();
        mesh.name = "Irregular Garden Ground";

        int vertexCount = 1 + GroundSegments * GroundRings;
        Vector3[] vertices = new Vector3[vertexCount];
        Vector2[] uvs = new Vector2[vertexCount];
        int[] triangles = new int[GroundSegments * 3 + (GroundRings - 1) * GroundSegments * 6];

        vertices[0] = new Vector3(0f, GetGroundHeight(0f, 0f, 0f), 0f);
        uvs[0] = Vector2.zero;

        for (int ring = 1; ring <= GroundRings; ring++)
        {
            float ringT = ring / (float)GroundRings;
            for (int segment = 0; segment < GroundSegments; segment++)
            {
                float angle = segment / (float)GroundSegments * Mathf.PI * 2f;
                float edgeNoise = Mathf.PerlinNoise(Mathf.Cos(angle) * 1.7f + 12.3f, Mathf.Sin(angle) * 1.7f + 4.8f);
                float fineNoise = Mathf.PerlinNoise(Mathf.Cos(angle * 3.1f) * 0.9f + 1.2f, Mathf.Sin(angle * 2.7f) * 0.9f + 8.6f);
                float edgeRadius = radius * (0.91f + edgeNoise * 0.16f + fineNoise * 0.045f);
                float distance = edgeRadius * Mathf.Pow(ringT, 1.02f);
                float x = Mathf.Cos(angle) * distance;
                float z = Mathf.Sin(angle) * distance;
                int index = GroundIndex(ring, segment);

                vertices[index] = new Vector3(x, GetGroundHeight(x, z, ringT), z);
                uvs[index] = new Vector2(x / GroundTextureMeters, z / GroundTextureMeters);
            }
        }

        int triangleIndex = 0;
        for (int segment = 0; segment < GroundSegments; segment++)
        {
            int nextSegment = (segment + 1) % GroundSegments;
            triangles[triangleIndex++] = 0;
            triangles[triangleIndex++] = GroundIndex(1, nextSegment);
            triangles[triangleIndex++] = GroundIndex(1, segment);
        }

        for (int ring = 1; ring < GroundRings; ring++)
        {
            for (int segment = 0; segment < GroundSegments; segment++)
            {
                int nextSegment = (segment + 1) % GroundSegments;
                int inner = GroundIndex(ring, segment);
                int innerNext = GroundIndex(ring, nextSegment);
                int outer = GroundIndex(ring + 1, segment);
                int outerNext = GroundIndex(ring + 1, nextSegment);

                triangles[triangleIndex++] = inner;
                triangles[triangleIndex++] = innerNext;
                triangles[triangleIndex++] = outer;

                triangles[triangleIndex++] = outer;
                triangles[triangleIndex++] = innerNext;
                triangles[triangleIndex++] = outerNext;
            }
        }

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    private static Mesh CreateGardenBedMesh(int rowIndex, float rowZ, float groundRadius)
    {
        Mesh mesh = new Mesh();
        mesh.name = "Raised Soil Bed";

        Vector3[] vertices = new Vector3[(GardenBedXSegments + 1) * (GardenBedZSegments + 1)];
        Vector2[] uvs = new Vector2[vertices.Length];
        int[] triangles = new int[GardenBedXSegments * GardenBedZSegments * 6];

        for (int xIndex = 0; xIndex <= GardenBedXSegments; xIndex++)
        {
            float xT = xIndex / (float)GardenBedXSegments;
            float x = Mathf.Lerp(-GardenBedHalfLength, GardenBedHalfLength, xT);
            float halfWidth = GetGardenBedHalfWidth(xT, rowIndex);

            for (int zIndex = 0; zIndex <= GardenBedZSegments; zIndex++)
            {
                float zT = zIndex / (float)GardenBedZSegments;
                float side = zT * 2f - 1f;
                float z = side * halfWidth;
                float y = GetGardenBedSurfaceHeight(x, rowZ + z, rowIndex, rowZ, groundRadius);

                int index = xIndex * (GardenBedZSegments + 1) + zIndex;
                vertices[index] = new Vector3(x, y, z);
                uvs[index] = new Vector2(x / 2.8f, z / 2.8f);
            }
        }

        int triangleIndex = 0;
        for (int xIndex = 0; xIndex < GardenBedXSegments; xIndex++)
        {
            for (int zIndex = 0; zIndex < GardenBedZSegments; zIndex++)
            {
                int current = xIndex * (GardenBedZSegments + 1) + zIndex;
                int nextX = current + GardenBedZSegments + 1;

                triangles[triangleIndex++] = current;
                triangles[triangleIndex++] = current + 1;
                triangles[triangleIndex++] = nextX;

                triangles[triangleIndex++] = nextX;
                triangles[triangleIndex++] = current + 1;
                triangles[triangleIndex++] = nextX + 1;
            }
        }

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    private static Mesh CreateSoilPatchMesh(float radius, Vector3 worldCenter, int seed, float groundRadius)
    {
        const int segments = 28;

        Mesh mesh = new Mesh();
        mesh.name = "Irregular Soil Patch";

        Vector3[] vertices = new Vector3[segments + 1];
        Vector2[] uvs = new Vector2[segments + 1];
        int[] triangles = new int[segments * 3];

        float centerRingT = Mathf.Clamp01(new Vector2(worldCenter.x, worldCenter.z).magnitude / groundRadius);
        float centerHeight = GetGroundHeight(worldCenter.x, worldCenter.z, centerRingT);
        vertices[0] = new Vector3(0f, centerHeight - worldCenter.y + 0.018f, 0f);
        uvs[0] = Vector2.zero;

        for (int segment = 0; segment < segments; segment++)
        {
            float angle = segment / (float)segments * Mathf.PI * 2f;
            float shapeNoise = Mathf.PerlinNoise(
                seed * 2.9f + Mathf.Cos(angle) * 1.4f + 3.1f,
                seed * 4.1f + Mathf.Sin(angle) * 1.4f + 5.7f);
            float patchRadius = radius * (0.68f + shapeNoise * 0.46f);
            float x = Mathf.Cos(angle) * patchRadius;
            float z = Mathf.Sin(angle) * patchRadius;
            float worldX = worldCenter.x + x;
            float worldZ = worldCenter.z + z;
            float ringT = Mathf.Clamp01(new Vector2(worldX, worldZ).magnitude / groundRadius);
            float y = GetGroundHeight(worldX, worldZ, ringT) - worldCenter.y + 0.018f;

            vertices[segment + 1] = new Vector3(x, y, z);
            uvs[segment + 1] = new Vector2(x / 2.5f, z / 2.5f);
        }

        int triangleIndex = 0;
        for (int segment = 0; segment < segments; segment++)
        {
            int nextSegment = segment == segments - 1 ? 1 : segment + 2;
            triangles[triangleIndex++] = 0;
            triangles[triangleIndex++] = nextSegment;
            triangles[triangleIndex++] = segment + 1;
        }

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    private static float GetGroundHeight(float x, float z, float ringT)
    {
        float large = Mathf.PerlinNoise(x * 0.08f + 15.4f, z * 0.08f + 9.2f) - 0.5f;
        float detail = Mathf.PerlinNoise(x * 0.27f + 4.8f, z * 0.27f + 21.7f) - 0.5f;
        float centerFade = Mathf.SmoothStep(0.08f, 0.75f, ringT);
        float softEdgeDrop = Mathf.SmoothStep(0.82f, 1f, ringT) * 0.18f;

        return (large * 0.28f + detail * 0.065f) * centerFade - softEdgeDrop;
    }

    private float SampleGroundHeight(float x, float z)
    {
        return SampleGeneratedGroundHeight(x, z, arenaRadius + 7f);
    }

    private float SampleGardenBedSurfaceHeight(float worldX, float worldZ, int rowIndex, float rowZ)
    {
        return GetGardenBedSurfaceHeight(worldX, worldZ, rowIndex, rowZ, arenaRadius + 7f);
    }

    public float SampleGroundSurface(float x, float z)
    {
        return SampleGroundHeight(x, z);
    }

    public static float SampleGeneratedGroundHeight(float x, float z, float groundRadius)
    {
        float ringT = Mathf.Clamp01(new Vector2(x, z).magnitude / groundRadius);
        return GetGroundHeight(x, z, ringT);
    }

    private static float GetGardenBedSurfaceHeight(float worldX, float worldZ, int rowIndex, float rowZ, float groundRadius)
    {
        float xT = Mathf.InverseLerp(-GardenBedHalfLength, GardenBedHalfLength, worldX);
        float endFade = GetGardenBedEndFade(xT);
        float halfWidth = GetGardenBedHalfWidth(xT, rowIndex);
        float side = halfWidth > 0.001f
            ? Mathf.Clamp((worldZ - rowZ) / halfWidth, -1f, 1f)
            : 0f;
        float zT = side * 0.5f + 0.5f;
        float sideFade = 1f - Mathf.Abs(side);
        float soilCrown = Mathf.Pow(Mathf.Clamp01(sideFade), 0.55f) * 0.22f;
        float smallNoise = Mathf.PerlinNoise(rowIndex * 6.1f + xT * 12f, zT * 6.5f + 3.3f) * 0.04f;
        float furrow = Mathf.Sin((xT * 7f + rowIndex * 0.25f) * Mathf.PI * 2f) * 0.012f * sideFade;
        float groundY = SampleGeneratedGroundHeight(worldX, worldZ, groundRadius);

        return groundY + (GardenBedBaseLift + soilCrown + smallNoise + furrow) * endFade;
    }

    private static float GetGardenBedHalfWidth(float xT, int rowIndex)
    {
        float endFade = GetGardenBedEndFade(xT);
        float edgeNoise = Mathf.PerlinNoise(rowIndex * 13.7f + xT * 5.2f, 2.5f);
        return Mathf.Lerp(0.7f, 1.38f, endFade) + (edgeNoise - 0.5f) * 0.22f;
    }

    private static float GetGardenBedEndFade(float xT)
    {
        return Mathf.SmoothStep(0f, 1f, Mathf.Min(xT, 1f - xT) * 2f);
    }

    private static int GroundIndex(int ring, int segment)
    {
        if (segment < 0)
        {
            segment += GroundSegments;
        }
        else if (segment >= GroundSegments)
        {
            segment -= GroundSegments;
        }

        return 1 + (ring - 1) * GroundSegments + segment;
    }

    private static Material CreateTexturedMaterial(
        Shader shader,
        string albedoTextureName,
        string normalTextureName,
        Color tint,
        Vector2 tiling,
        float bumpScale,
        float smoothness)
    {
        Material material = new Material(shader);
        SetMaterialColor(material, tint);
        SetMaterialSmoothness(material, smoothness);

        Texture2D albedo = Resources.Load<Texture2D>(PolyHavenTexturePath + albedoTextureName);
        if (albedo != null)
        {
            ApplyTexture(material, "_BaseMap", albedo, tiling);
            ApplyTexture(material, "_MainTex", albedo, tiling);
        }

        Texture2D normal = Resources.Load<Texture2D>(PolyHavenTexturePath + normalTextureName);
        if (normal != null && material.HasProperty("_BumpMap"))
        {
            material.SetTexture("_BumpMap", normal);
            material.SetTextureScale("_BumpMap", tiling);
            if (material.HasProperty("_BumpScale"))
            {
                material.SetFloat("_BumpScale", bumpScale);
            }
            material.EnableKeyword("_NORMALMAP");
        }

        return material;
    }

    private static void ApplyTexture(Material material, string propertyName, Texture texture, Vector2 tiling)
    {
        if (!material.HasProperty(propertyName))
        {
            return;
        }

        material.SetTexture(propertyName, texture);
        material.SetTextureScale(propertyName, tiling);
    }

    private static void SetMaterialColor(Material material, Color color)
    {
        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }
        else if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", color);
        }
    }

    private static void SetMaterialSmoothness(Material material, float smoothness)
    {
        if (material.HasProperty("_Smoothness"))
        {
            material.SetFloat("_Smoothness", smoothness);
        }
    }

    private static void DisableMaterialHighlights(Material material)
    {
        if (material.HasProperty("_Metallic"))
        {
            material.SetFloat("_Metallic", 0f);
        }

        if (material.HasProperty("_SpecularHighlights"))
        {
            material.SetFloat("_SpecularHighlights", 0f);
        }

        if (material.HasProperty("_EnvironmentReflections"))
        {
            material.SetFloat("_EnvironmentReflections", 0f);
        }
    }

    private static T GetOrAddComponent<T>(GameObject gameObject) where T : Component
    {
        T component = gameObject.GetComponent<T>();
        if (component == null)
        {
            component = gameObject.AddComponent<T>();
        }

        return component;
    }

    private static void StripCollider(GameObject gameObject)
    {
        Collider collider = gameObject.GetComponent<Collider>();
        if (collider != null)
        {
            Object.Destroy(collider);
        }
    }

    private static void EnsureGardenManager()
    {
        if (GardenManager.Instance != null || FindFirstObjectByType<GardenManager>() != null)
        {
            return;
        }

        GameObject manager = new GameObject("GardenManager");
        manager.AddComponent<GardenManager>();
    }
}

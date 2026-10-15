#if UNITY_6000_0_OR_NEWER
using PMat = UnityEngine.PhysicsMaterial;
using PCombine = UnityEngine.PhysicsMaterialCombine;
#else
using PMat = UnityEngine.PhysicMaterial;
using PCombine = UnityEngine.PhysicMaterialCombine;
#endif
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Menu: Tools > Build Park Physics Scene
public static class ParkSceneBuilder
{
    [MenuItem("Tools/Build Park Physics Scene")]
    public static void Build()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        EnsureFolder("Assets/Scenes");
        EnsureFolder("Assets/Materials");
        EnsureFolder("Assets/PhysicsMaterials");

        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // ---------- Materials ----------
        var grass  = MakeMat("Grass",  new Color(0.25f, 0.60f, 0.25f));
        var path   = MakeMat("Path",   new Color(0.80f, 0.72f, 0.55f));
        var wood   = MakeMat("Wood",   new Color(0.55f, 0.35f, 0.18f));
        var crate  = MakeMat("Crate",  new Color(0.85f, 0.60f, 0.25f));
        var ramp   = MakeMat("Ramp",   new Color(0.45f, 0.45f, 0.50f));
        var leaves = MakeMat("Leaves", new Color(0.10f, 0.45f, 0.15f));
        var ballRed  = MakeMat("BallRed",  new Color(0.90f, 0.15f, 0.15f));
        var ballBlue = MakeMat("BallBlue", new Color(0.15f, 0.35f, 0.90f));
        var barrel = MakeMat("Barrel", new Color(0.70f, 0.10f, 0.10f));

        // ---------- Physics material (bouncy) ----------
        string pmPath = "Assets/PhysicsMaterials/BouncyBall.asset";
        AssetDatabase.DeleteAsset(pmPath);
        var bouncy = new PMat("BouncyBall")
        {
            bounciness = 0.8f,
            dynamicFriction = 0.4f,
            staticFriction = 0.4f,
            bounceCombine = PCombine.Maximum,
            frictionCombine = PCombine.Average
        };
        AssetDatabase.CreateAsset(bouncy, pmPath);

        // ---------- Hierarchy groups ----------
        var env     = new GameObject("Park_Environment").transform;
        var decor   = new GameObject("Park_Decor").transform;
        var physics = new GameObject("Physics_Objects").transform;

        // ---------- Ground ----------
        var ground = Prim(PrimitiveType.Plane, "Ground", env, Vector3.zero, new Vector3(3, 1, 3), Vector3.zero, grass);
        // Plane already has a MeshCollider -> objects cannot fall through.

        // Walking path (visual only)
        var pathGo = Prim(PrimitiveType.Cube, "Path", env, new Vector3(0, 0.02f, 0), new Vector3(3, 0.04f, 28), Vector3.zero, path);
        Object.DestroyImmediate(pathGo.GetComponent<Collider>());

        // ---------- Bench (cubes, static) ----------
        var bench = new GameObject("Bench").transform; bench.SetParent(decor, false);
        Prim(PrimitiveType.Cube, "Bench_Seat", bench, new Vector3(7, 0.6f, -2), new Vector3(3, 0.2f, 0.9f), Vector3.zero, wood);
        Prim(PrimitiveType.Cube, "Bench_Back", bench, new Vector3(7, 1.1f, -2.4f), new Vector3(3, 0.8f, 0.15f), Vector3.zero, wood);
        Prim(PrimitiveType.Cube, "Bench_LegL", bench, new Vector3(5.7f, 0.25f, -2), new Vector3(0.2f, 0.5f, 0.8f), Vector3.zero, wood);
        Prim(PrimitiveType.Cube, "Bench_LegR", bench, new Vector3(8.3f, 0.25f, -2), new Vector3(0.2f, 0.5f, 0.8f), Vector3.zero, wood);

        // ---------- Trees (cylinder trunk + sphere leaves, static) ----------
        MakeTree(decor, "Tree_1", new Vector3(10, 0, 6), trunkMat: wood, leafMat: leaves);
        MakeTree(decor, "Tree_2", new Vector3(-11, 0, -5), trunkMat: wood, leafMat: leaves);

        // ---------- Ramp (static cube, slopes toward +Z) ----------
        Prim(PrimitiveType.Cube, "Ramp", env, new Vector3(-3, 1.4f, 0), new Vector3(4, 0.3f, 6), new Vector3(20, 0, 0), ramp);

        // ---------- Physics objects ----------
        // Crate pyramid at the bottom of the ramp
        float s = 1.1f; float z = 6f;
        Vector3[] crates =
        {
            new Vector3(-3 - s, 0.5f, z), new Vector3(-3, 0.5f, z), new Vector3(-3 + s, 0.5f, z),
            new Vector3(-3 - s/2, 1.55f, z), new Vector3(-3 + s/2, 1.55f, z),
            new Vector3(-3, 2.6f, z)
        };
        for (int i = 0; i < crates.Length; i++)
        {
            var c = Prim(PrimitiveType.Cube, "Crate_" + (i + 1), physics, crates[i], Vector3.one, Vector3.zero, crate);
            AddRb(c, 1f);
        }

        // Barrel (cylinder) that can be knocked over
        var barrelGo = Prim(PrimitiveType.Cylinder, "Barrel", physics, new Vector3(0.2f, 0.6f, 6f), new Vector3(1f, 0.6f, 1f), Vector3.zero, barrel);
        Object.DestroyImmediate(barrelGo.GetComponent<CapsuleCollider>());
        var mc = barrelGo.AddComponent<MeshCollider>(); mc.convex = true;
        AddRb(barrelGo, 2f);

        // Ball 1: drops onto the ramp, rolls into the crates
        var ball1 = Prim(PrimitiveType.Sphere, "Ball_Ramp", physics, new Vector3(-3, 8, -1.5f), Vector3.one, Vector3.zero, ballRed);
        ball1.GetComponent<SphereCollider>().sharedMaterial = bouncy;
        AddRb(ball1, 1f, continuous: true);

        // Ball 2: drops straight onto the ground and bounces
        var ball2 = Prim(PrimitiveType.Sphere, "Ball_Bounce", physics, new Vector3(3, 9, -3), Vector3.one, Vector3.zero, ballBlue);
        ball2.GetComponent<SphereCollider>().sharedMaterial = bouncy;
        AddRb(ball2, 1f, continuous: true);

        physics.gameObject.AddComponent<BallResetter>(); // press R in Play mode to replay

        // ---------- Camera ----------
        var cam = Camera.main;
        if (cam != null)
        {
            cam.transform.position = new Vector3(0, 10, -19);
            cam.transform.LookAt(new Vector3(0, 1.5f, 2));
            cam.name = "Main Camera";
        }

        // ---------- Save ----------
        string scenePath = "Assets/Scenes/ParkPhysicsEnvironment.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AssetDatabase.SaveAssets();
        Debug.Log("Park scene built and saved to " + scenePath + ". Press Play!");
    }

    // ---------- helpers ----------
    static GameObject Prim(PrimitiveType type, string name, Transform parent, Vector3 pos, Vector3 scale, Vector3 euler, Material mat)
    {
        var go = GameObject.CreatePrimitive(type);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.position = pos;
        go.transform.eulerAngles = euler;
        go.transform.localScale = scale;
        go.GetComponent<Renderer>().sharedMaterial = mat;
        return go;
    }

    static void AddRb(GameObject go, float mass, bool continuous = false)
    {
        var rb = go.AddComponent<Rigidbody>();
        rb.mass = mass;
        rb.useGravity = true;
        if (continuous) rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    static void MakeTree(Transform parent, string name, Vector3 pos, Material trunkMat, Material leafMat)
    {
        var tree = new GameObject(name).transform; tree.SetParent(parent, false);
        Prim(PrimitiveType.Cylinder, name + "_Trunk", tree, pos + new Vector3(0, 1.5f, 0), new Vector3(0.7f, 1.5f, 0.7f), Vector3.zero, trunkMat);
        Prim(PrimitiveType.Sphere,   name + "_Leaves", tree, pos + new Vector3(0, 4f, 0),   new Vector3(3.2f, 3.2f, 3.2f), Vector3.zero, leafMat);
    }

    static Material MakeMat(string name, Color color)
    {
        var shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        var m = new Material(shader) { name = name };
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", color);
        if (m.HasProperty("_Color")) m.SetColor("_Color", color);
        string p = "Assets/Materials/" + name + ".mat";
        AssetDatabase.DeleteAsset(p);
        AssetDatabase.CreateAsset(m, p);
        return m;
    }

    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = System.IO.Path.GetDirectoryName(path).Replace("\\", "/");
        AssetDatabase.CreateFolder(parent, System.IO.Path.GetFileName(path));
    }
}

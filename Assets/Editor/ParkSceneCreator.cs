using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Unity Editor script that creates the complete Park Physics Environment.
/// 
/// HOW TO USE:
///   1. Open this project in Unity Editor
///   2. Go to menu: Park Physics > Create Park Environment
///   3. The scene will be auto-generated and saved
///   4. Press Play (▶) to see the bouncing sphere in action
///
/// REQUIREMENTS SATISFIED:
///   - 1 Plane (ParkGround)
///   - 3+ Cubes (BenchCube_1, BenchCube_2, ParkBlock_3)
///   - 2 Spheres (BouncingSphere, DecorativeSphere)
///   - 1 Cylinder (TreeTrunk)
///   - 2+ Rigidbody components (BouncingSphere, DecorativeSphere)
///   - Proper Colliders on all objects
///   - BouncyMaterial physics material (Bounciness = 0.8)
///   - Camera positioned for clear park view
///   - Scene saved as ParkPhysicsEnvironment
/// </summary>
public class ParkSceneCreator
{
    // =========================================================================
    // MENU ITEM: Create the Park Environment
    // =========================================================================
    [MenuItem("Park Physics/Create Park Environment")]
    public static void CreateParkEnvironment()
    {
        // Safety check: cannot create scenes during Play mode
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog(
                "Stop Play Mode First",
                "Please stop Play mode (press ▶) before creating the park scene.",
                "OK"
            );
            return;
        }

        // ------------------------------------------------------------------
        // STEP 1: Create a fresh scene with default Camera and Light
        // ------------------------------------------------------------------
        var scene = EditorSceneManager.NewScene(
            NewSceneSetup.DefaultGameObjects,
            NewSceneMode.Single
        );

        // ------------------------------------------------------------------
        // STEP 2: Ensure asset folders exist
        // ------------------------------------------------------------------
        EnsureFolder("Assets", "Materials");
        EnsureFolder("Assets", "PhysicsMaterials");
        EnsureFolder("Assets", "Scenes");

        // ------------------------------------------------------------------
        // STEP 3: Create all Materials
        // ------------------------------------------------------------------
        // Required materials (as specified in the assignment)
        Material grassGreen     = CreateMaterial("GrassGreen",     new Color(0.18f, 0.58f, 0.14f), 0.10f);
        Material woodBrown      = CreateMaterial("WoodBrown",      new Color(0.55f, 0.35f, 0.15f), 0.25f);
        Material stoneGray      = CreateMaterial("StoneGray",      new Color(0.58f, 0.56f, 0.54f), 0.20f);
        Material bouncyRed      = CreateMaterial("BouncyRed",      new Color(0.90f, 0.12f, 0.12f), 0.70f);
        Material decorativeBlue = CreateMaterial("DecorativeBlue", new Color(0.14f, 0.38f, 0.88f), 0.65f);
        Material barkBrown      = CreateMaterial("BarkBrown",      new Color(0.38f, 0.22f, 0.08f), 0.15f);

        // Extra materials for decorative objects
        Material leafGreen = CreateMaterial("LeafGreen", new Color(0.12f, 0.48f, 0.10f), 0.15f);
        Material pathBeige = CreateMaterial("PathBeige", new Color(0.78f, 0.74f, 0.68f), 0.10f);

        // ------------------------------------------------------------------
        // STEP 4: Create the Physics Material — BouncyMaterial
        // ------------------------------------------------------------------
        PhysicsMaterial bouncyPhysicsMat = new PhysicsMaterial("BouncyMaterial");
        bouncyPhysicsMat.bounciness      = 0.8f;          // High bounciness for visible bounce
        bouncyPhysicsMat.dynamicFriction  = 0.4f;
        bouncyPhysicsMat.staticFriction   = 0.4f;
        bouncyPhysicsMat.bounceCombine    = PhysicsMaterialCombine.Maximum;  // Ensures max bounce is used
        bouncyPhysicsMat.frictionCombine  = PhysicsMaterialCombine.Average;

        AssetDatabase.CreateAsset(
            bouncyPhysicsMat,
            "Assets/PhysicsMaterials/BouncyMaterial.asset"
        );
        // Reload from disk to get a persistent reference
        bouncyPhysicsMat = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>(
            "Assets/PhysicsMaterials/BouncyMaterial.asset"
        );

        // ==================================================================
        //                    CREATE GAME OBJECTS
        // ==================================================================

        // ------------------------------------------------------------------
        // STEP 5: Ground Plane
        // ------------------------------------------------------------------
        // The Plane primitive comes with a MeshCollider by default,
        // which prevents objects from falling through the ground.
        GameObject parkGround = GameObject.CreatePrimitive(PrimitiveType.Plane);
        parkGround.name = "ParkGround";
        parkGround.transform.position   = Vector3.zero;
        parkGround.transform.localScale = new Vector3(5f, 1f, 5f); // 50×50 unit ground
        parkGround.GetComponent<Renderer>().sharedMaterial = grassGreen;
        parkGround.isStatic = true;

        // ------------------------------------------------------------------
        // STEP 6: Cubes (at least 3 required)
        // ------------------------------------------------------------------

        // BenchCube_1 — elongated flat cube representing a park bench
        GameObject benchCube1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        benchCube1.name = "BenchCube_1";
        benchCube1.transform.position   = new Vector3(-5f, 0.35f, 4f);
        benchCube1.transform.localScale = new Vector3(2.5f, 0.15f, 0.7f);
        benchCube1.GetComponent<Renderer>().sharedMaterial = woodBrown;
        benchCube1.isStatic = true;
        // Cube primitives come with a BoxCollider by default

        // Bench legs for BenchCube_1 (decorative sub-objects)
        CreateBenchLegs(benchCube1, woodBrown);

        // BenchCube_2 — another bench, rotated 90°
        GameObject benchCube2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        benchCube2.name = "BenchCube_2";
        benchCube2.transform.position   = new Vector3(5f, 0.35f, -3f);
        benchCube2.transform.rotation   = Quaternion.Euler(0f, 90f, 0f);
        benchCube2.transform.localScale = new Vector3(2.5f, 0.15f, 0.7f);
        benchCube2.GetComponent<Renderer>().sharedMaterial = woodBrown;
        benchCube2.isStatic = true;

        // Bench legs for BenchCube_2
        CreateBenchLegs(benchCube2, woodBrown);

        // ParkBlock_3 — decorative stone block
        GameObject parkBlock3 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        parkBlock3.name = "ParkBlock_3";
        parkBlock3.transform.position   = new Vector3(1f, 0.5f, -6f);
        parkBlock3.transform.localScale = new Vector3(1.5f, 1f, 1.5f);
        parkBlock3.GetComponent<Renderer>().sharedMaterial = stoneGray;
        parkBlock3.isStatic = true;

        // ------------------------------------------------------------------
        // STEP 7: Spheres (2 required)
        // ------------------------------------------------------------------

        // *** BouncingSphere — the key physics demonstration object ***
        // Positioned 8 units above ground so it falls visibly.
        // Has Rigidbody (Use Gravity = true) and BouncyMaterial on its collider.
        GameObject bouncingSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bouncingSphere.name = "BouncingSphere";
        bouncingSphere.transform.position   = new Vector3(0f, 8f, 0f);
        bouncingSphere.transform.localScale = Vector3.one * 0.8f;
        bouncingSphere.GetComponent<Renderer>().sharedMaterial = bouncyRed;

        // Add Rigidbody — required for physics simulation
        Rigidbody rbBounce       = bouncingSphere.AddComponent<Rigidbody>();
        rbBounce.mass            = 1f;
        rbBounce.useGravity      = true;   // Sphere will fall toward the ground
        rbBounce.linearDamping            = 0f;     // No air resistance for clean bounce
        rbBounce.angularDamping     = 0.05f;
        rbBounce.interpolation   = RigidbodyInterpolation.Interpolate;
        rbBounce.collisionDetectionMode = CollisionDetectionMode.Continuous;

        // Assign BouncyMaterial to the sphere's collider
        SphereCollider bounceSphereCollider = bouncingSphere.GetComponent<SphereCollider>();
        bounceSphereCollider.material = bouncyPhysicsMat;

        // DecorativeSphere — sits on the ground, demonstrates second Rigidbody
        GameObject decorativeSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        decorativeSphere.name = "DecorativeSphere";
        decorativeSphere.transform.position   = new Vector3(6f, 0.4f, 5f);
        decorativeSphere.transform.localScale = Vector3.one * 0.6f;
        decorativeSphere.GetComponent<Renderer>().sharedMaterial = decorativeBlue;

        // Second Rigidbody (requirement: at least 2 objects with Rigidbody)
        Rigidbody rbDecorative   = decorativeSphere.AddComponent<Rigidbody>();
        rbDecorative.mass        = 2f;
        rbDecorative.useGravity  = true;
        rbDecorative.linearDamping        = 0.5f;  // Slight drag so it settles and doesn't roll far

        // ------------------------------------------------------------------
        // STEP 8: Cylinder (at least 1 required)
        // ------------------------------------------------------------------
        // TreeTrunk — a cylinder used as a tree trunk
        // Grouped under a parent "Tree_1" for clean hierarchy organization
        GameObject tree1 = new GameObject("Tree_1");
        tree1.transform.position = new Vector3(-7f, 0f, -5f);

        GameObject treeTrunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        treeTrunk.name = "TreeTrunk";
        treeTrunk.transform.SetParent(tree1.transform);
        treeTrunk.transform.localPosition = new Vector3(0f, 1.5f, 0f);
        treeTrunk.transform.localScale    = new Vector3(0.6f, 1.5f, 0.6f);
        treeTrunk.GetComponent<Renderer>().sharedMaterial = barkBrown;
        treeTrunk.isStatic = true;
        // Cylinder comes with a CapsuleCollider by default — appropriate for this shape

        // Tree crown (decorative green foliage sphere)
        GameObject treeCrown1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        treeCrown1.name = "TreeCrown";
        treeCrown1.transform.SetParent(tree1.transform);
        treeCrown1.transform.localPosition = new Vector3(0f, 3.8f, 0f);
        treeCrown1.transform.localScale    = new Vector3(2.8f, 2.2f, 2.8f);
        treeCrown1.GetComponent<Renderer>().sharedMaterial = leafGreen;
        treeCrown1.isStatic = true;
        Object.DestroyImmediate(treeCrown1.GetComponent<SphereCollider>()); // Not needed

        // ------------------------------------------------------------------
        // STEP 9: Extra Decorative Objects
        // ------------------------------------------------------------------

        // Second tree (for a fuller park look)
        GameObject tree2 = new GameObject("Tree_2");
        tree2.transform.position = new Vector3(8f, 0f, -7f);

        GameObject treeTrunk2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        treeTrunk2.name = "TreeTrunk_2";
        treeTrunk2.transform.SetParent(tree2.transform);
        treeTrunk2.transform.localPosition = new Vector3(0f, 1.8f, 0f);
        treeTrunk2.transform.localScale    = new Vector3(0.5f, 1.8f, 0.5f);
        treeTrunk2.GetComponent<Renderer>().sharedMaterial = barkBrown;
        treeTrunk2.isStatic = true;

        GameObject treeCrown2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        treeCrown2.name = "TreeCrown_2";
        treeCrown2.transform.SetParent(tree2.transform);
        treeCrown2.transform.localPosition = new Vector3(0f, 4.4f, 0f);
        treeCrown2.transform.localScale    = new Vector3(3.2f, 2.5f, 3.2f);
        treeCrown2.GetComponent<Renderer>().sharedMaterial = leafGreen;
        treeCrown2.isStatic = true;
        Object.DestroyImmediate(treeCrown2.GetComponent<SphereCollider>());

        // Third tree (corner accent)
        GameObject tree3 = new GameObject("Tree_3");
        tree3.transform.position = new Vector3(-8f, 0f, -8f);

        GameObject treeTrunk3 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        treeTrunk3.name = "TreeTrunk_3";
        treeTrunk3.transform.SetParent(tree3.transform);
        treeTrunk3.transform.localPosition = new Vector3(0f, 2f, 0f);
        treeTrunk3.transform.localScale    = new Vector3(0.7f, 2f, 0.7f);
        treeTrunk3.GetComponent<Renderer>().sharedMaterial = barkBrown;
        treeTrunk3.isStatic = true;

        GameObject treeCrown3 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        treeCrown3.name = "TreeCrown_3";
        treeCrown3.transform.SetParent(tree3.transform);
        treeCrown3.transform.localPosition = new Vector3(0f, 4.8f, 0f);
        treeCrown3.transform.localScale    = new Vector3(3.5f, 2.8f, 3.5f);
        treeCrown3.GetComponent<Renderer>().sharedMaterial = leafGreen;
        treeCrown3.isStatic = true;
        Object.DestroyImmediate(treeCrown3.GetComponent<SphereCollider>());

        // Pathway running through the park (flat thin cube)
        GameObject pathway = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pathway.name = "Pathway";
        pathway.transform.position   = new Vector3(0f, 0.016f, 0f);
        pathway.transform.localScale = new Vector3(2.5f, 0.03f, 22f);
        pathway.GetComponent<Renderer>().sharedMaterial = pathBeige;
        pathway.isStatic = true;

        // Small decorative stone beside the pathway
        GameObject stoneDecor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        stoneDecor.name = "StoneDecor_1";
        stoneDecor.transform.position   = new Vector3(-2f, 0.2f, -2f);
        stoneDecor.transform.rotation   = Quaternion.Euler(0f, 35f, 0f);
        stoneDecor.transform.localScale = new Vector3(0.6f, 0.4f, 0.6f);
        stoneDecor.GetComponent<Renderer>().sharedMaterial = stoneGray;
        stoneDecor.isStatic = true;

        // Another small decorative stone
        GameObject stoneDecor2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        stoneDecor2.name = "StoneDecor_2";
        stoneDecor2.transform.position   = new Vector3(3f, 0.15f, 2f);
        stoneDecor2.transform.rotation   = Quaternion.Euler(0f, -20f, 5f);
        stoneDecor2.transform.localScale = new Vector3(0.4f, 0.3f, 0.5f);
        stoneDecor2.GetComponent<Renderer>().sharedMaterial = stoneGray;
        stoneDecor2.isStatic = true;

        // ------------------------------------------------------------------
        // STEP 10: Configure the Main Camera
        // ------------------------------------------------------------------
        // Elevated angled perspective for a clear overview of the entire park
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.gameObject.name         = "Main Camera";
            mainCam.transform.position      = new Vector3(15f, 12f, 15f);
            mainCam.transform.LookAt(new Vector3(0f, 1f, 0f));
            mainCam.backgroundColor         = new Color(0.53f, 0.76f, 0.98f); // Light sky blue
            mainCam.nearClipPlane           = 0.1f;
            mainCam.farClipPlane            = 300f;
            mainCam.fieldOfView             = 50f;
        }

        // ------------------------------------------------------------------
        // STEP 11: Configure the Directional Light
        // ------------------------------------------------------------------
        Light[] lights = Object.FindObjectsOfType<Light>();
        foreach (Light light in lights)
        {
            if (light.type == LightType.Directional)
            {
                light.gameObject.name    = "Directional Light";
                light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
                light.intensity          = 1.15f;
                light.shadows            = LightShadows.Soft;
                light.color              = new Color(1f, 0.96f, 0.90f); // Warm sunlight tint
            }
        }

        // Set ambient lighting for a pleasant outdoor feel
        RenderSettings.ambientLight = new Color(0.40f, 0.45f, 0.55f);

        // ------------------------------------------------------------------
        // STEP 12: Save the scene as ParkPhysicsEnvironment
        // ------------------------------------------------------------------
        string scenePath = "Assets/Scenes/ParkPhysicsEnvironment.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("✅ Park Physics Environment created and saved to: " + scenePath);

        EditorUtility.DisplayDialog(
            "Park Physics Environment — Created!",
            "The scene has been created and saved successfully.\n\n" +
            "📁 Scene:            Assets/Scenes/ParkPhysicsEnvironment.unity\n" +
            "🎨 Materials:        Assets/Materials/\n" +
            "⚽ Physics Material: Assets/PhysicsMaterials/BouncyMaterial\n\n" +
            "HOW TO TEST:\n" +
            "1. Press the Play button (▶) at the top of the Editor\n" +
            "2. Watch the red BouncingSphere fall and bounce off the ground\n" +
            "3. The blue DecorativeSphere will settle on the ground\n\n" +
            "TIP: Select 'BouncingSphere' in the Hierarchy to inspect its\n" +
            "Rigidbody, SphereCollider, and BouncyMaterial in the Inspector.",
            "OK"
        );
    }

    // =========================================================================
    // MENU ITEM: Verify that all scene requirements are met
    // =========================================================================
    [MenuItem("Park Physics/Verify Scene Requirements")]
    public static void VerifySceneRequirements()
    {
        System.Text.StringBuilder report = new System.Text.StringBuilder();
        report.AppendLine("========================================");
        report.AppendLine("  PARK PHYSICS — REQUIREMENT CHECKLIST");
        report.AppendLine("========================================\n");

        bool allPassed = true;

        // 1. Check for Plane (ground)
        int planeCount = 0;
        MeshFilter[] meshFilters = Object.FindObjectsOfType<MeshFilter>();
        foreach (var mf in meshFilters)
        {
            if (mf.sharedMesh != null && mf.sharedMesh.name == "Plane")
                planeCount++;
        }
        LogCheck(report, planeCount >= 1, "Ground Plane", planeCount + " Plane(s) found");
        if (planeCount < 1) allPassed = false;

        // 2. Check for Cubes (at least 3)
        int cubeCount = 0;
        foreach (var mf in meshFilters)
        {
            if (mf.sharedMesh != null && mf.sharedMesh.name == "Cube")
                cubeCount++;
        }
        LogCheck(report, cubeCount >= 3, "Cubes (≥3)", cubeCount + " Cube(s) found");
        if (cubeCount < 3) allPassed = false;

        // 3. Check for Spheres (at least 2)
        int sphereCount = 0;
        foreach (var mf in meshFilters)
        {
            if (mf.sharedMesh != null && mf.sharedMesh.name == "Sphere")
                sphereCount++;
        }
        LogCheck(report, sphereCount >= 2, "Spheres (≥2)", sphereCount + " Sphere(s) found");
        if (sphereCount < 2) allPassed = false;

        // 4. Check for Cylinder (at least 1)
        int cylinderCount = 0;
        foreach (var mf in meshFilters)
        {
            if (mf.sharedMesh != null && mf.sharedMesh.name == "Cylinder")
                cylinderCount++;
        }
        LogCheck(report, cylinderCount >= 1, "Cylinder (≥1)", cylinderCount + " Cylinder(s) found");
        if (cylinderCount < 1) allPassed = false;

        // 5. Check for Rigidbody components (at least 2)
        Rigidbody[] rigidbodies = Object.FindObjectsOfType<Rigidbody>();
        LogCheck(report, rigidbodies.Length >= 2, "Rigidbody (≥2)", rigidbodies.Length + " Rigidbody(ies) found");
        if (rigidbodies.Length < 2) allPassed = false;

        // 6. Check for BouncingSphere specifically
        GameObject bouncingSphere = GameObject.Find("BouncingSphere");
        bool hasBounce = false;
        if (bouncingSphere != null)
        {
            Rigidbody rb = bouncingSphere.GetComponent<Rigidbody>();
            SphereCollider sc = bouncingSphere.GetComponent<SphereCollider>();
            bool hasRb = rb != null && rb.useGravity;
            bool hasSc = sc != null;
            bool hasPhysMat = sc != null && sc.material != null && sc.material.bounciness >= 0.5f;

            LogCheck(report, hasRb, "BouncingSphere Rigidbody", hasRb ? "Rigidbody with gravity ✓" : "MISSING or gravity disabled");
            LogCheck(report, hasSc, "BouncingSphere Collider", hasSc ? "SphereCollider ✓" : "MISSING");
            LogCheck(report, hasPhysMat, "BouncyMaterial assigned",
                hasPhysMat ? "Bounciness = " + sc.material.bounciness : "MISSING or bounciness too low");

            hasBounce = hasRb && hasSc && hasPhysMat;
        }
        else
        {
            LogCheck(report, false, "BouncingSphere", "GameObject 'BouncingSphere' NOT FOUND");
        }
        if (!hasBounce) allPassed = false;

        // 7. Check Colliders
        Collider[] colliders = Object.FindObjectsOfType<Collider>();
        LogCheck(report, colliders.Length >= 6, "Colliders", colliders.Length + " Collider(s) found");

        // 8. Check Camera
        Camera cam = Camera.main;
        LogCheck(report, cam != null, "Main Camera", cam != null ? "Found at " + cam.transform.position : "NOT FOUND");

        // Summary
        report.AppendLine("\n========================================");
        if (allPassed)
        {
            report.AppendLine("  ✅ ALL REQUIREMENTS PASSED!");
        }
        else
        {
            report.AppendLine("  ❌ SOME REQUIREMENTS NOT MET — see above");
        }
        report.AppendLine("========================================");

        Debug.Log(report.ToString());

        EditorUtility.DisplayDialog(
            allPassed ? "✅ All Requirements Passed!" : "❌ Some Requirements Not Met",
            allPassed
                ? "All assignment requirements are satisfied.\nCheck the Console for the full report."
                : "Some requirements are not met.\nCheck the Console for details.",
            "OK"
        );
    }

    // =========================================================================
    //                         HELPER METHODS
    // =========================================================================

    /// <summary>
    /// Ensures an asset sub-folder exists inside the given parent folder.
    /// </summary>
    private static void EnsureFolder(string parentFolder, string newFolderName)
    {
        string fullPath = parentFolder + "/" + newFolderName;
        if (!AssetDatabase.IsValidFolder(fullPath))
        {
            AssetDatabase.CreateFolder(parentFolder, newFolderName);
        }
    }

    /// <summary>
    /// Creates a material using the Standard shader (or URP fallback),
    /// saves it as an asset, and returns the loaded reference.
    /// </summary>
    private static Material CreateMaterial(string name, Color color, float smoothness)
    {
        string path = "Assets/Materials/" + name + ".mat";

        // Find the best available shader
        Shader shader = Shader.Find("Standard");
        if (shader == null)
            shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Unlit/Color");
            Debug.LogWarning("[ParkSceneCreator] Standard shader not found. Using fallback for: " + name);
        }

        Material mat = new Material(shader);
        mat.name  = name;
        mat.color = color;

        // Set smoothness (property name differs between Standard and URP)
        if (mat.HasProperty("_Glossiness"))
            mat.SetFloat("_Glossiness", smoothness);
        if (mat.HasProperty("_Smoothness"))
            mat.SetFloat("_Smoothness", smoothness);

        AssetDatabase.CreateAsset(mat, path);
        return AssetDatabase.LoadAssetAtPath<Material>(path);
    }

    /// <summary>
    /// Creates four small cube legs underneath a bench seat for visual detail.
    /// </summary>
    private static void CreateBenchLegs(GameObject benchSeat, Material legMaterial)
    {
        float seatWidth  = benchSeat.transform.localScale.x;
        float seatHeight = benchSeat.transform.position.y;
        float seatDepth  = benchSeat.transform.localScale.z;

        float legHeight    = seatHeight - 0.05f;
        float halfWidth    = (seatWidth / 2f) - 0.15f;
        float halfDepth    = (seatDepth / 2f) - 0.08f;

        Vector3[] legOffsets = new Vector3[]
        {
            new Vector3(-halfWidth, 0f,  halfDepth),
            new Vector3( halfWidth, 0f,  halfDepth),
            new Vector3(-halfWidth, 0f, -halfDepth),
            new Vector3( halfWidth, 0f, -halfDepth),
        };

        for (int i = 0; i < 4; i++)
        {
            GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leg.name = benchSeat.name + "_Leg_" + (i + 1);
            leg.transform.SetParent(benchSeat.transform);

            // Position leg beneath the seat (in local space)
            Vector3 localPos = benchSeat.transform.InverseTransformPoint(
                benchSeat.transform.position + benchSeat.transform.rotation * legOffsets[i]
            );
            // Adjust Y so leg sits below the seat top
            localPos.y = -(legHeight / benchSeat.transform.localScale.y) / 2f;

            leg.transform.localPosition = localPos;
            leg.transform.localScale    = new Vector3(
                0.08f / benchSeat.transform.localScale.x,
                legHeight / benchSeat.transform.localScale.y,
                0.08f / benchSeat.transform.localScale.z
            );
            leg.transform.localRotation = Quaternion.identity;
            leg.GetComponent<Renderer>().sharedMaterial = legMaterial;
            leg.isStatic = true;
        }
    }

    /// <summary>
    /// Helper for the verification report: logs a pass/fail line.
    /// </summary>
    private static void LogCheck(System.Text.StringBuilder sb, bool passed, string label, string detail)
    {
        string icon = passed ? "✅" : "❌";
        sb.AppendLine(icon + " " + label.PadRight(28) + " — " + detail);
    }
}

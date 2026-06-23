using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class WeaponSystemTestSceneSetupTool
{
    private const string RootDir = "Assets/AI/WeaponSystem/UnityTest";
    private const string PrefabDir = RootDir + "/Prefabs";
    private const string DataDir = RootDir + "/Data";
    private const string SceneDir = "Assets/AI/WeaponSystem/TestScenes";
    private const string ScenePath = SceneDir + "/WeaponSystemTest.unity";
    private const string MaterialDir = RootDir + "/Materials";

    private const string AssetsRawDir = "Assets/AI/WeaponSystem/WeaponSystemUnityPackage/AssetsRaw/weapons";
    private const string PistolPng = AssetsRawDir + "/ranged/pistol/pistol.png";
    private const string PistolIconPng = AssetsRawDir + "/ranged/pistol/pistol_icon.png";
    private const string SpearPng = AssetsRawDir + "/melee/spear/spear.png";
    private const string SpearIconPng = AssetsRawDir + "/melee/spear/spear_icon.png";
    private const string SwordPng = AssetsRawDir + "/melee/sword/sword.png";
    private const string SwordIconPng = AssetsRawDir + "/melee/sword/sword_icon.png";

    private const string ResWeaponsDir = "Assets/Resources/Weapons";
    private const string ResProjectilesDir = ResWeaponsDir + "/Projectiles";
    private const string ResGenericWeaponPath = ResWeaponsDir + "/GenericWeapon.prefab";
    private const string ResBulletPath = ResProjectilesDir + "/bullet.prefab";
    private const string JsonScenePath = SceneDir + "/WeaponSystemJsonTest.unity";

    [MenuItem("Tools/WeaponSystem/Create Runtime Resources (Generic + Bullet)")]
    public static void CreateRuntimeResources()
    {
        EnsureDirectory(ResWeaponsDir);
        EnsureDirectory(ResProjectilesDir);
        EnsureDirectory(MaterialDir);
        if (!AssetDatabase.IsValidFolder(ResProjectilesDir)) return;

        CreateBulletPrefab(ResBulletPath);
        CreateGenericWeaponPrefab(ResGenericWeaponPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[WeaponSystemTest] 已生成运行时 Resources：{ResGenericWeaponPath}、{ResBulletPath}");
    }

    [MenuItem("Tools/WeaponSystem/Create Test Scene (JSON-driven)")]
    public static void CreateJsonDrivenTestScene()
    {
        EnsureDirectory(SceneDir);
        int enemyLayer = EnsureLayer("Enemy");
        CreateRuntimeResources();

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "WeaponSystemJsonTest";
        CreateCamera();
        new GameObject("ResourceManager").AddComponent<ResourceManager>();
        new GameObject("PoolManager").AddComponent<PoolManager>();

        GameObject player = new GameObject("WeaponTestPlayer");
        player.tag = "Player";
        player.transform.position = Vector3.zero;
        GameObject weaponHolder = new GameObject("WeaponHolderCenter");
        weaponHolder.transform.SetParent(player.transform, false);

        WeaponManager weaponManager = player.AddComponent<WeaponManager>();
        weaponManager.weaponHolderCenter = weaponHolder.transform;
        weaponManager.weaponOrbitRadius = 0.8f;
        weaponManager.startingWeaponIds = new List<int> { 201, 202, 203 };

        int ringCount = 8;
        for (int i = 0; i < ringCount; i++)
        {
            float ang = i * Mathf.PI * 2f / ringCount;
            Vector3 pos = new Vector3(Mathf.Cos(ang), Mathf.Sin(ang), 0f) * 1.5f;
            CreateDummy($"Dummy_{i}", pos, enemyLayer);
        }

        EditorSceneManager.SaveScene(scene, JsonScenePath);
        AddScenesToBuildSettings(JsonScenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[WeaponSystemTest] 已创建 JSON 驱动测试场景: {JsonScenePath}（请确认已执行 Excel->JSON 转换）");
    }

    [MenuItem("Tools/WeaponSystem/Create Weapon System Test Scene")]
    public static void CreateWeaponSystemTestScene()
    {
        EnsureDirectory(PrefabDir);
        EnsureDirectory(DataDir);
        EnsureDirectory(MaterialDir);
        EnsureDirectory(SceneDir);
        int enemyLayer = EnsureLayer("Enemy");

        Sprite pistolSprite = LoadWeaponSprite(PistolPng);
        Sprite spearSprite = LoadWeaponSprite(SpearPng);
        Sprite swordSprite = LoadWeaponSprite(SwordPng);

        GameObject bulletPrefab = CreateBulletPrefab(PrefabDir + "/TestBullet.prefab");
        GameObject genericWeaponPrefab = CreateGenericWeaponPrefab(PrefabDir + "/GenericWeapon.prefab");
        WeaponData pistolData = CreateTestWeaponData("PistolTestWeaponData", WeaponKind.Ranged, null, bulletPrefab, enemyLayer);
        WeaponData spearData = CreateTestWeaponData("SpearTestWeaponData", WeaponKind.Melee, null, null, enemyLayer);
        WeaponData swordData = CreateTestWeaponData("SwordTestWeaponData", WeaponKind.Melee, null, null, enemyLayer);

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "WeaponSystemTest";
        CreateCamera();
        new GameObject("PoolManager").AddComponent<PoolManager>();

        GameObject player = new GameObject("WeaponTestPlayer");
        player.tag = "Player";
        player.transform.position = Vector3.zero;
        GameObject weaponHolder = new GameObject("WeaponHolderCenter");
        weaponHolder.transform.SetParent(player.transform, false);

        WeaponManager weaponManager = player.AddComponent<WeaponManager>();
        weaponManager.weaponHolderCenter = weaponHolder.transform;
        weaponManager.weaponOrbitRadius = 0.8f;
        weaponManager.genericWeaponPrefab = genericWeaponPrefab;
        weaponManager.startingWeapons = new List<WeaponData> { pistolData, spearData, swordData };

        int ringCount = 8;
        for (int i = 0; i < ringCount; i++)
        {
            float ang = i * Mathf.PI * 2f / ringCount;
            Vector3 pos = new Vector3(Mathf.Cos(ang), Mathf.Sin(ang), 0f) * 1.5f;
            CreateDummy($"Dummy_{i}", pos, enemyLayer);
        }

        EditorSceneManager.SaveScene(scene, ScenePath);
        AddScenesToBuildSettings(ScenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[WeaponSystemTest] 已创建武器测试场景: {ScenePath}");
    }

    private static GameObject CreateBulletPrefab(string path)
    {
        GameObject root = new GameObject("TestBullet");
        root.AddComponent<Bullet>();
        root.AddComponent<CircleCollider2D>().isTrigger = true;
        CreateVisualQuad(root.transform, "Visual", new Vector3(0.18f, 0.18f, 1f), Color.yellow);
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return prefab;
    }

    private static GameObject CreateGenericWeaponPrefab(string path)
    {
        GameObject root = new GameObject("GenericWeapon");
        WeaponInstance instance = root.AddComponent<WeaponInstance>();
        GameObject sprite = new GameObject("WeaponSprite");
        sprite.transform.SetParent(root.transform, false);
        SpriteRenderer renderer = sprite.AddComponent<SpriteRenderer>();
        renderer.sortingOrder = 5;
        instance.weaponSpriteRoot = sprite.transform;
        GameObject muzzle = new GameObject("Muzzle");
        muzzle.transform.SetParent(root.transform, false);
        instance.muzzle = muzzle.transform;
        GameObject hitbox = new GameObject("Hitbox");
        hitbox.transform.SetParent(root.transform, false);
        instance.meleeHitbox = hitbox.AddComponent<WeaponHitbox>();
        hitbox.SetActive(false);
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return prefab;
    }

    private static WeaponData CreateTestWeaponData(string assetName, WeaponKind kind, GameObject weaponPrefab, GameObject projectilePrefab, int enemyLayer)
    {
        string path = DataDir + "/" + assetName + ".asset";
        WeaponData data = AssetDatabase.LoadAssetAtPath<WeaponData>(path);
        if (data == null) { data = ScriptableObject.CreateInstance<WeaponData>(); AssetDatabase.CreateAsset(data, path); }
        data.weaponName = assetName;
        data.weaponKind = kind;
        data.isMelee = kind == WeaponKind.Melee;
        data.weaponPrefab = weaponPrefab;
        data.projectilePrefab = projectilePrefab;
        data.damage = kind == WeaponKind.Melee ? 15f : 12f;
        data.attackSpeed = kind == WeaponKind.Melee ? 0.75f : 1f;
        data.range = kind == WeaponKind.Melee ? 3.5f : 4f;
        data.flySpeed = 30f;
        data.hitLayers = 1 << enemyLayer;
        data.maxLifeTime = 0.5f;
        data.destroyOnHit = true;
        data.recoilDistance = 0.25f;
        data.recoilDuration = 0.1f;
        data.hitboxSize = new Vector2(2.8f, 0.32f);
        data.hitboxOffset = new Vector2(1.4f, 0f);
        data.meleeThrustDistance = 0.7f;
        data.meleeWindupDuration = 0.06f;
        data.meleeActiveDuration = 0.12f;
        data.meleeReturnDuration = 0.1f;
        data.critChance = 0.2f;
        data.critMultiplier = 2f;
        data.knockback = 10f;
        data.piercing = 3;
        data.bounce = 1;
        EditorUtility.SetDirty(data);
        return data;
    }

    private static Sprite LoadWeaponSprite(string assetPath)
    {
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null) { Debug.LogWarning($"[WeaponSystemTest] 找不到贴图: {assetPath}"); return null; }
        bool dirty = false;
        if (importer.textureType != TextureImporterType.Sprite) { importer.textureType = TextureImporterType.Sprite; dirty = true; }
        if (importer.spriteImportMode != SpriteImportMode.Single) { importer.spriteImportMode = SpriteImportMode.Single; dirty = true; }
        if (importer.spritePixelsPerUnit != 100) { importer.spritePixelsPerUnit = 100; dirty = true; }
        if (dirty) importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
    }

    private static void CreateDummy(string name, Vector3 pos, int enemyLayer)
    {
        GameObject dummy = new GameObject(name);
        dummy.layer = enemyLayer;
        dummy.transform.position = pos;
        CreateVisualQuad(dummy.transform, "Visual", new Vector3(0.5f, 0.5f, 1f), Color.red);
        dummy.AddComponent<BoxCollider2D>();
        Rigidbody2D rb = dummy.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        dummy.AddComponent<WeaponTestDummy>();
    }

    private static void CreateVisualQuad(Transform parent, string name, Vector3 scale, Color color)
    {
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Quad);
        visual.name = name;
        visual.transform.SetParent(parent, false);
        visual.transform.localScale = scale;
        Collider c = visual.GetComponent<Collider>();
        if (c != null) Object.DestroyImmediate(c);
        visual.GetComponent<MeshRenderer>().sharedMaterial = GetMaterial(color);
    }

    private static Material GetMaterial(Color color)
    {
        string colorName = ColorUtility.ToHtmlStringRGBA(color);
        string path = MaterialDir + "/Mat_" + colorName + ".mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat != null) return mat;
        Shader shader = Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Color");
        mat = new Material(shader) { color = color };
        AssetDatabase.CreateAsset(mat, path);
        return mat;
    }

    private static void CreateCamera()
    {
        GameObject cam = new GameObject("Main Camera");
        cam.tag = "MainCamera";
        cam.transform.position = new Vector3(0, 0, -10);
        Camera c = cam.AddComponent<Camera>();
        c.clearFlags = CameraClearFlags.SolidColor;
        c.backgroundColor = new Color(0.06f, 0.07f, 0.09f);
        c.orthographic = true;
        c.orthographicSize = 5f;
        cam.AddComponent<AudioListener>();
    }

    private static int EnsureLayer(string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        if (layer >= 0) return layer;
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layers = tagManager.FindProperty("layers");
        for (int i = 8; i < layers.arraySize; i++)
        {
            var prop = layers.GetArrayElementAtIndex(i);
            if (!string.IsNullOrEmpty(prop.stringValue)) continue;
            prop.stringValue = layerName;
            tagManager.ApplyModifiedProperties();
            return i;
        }
        Debug.LogWarning($"[WeaponSystemTest] 无法自动创建 Layer: {layerName}");
        return 0;
    }

    private static void EnsureDirectory(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string[] parts = path.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }

    private static void AddScenesToBuildSettings(params string[] scenePaths)
    {
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        foreach (string p in scenePaths)
            if (!scenes.Exists(s => s.path == p)) scenes.Add(new EditorBuildSettingsScene(p, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}

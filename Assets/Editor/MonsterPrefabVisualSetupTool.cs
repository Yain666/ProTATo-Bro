using System.IO;
using UnityEditor;
using UnityEngine;

public static class MonsterPrefabVisualSetupTool
{
    private const string MonsterPrefabFolder = "Assets/Resources/Prefab/Monster";
    private const string BackupFolder = "Assets/Resources/Prefab/Monster_Backup";

    private static readonly string[] TargetPrefabs =
    {
        "NormalZombie_Prefab.prefab",
        "RangedSkeleton_Prefab.prefab",
        "EliteBoar_Prefab.prefab"
    };

    [MenuItem("Tools/Monster/Backup Monster Prefabs")]
    public static void BackupMonsterPrefabs()
    {
        EnsureFolder("Assets/Resources");
        EnsureFolder("Assets/Resources/Prefab");
        EnsureFolder(BackupFolder);

        for (int i = 0; i < TargetPrefabs.Length; i++)
        {
            string fileName = TargetPrefabs[i];
            string src = Path.Combine(MonsterPrefabFolder, fileName).Replace("\\", "/");
            string dst = Path.Combine(BackupFolder, fileName).Replace("\\", "/");

            if (!File.Exists(src))
            {
                Debug.LogWarning($"[MonsterPrefabVisualSetupTool] 找不到源 prefab: {src}");
                continue;
            }

            if (AssetDatabase.LoadAssetAtPath<GameObject>(dst) != null)
            {
                AssetDatabase.DeleteAsset(dst);
            }

            AssetDatabase.CopyAsset(src, dst);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[MonsterPrefabVisualSetupTool] 已备份目标怪物 prefab。");
    }

    [MenuItem("Tools/Monster/Restore Monster Prefabs From Backup")]
    public static void RestoreMonsterPrefabsFromBackup()
    {
        for (int i = 0; i < TargetPrefabs.Length; i++)
        {
            string fileName = TargetPrefabs[i];
            string src = Path.Combine(BackupFolder, fileName).Replace("\\", "/");
            string dst = Path.Combine(MonsterPrefabFolder, fileName).Replace("\\", "/");

            if (AssetDatabase.LoadAssetAtPath<GameObject>(src) == null)
            {
                Debug.LogWarning($"[MonsterPrefabVisualSetupTool] 找不到备份 prefab: {src}");
                continue;
            }

            AssetDatabase.DeleteAsset(dst);
            AssetDatabase.CopyAsset(src, dst);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[MonsterPrefabVisualSetupTool] 已从备份恢复目标怪物 prefab。");
    }

    [MenuItem("Tools/Monster/Prepare Visual Roots")]
    public static void PrepareVisualRoots()
    {
        BackupMonsterPrefabs();

        for (int i = 0; i < TargetPrefabs.Length; i++)
        {
            string fileName = TargetPrefabs[i];
            string prefabPath = Path.Combine(MonsterPrefabFolder, fileName).Replace("\\", "/");
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
            if (prefabRoot == null)
            {
                Debug.LogWarning($"[MonsterPrefabVisualSetupTool] 无法打开 prefab: {prefabPath}");
                continue;
            }

            try
            {
                PreparePrefab(prefabRoot);
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[MonsterPrefabVisualSetupTool] 已完成怪物视觉层级整理。");
    }

    private static void PreparePrefab(GameObject prefabRoot)
    {
        SpriteRenderer rootRenderer = prefabRoot.GetComponent<SpriteRenderer>();
        if (rootRenderer == null)
        {
            Debug.LogWarning($"[MonsterPrefabVisualSetupTool] 根节点上没有 SpriteRenderer: {prefabRoot.name}");
            return;
        }

        Transform visualRoot = prefabRoot.transform.Find("VisualRoot");
        if (visualRoot == null)
        {
            GameObject go = new GameObject("VisualRoot");
            visualRoot = go.transform;
            visualRoot.SetParent(prefabRoot.transform, false);
            visualRoot.SetSiblingIndex(prefabRoot.transform.childCount);
        }

        Transform body = visualRoot.Find("Body");
        if (body == null)
        {
            GameObject go = new GameObject("Body");
            body = go.transform;
            body.SetParent(visualRoot, false);
        }

        SpriteRenderer bodyRenderer = body.GetComponent<SpriteRenderer>();
        if (bodyRenderer == null)
        {
            bodyRenderer = body.gameObject.AddComponent<SpriteRenderer>();
        }

        CopyRenderer(rootRenderer, bodyRenderer);
        body.localPosition = Vector3.zero;
        body.localRotation = Quaternion.identity;
        body.localScale = Vector3.one;

        rootRenderer.enabled = false;

        Monster monster = prefabRoot.GetComponent<Monster>();
        if (monster != null)
        {
            MonsterVisualController visualController = prefabRoot.GetComponent<MonsterVisualController>();
            if (visualController == null)
            {
                visualController = prefabRoot.AddComponent<MonsterVisualController>();
            }

            monster.VisualController = visualController;
            EditorUtility.SetDirty(monster);
        }

        MonsterDamageFlash damageFlash = prefabRoot.GetComponent<MonsterDamageFlash>();
        if (damageFlash != null)
        {
            damageFlash.targetRenderer = bodyRenderer;
            EditorUtility.SetDirty(damageFlash);
        }
    }

    private static void CopyRenderer(SpriteRenderer source, SpriteRenderer target)
    {
        target.sprite = source.sprite;
        target.color = source.color;
        target.flipX = source.flipX;
        target.flipY = source.flipY;
        target.drawMode = source.drawMode;
        target.size = source.size;
        target.sortingLayerID = source.sortingLayerID;
        target.sortingOrder = source.sortingOrder;
        target.maskInteraction = source.maskInteraction;
        target.sharedMaterial = source.sharedMaterial;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
        {
            return;
        }

        int slash = path.LastIndexOf('/');
        if (slash <= 0)
        {
            return;
        }

        string parent = path.Substring(0, slash);
        string folderName = path.Substring(slash + 1);
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, folderName);
    }
}

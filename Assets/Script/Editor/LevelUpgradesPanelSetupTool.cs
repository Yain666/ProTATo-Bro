using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class LevelUpgradesPanelSetupTool
{
    private const string PrefabPath = "Assets/Resources/UI/Panels/LevelUpgradesPanel.prefab";

    [MenuItem("Tools/UI/Create LevelUpgrades Prefab")]
    public static void CreatePrefab()
    {
        EnsureDirectory("Assets/Resources/UI/Panels");

        GameObject tempParent = new GameObject("LevelUpgradesPrefabTemp", typeof(RectTransform));
        RectTransform tempRect = tempParent.GetComponent<RectTransform>();
        Stretch(tempRect);

        LevelUpgradesPanel panel = LevelUpgradesRuntimeFactory.GetOrCreate(tempParent.transform);
        if (panel == null)
        {
            Object.DestroyImmediate(tempParent);
            Debug.LogError("[LevelUpgradesPanelSetupTool] 创建升级面板失败。");
            return;
        }

        panel.gameObject.SetActive(true);
        PrefabUtility.SaveAsPrefabAsset(panel.gameObject, PrefabPath);
        Object.DestroyImmediate(tempParent);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[LevelUpgradesPanelSetupTool] 已创建升级面板 Prefab: {PrefabPath}");
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void EnsureDirectory(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
        {
            return;
        }

        string parent = System.IO.Path.GetDirectoryName(path)?.Replace('\\', '/');
        string folder = System.IO.Path.GetFileName(path);
        if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
        {
            EnsureDirectory(parent);
        }

        if (!string.IsNullOrEmpty(parent) && !string.IsNullOrEmpty(folder))
        {
            AssetDatabase.CreateFolder(parent, folder);
        }
    }
}

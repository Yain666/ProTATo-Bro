using UnityEditor;
using UnityEngine;

public static class PlayerPrefabSetupTool
{
    private const string DefaultScenePlayerName = "PlayerGirl";
    private const string PrefabPath = "Assets/Resources/Prefab/Player/BattlePlayer.prefab";

    [MenuItem("Tools/Player/Save Battle Player Prefab")]
    public static void SaveBattlePlayerPrefab()
    {
        GameObject scenePlayer = Selection.activeGameObject;
        if (scenePlayer == null || scenePlayer.GetComponent<PlayerController>() == null)
        {
            scenePlayer = GameObject.Find(DefaultScenePlayerName);
        }

        if (scenePlayer == null)
        {
            Debug.LogError("[PlayerPrefabSetupTool] 当前场景里找不到可保存的玩家对象。请选中 PlayerGirl 或保持场景里存在 PlayerGirl。");
            return;
        }

        EnsureRequiredComponents(scenePlayer);
        EnsureFolder("Assets/Resources");
        EnsureFolder("Assets/Resources/Prefab");
        EnsureFolder("Assets/Resources/Prefab/Player");

        PrefabUtility.SaveAsPrefabAssetAndConnect(scenePlayer, PrefabPath, InteractionMode.UserAction);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[PlayerPrefabSetupTool] 已保存战斗玩家 Prefab: {PrefabPath}");
    }

    [MenuItem("Tools/Player/Select Battle Player Prefab")]
    public static void SelectBattlePlayerPrefab()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (prefab == null)
        {
            Debug.LogWarning($"[PlayerPrefabSetupTool] 尚未找到 Prefab: {PrefabPath}");
            return;
        }

        Selection.activeObject = prefab;
        EditorGUIUtility.PingObject(prefab);
    }

    private static void EnsureRequiredComponents(GameObject scenePlayer)
    {
        if (scenePlayer.GetComponent<PlayerVisualController>() == null)
        {
            scenePlayer.AddComponent<PlayerVisualController>();
        }

        PlayerController playerController = scenePlayer.GetComponent<PlayerController>();
        if (playerController != null && playerController.visualController == null)
        {
            playerController.visualController = scenePlayer.GetComponent<PlayerVisualController>();
            EditorUtility.SetDirty(playerController);
        }
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

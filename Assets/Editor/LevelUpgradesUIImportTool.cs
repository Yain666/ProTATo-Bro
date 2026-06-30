using UnityEditor;

public static class LevelUpgradesUIImportTool
{
    [MenuItem("Tools/UI/Fix LevelUpgrade Icons Import")]
    public static void FixLevelUpgradeIconsImportMenu()
    {
        ApplyImportSettings();
        AssetDatabase.Refresh();
    }

    [InitializeOnLoadMethod]
    private static void EnsureLevelUpgradesSprites()
    {
        EditorApplication.delayCall += ApplyImportSettings;
    }

    private static void ApplyImportSettings()
    {
        string[] assetPaths =
        {
            "Assets/Resources/UI/Panels/LevelUpgradesUI/Icons",
            "Assets/Resources/UI/Panels/LevelUpgradesUI/UIAssets"
        };

        for (int i = 0; i < assetPaths.Length; i++)
        {
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { assetPaths[i] });
            for (int j = 0; j < guids.Length; j++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[j]);
                TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (importer == null)
                {
                    continue;
                }

                bool changed = false;
                if (importer.textureType != TextureImporterType.Sprite)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    changed = true;
                }

                if (importer.mipmapEnabled)
                {
                    importer.mipmapEnabled = false;
                    changed = true;
                }

                if (!importer.alphaIsTransparency)
                {
                    importer.alphaIsTransparency = true;
                    changed = true;
                }

                if (changed)
                {
                    importer.SaveAndReimport();
                }
            }
        }
    }
}

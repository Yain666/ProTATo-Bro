using UnityEngine;

public static class WeaponEffectUtility
{
    private static Sprite _fallbackSprite;

    public static void PlayImpactFlash(WeaponData weaponData, Vector3 worldPosition, Vector3 forward)
    {
        if (weaponData == null || weaponData.IsMeleeWeapon)
        {
            return;
        }

        Sprite sprite = LoadWeaponSprite(weaponData.weaponName, "击中特效1");

        if (sprite == null)
        {
            sprite = GetFallbackSprite();
        }

        Quaternion rotation = forward.sqrMagnitude > 0.0001f
            ? Quaternion.Euler(0f, 0f, Mathf.Atan2(forward.y, forward.x) * Mathf.Rad2Deg)
            : Quaternion.identity;

        SpawnTransientSprite(worldPosition, rotation, sprite, 0.2f, 0.22f, new Color(1f, 0.98f, 0.84f, 0.92f), 11);
    }

    private static Sprite LoadWeaponSprite(string weaponName, string assetName)
    {
        if (string.IsNullOrEmpty(weaponName) || string.IsNullOrEmpty(assetName))
        {
            return null;
        }

        string path = $"Weapon/{weaponName}/{assetName}";
        return Resources.Load<Sprite>(path);
    }

    private static Sprite GetFallbackSprite()
    {
        if (_fallbackSprite != null)
        {
            return _fallbackSprite;
        }

        Texture2D texture = Texture2D.whiteTexture;
        _fallbackSprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), texture.width);
        return _fallbackSprite;
    }

    private static void SpawnTransientSprite(Vector3 worldPosition, Quaternion rotation, Sprite sprite, float lifetime, float scale, Color color, int sortingOrder)
    {
        GameObject root = new GameObject($"Fx_{sprite.name}");
        root.transform.position = worldPosition;
        root.transform.rotation = rotation;

        SpriteRenderer renderer = root.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;

        root.transform.localScale = new Vector3(scale, scale, 1f);
        root.AddComponent<TransientSpriteEffect>().Initialize(lifetime, 1.08f);
    }
}

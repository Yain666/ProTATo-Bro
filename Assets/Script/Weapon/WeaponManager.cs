using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public List<WeaponData> startingWeapons;
    public List<int> startingWeaponIds;
    public GameObject genericWeaponPrefab;
    public Transform weaponHolderCenter;
    public float weaponOrbitRadius = 1.5f;

    private const string GenericWeaponResPath = "Weapons/GenericWeapon";
    private const int MaxWeapons = 6;
    private List<WeaponInstance> _activeWeapons = new List<WeaponInstance>();

    private static readonly Vector2[][] MountLayouts =
    {
        new Vector2[0],
        new[] { new Vector2(0f, -0.06f) },
        new[] { new Vector2(0.40f, 0.04f), new Vector2(-0.40f, 0.04f) },
        new[] { new Vector2(0.40f, 0.04f), new Vector2(-0.40f, 0.04f), new Vector2(0f, 0.69f) },
        new[] { new Vector2(0.40f, -0.11f), new Vector2(-0.40f, -0.11f), new Vector2(-0.40f, 0.54f), new Vector2(0.40f, 0.54f) },
        new[] { new Vector2(0.35f, -0.11f), new Vector2(-0.35f, -0.11f), new Vector2(-0.55f, 0.44f), new Vector2(0.55f, 0.44f), new Vector2(0f, 0.79f) },
        new[] { new Vector2(0.35f, -0.21f), new Vector2(-0.35f, -0.21f), new Vector2(-0.60f, 0.24f), new Vector2(0.60f, 0.24f), new Vector2(-0.35f, 0.69f), new Vector2(0.35f, 0.69f) },
    };

    private void OnEnable() { EventSystem.OnWeaponsChanged += HandleWeaponsChanged; }
    private void OnDisable() { EventSystem.OnWeaponsChanged -= HandleWeaponsChanged; }

    private void HandleWeaponsChanged(IReadOnlyList<OwnedWeapon> owned)
    {
        foreach (var w in _activeWeapons) { if (w != null) Destroy(w.gameObject); }
        _activeWeapons.Clear();
        if (owned != null)
        {
            WeaponDataController.Initialize();
            foreach (var ow in owned)
            {
                WeaponConfigData cfg = WeaponDataController.Instance.GetWeaponData(ow.id);
                if (cfg == null) continue;
                WeaponInstance inst = InstantiateWeapon(WeaponRuntimeFactory.Build(cfg, ow.grade));
                if (inst != null) _activeWeapons.Add(inst);
            }
        }
        RepositionWeapons();
    }

    void Start()
    {
        if (startingWeaponIds != null && startingWeaponIds.Count > 0)
        {
            WeaponDataController.Initialize();
            foreach (int id in startingWeaponIds) AddWeaponById(id);
        }
        if (startingWeapons != null)
            foreach (var data in startingWeapons) AddWeapon(data);

        SyncFromShopInventory();
    }

    private void SyncFromShopInventory()
    {
        if (startingWeaponIds != null && startingWeaponIds.Count > 0) return;
        ShopSystem shop = FindObjectOfType<ShopSystem>();
        if (shop == null) return;
        var owned = shop.OwnedWeapons;
        if (owned == null || owned.Count == 0) return;
        HandleWeaponsChanged(owned);
    }

    public bool AddWeaponById(int id)
    {
        WeaponDataController.Initialize();
        WeaponConfigData cfg = WeaponDataController.Instance.GetWeaponData(id);
        if (cfg == null) return false;
        return AddWeapon(WeaponRuntimeFactory.Build(cfg, cfg.min_grade));
    }

    public bool AddWeapon(WeaponData data)
    {
        if (data == null || _activeWeapons.Count >= MaxWeapons) return false;
        WeaponInstance inst = InstantiateWeapon(data);
        if (inst == null) return false;
        _activeWeapons.Add(inst);
        RepositionWeapons();
        return true;
    }

    private WeaponInstance InstantiateWeapon(WeaponData data)
    {
        if (data == null) return null;
        GameObject prefab = data.weaponPrefab != null ? data.weaponPrefab : GetGenericPrefab();
        if (prefab == null)
        {
            Debug.LogError($"[WeaponManager] 找不到 prefab: {data.weaponName}");
            return null;
        }
        GameObject obj = Instantiate(prefab, weaponHolderCenter);
        WeaponInstance inst = obj.GetComponent<WeaponInstance>();
        inst.Initialize(data, transform);
        return inst;
    }

    private GameObject GetGenericPrefab()
    {
        if (genericWeaponPrefab == null) genericWeaponPrefab = Resources.Load<GameObject>(GenericWeaponResPath);
        return genericWeaponPrefab;
    }

    private void RepositionWeapons()
    {
        int count = _activeWeapons.Count;
        if (count == 0) return;
        for (int i = 0; i < count; i++)
            _activeWeapons[i].transform.localPosition = GetMountOffset(i, count);
    }

    private Vector2 GetMountOffset(int index, int count)
    {
        if (count >= 1 && count < MountLayouts.Length) return MountLayouts[count][index];
        float radius = (60f + (count - 6) * 5f) / 100f;
        float angle = index * (2f * Mathf.PI / count);
        return new Vector2(radius * Mathf.Cos(angle), radius * Mathf.Sin(angle));
    }
}

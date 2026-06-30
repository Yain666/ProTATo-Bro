using System.Collections.Generic;
using Script.Player.PlayerComponent;
using UnityEngine;
using UnityEngine.UI;

public class ShopPanel : BasePanel
{
    public ShopSystem shopSystem;
    public Text titleText;
    public Button refreshButton;
    public Button continueButton;
    public Button closeButton;
    public List<ShopItemSlot> itemSlots = new List<ShopItemSlot>();
    public List<WeaponSlotView> weaponSlots = new List<WeaponSlotView>();
    public Text goldText;
    public Image goldIcon;

    private ShopWeaponRecyclePopup _recyclePopup;

    protected override void OnOpen(object args)
    {
        ResolveShopSystem();
        shopSystem?.EnsureInitialized();
        BindButtons();
        EnsureRecyclePopup();
        BindWeaponSlotClicks();
        RunStateManager.Instance.OnStateChanged += HandleGoldChanged;
        RefreshTitle();
        RefreshGold();
        RefreshWeaponSlots();
        RefreshItems();
    }

    protected override void OnClose()
    {
        UnbindButtons();
        if (_recyclePopup != null)
        {
            _recyclePopup.Hide();
        }
        RunStateManager.Instance.OnStateChanged -= HandleGoldChanged;
    }

    protected override void OnRefresh(object args)
    {
        RefreshTitle();
        RefreshGold();
        RefreshWeaponSlots();
        RefreshItems();
    }

    public void BuyItem(ShopItemSlot slot, IShopPurchasable item)
    {
        if (shopSystem == null || item == null) return;

        int goldBeforePurchase = RunStateManager.Instance.Gold;
        shopSystem.PurchaseItem(item);
        if (RunStateManager.Instance.Gold < goldBeforePurchase || item.Price <= 0)
        {
            slot.SetPurchased();
            RefreshWeaponSlots();
        }
    }

    private void RefreshItems()
    {
        if (shopSystem == null)
        {
            Debug.LogError("[ShopPanel] 找不到 ShopSystem，无法刷新商店。");
            return;
        }

        if (!shopSystem.EnsureInitialized()) return;

        List<IShopPurchasable> items = shopSystem.RollItems(itemSlots.Count);
        int currentGold = RunStateManager.Instance.Gold;
        for (int i = 0; i < itemSlots.Count; i++)
        {
            IShopPurchasable item = i < items.Count ? items[i] : null;
            itemSlots[i].Bind(this, item, currentGold);
        }
    }

    private void RefreshTitle()
    {
        RunState state = RunStateManager.Instance.State;
        if (titleText != null)
        {
            titleText.text = $"商店  关卡 {state.currentLevel} - 波次 {state.currentWave}";
        }
    }

    private void RefreshGold()
    {
        int gold = RunStateManager.Instance.Gold;
        if (goldText != null)
        {
            goldText.text = gold.ToString();
        }

        if (goldIcon != null)
        {
            if (goldIcon.sprite == null)
            {
                Sprite sprite = Resources.Load<Sprite>("UI/Panels/ShopPanel/Textures/harvesting_icon");
                if (sprite != null)
                {
                    goldIcon.sprite = sprite;
                    goldIcon.color = Color.white;
                    goldIcon.enabled = true;
                }
            }
        }

        RefreshPriceColors();
    }

    private void RefreshPriceColors()
    {
        int currentGold = RunStateManager.Instance.Gold;
        foreach (var slot in itemSlots)
        {
            if (slot.isActiveAndEnabled)
            {
                slot.UpdatePriceColor(currentGold);
            }
        }
    }

    private void HandleGoldChanged(RunState state)
    {
        RefreshGold();
    }

    private void RefreshWeaponSlots()
    {
        var owned = shopSystem != null ? shopSystem.OwnedWeapons : null;
        for (int i = 0; i < weaponSlots.Count; i++)
        {
            if (owned != null && i < owned.Count)
            {
                WeaponConfigData weaponData = WeaponDataController.Instance.GetWeaponData(owned[i].id);
                weaponSlots[i].SetWeapon(weaponData, owned[i].grade, i + 1);
            }
            else
            {
                weaponSlots[i].SetEmpty(i + 1);
            }
        }
    }

    private void BindWeaponSlotClicks()
    {
        for (int i = 0; i < weaponSlots.Count; i++)
        {
            if (weaponSlots[i] != null)
            {
                weaponSlots[i].BindClick(HandleWeaponSlotClicked);
            }
        }
    }

    private void HandleWeaponSlotClicked(int slotIndex, OwnedWeapon ownedWeapon)
    {
        if (shopSystem == null)
        {
            return;
        }

        WeaponConfigData weaponData = WeaponDataController.Instance.GetWeaponData(ownedWeapon.id);
        if (weaponData == null)
        {
            return;
        }

        int refundGold = shopSystem.GetWeaponRecyclePrice(ownedWeapon.id, ownedWeapon.grade);
        EnsureRecyclePopup();
        _recyclePopup.Show(weaponData.name, ownedWeapon.grade, refundGold, () => ConfirmSellWeapon(slotIndex));
    }

    private void ConfirmSellWeapon(int slotIndex)
    {
        if (shopSystem == null)
        {
            return;
        }

        if (shopSystem.SellWeaponAt(slotIndex, out int refundGold))
        {
            RefreshGold();
            RefreshWeaponSlots();
            Debug.Log($"<color=yellow>[武器回收] 获得金币 {refundGold}</color>");
        }
    }

    private void EnsureRecyclePopup()
    {
        if (_recyclePopup != null)
        {
            return;
        }

        _recyclePopup = ShopWeaponRecyclePopup.Create(transform);
    }

    private void ResolveShopSystem()
    {
        if (shopSystem != null) return;

        shopSystem = FindObjectOfType<ShopSystem>();
        if (shopSystem != null) return;

        GameObject shopObject = new GameObject("ShopSystem");
        WaveDataController waveDataController = shopObject.AddComponent<WaveDataController>();
        shopSystem = shopObject.AddComponent<ShopSystem>();
        shopSystem.waveDataController = waveDataController;

        if (FindObjectOfType<PlayerStatus>() == null)
        {
            GameObject playerStatusObject = new GameObject("MockPlayerStatus");
            playerStatusObject.AddComponent<PlayerStatus>();
        }
    }

    private void BindButtons()
    {
        UnbindButtons();
        UIButtonBinder.Bind(refreshButton, RefreshItems);
        UIButtonBinder.Bind(continueButton, CloseShop);
        UIButtonBinder.Bind(closeButton, CloseShop);
    }

    private void UnbindButtons()
    {
        if (refreshButton != null) refreshButton.onClick.RemoveListener(RefreshItems);
        if (continueButton != null) continueButton.onClick.RemoveListener(CloseShop);
        if (closeButton != null) closeButton.onClick.RemoveListener(CloseShop);
    }

    private void CloseShop()
    {
        UIManager.Instance.ClosePanel<ShopPanel>();
        if (BattleStateManager.Instance != null)
        {
            BattleStateManager.Instance.CloseShop();
        }
    }

}

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

    protected override void OnOpen(object args)
    {
        ResolveShopSystem();
        shopSystem?.EnsureInitialized();
        BindButtons();
        RunStateManager.Instance.OnStateChanged += HandleGoldChanged;
        RefreshTitle();
        RefreshGold();
        RefreshWeaponSlots();
        RefreshItems();
    }

    protected override void OnClose()
    {
        UnbindButtons();
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
        var ownedIds = shopSystem != null ? shopSystem.OwnedWeaponIds : null;
        for (int i = 0; i < weaponSlots.Count; i++)
        {
            if (ownedIds != null && i < ownedIds.Count)
            {
                WeaponShopData weaponData = WeaponDataController.Instance.GetWeaponData(ownedIds[i]);
                weaponSlots[i].SetWeapon(weaponData, i + 1);
            }
            else
            {
                weaponSlots[i].SetEmpty(i + 1);
            }
        }
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
        if (refreshButton != null) refreshButton.onClick.AddListener(RefreshItems);
        if (continueButton != null) continueButton.onClick.AddListener(CloseShop);
        if (closeButton != null) closeButton.onClick.AddListener(CloseShop);
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

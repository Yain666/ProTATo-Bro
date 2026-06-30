using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponSelectionPanel : BasePanel
{
    private const string SelectionSlotPrefabPath = "UI/Panels/Common/SelectionSlot";

    [Header("Top")]
    public Button backButton;
    public Text titleText;

    [Header("Character Summary")]
    public Image selectedCharacterIcon;
    public Text selectedCharacterNameText;
    public Text selectedCharacterJobText;
    public Text selectedCharacterStatsText;

    [Header("Weapon Detail")]
    public Image selectedWeaponIcon;
    public Text selectedWeaponNameText;
    public Text selectedWeaponTypeText;
    public Text selectedWeaponGradeText;
    public Text selectedWeaponDescriptionText;

    [Header("Actions")]
    public Button confirmButton;

    [Header("Grid")]
    public RectTransform gridContent;
    public GridLayoutGroup gridLayoutGroup;

    private readonly List<SelectionSlot> _weaponSlots = new List<SelectionSlot>();
    private readonly List<WeaponConfigData> _availableWeapons = new List<WeaponConfigData>();
    private CharacterData _selectedCharacter;
    private WeaponConfigData _selectedWeapon;

    protected override void OnOpen(object args)
    {
        CharacterDataController.Instance.Init();
        BasicPropertiesDataController.Instance.Init();
        WeaponDataController.Initialize();
        EnsureReferences();
        BindButtons();
        ResolveCharacter(args);
        BuildWeaponGrid();
        ClearWeaponSelection();
        RefreshCharacterSummary();
    }

    protected override void OnClose()
    {
        UnbindButtons();
        ClearWeaponButtons();
    }

    protected override void OnRefresh(object args)
    {
        ResolveCharacter(args);
        BuildWeaponGrid();
        ClearWeaponSelection();
        RefreshCharacterSummary();
    }

    private void EnsureReferences()
    {
        if (backButton == null) backButton = FindButton("TopBar/BackButton");
        if (confirmButton == null) confirmButton = FindButton("BottomBar/ConfirmButton");
        if (titleText == null) titleText = FindText("TopBar/Title");

        if (selectedCharacterIcon == null) selectedCharacterIcon = FindImage("Content/TopContent/CharacterSummaryPanel/Header/Icon");
        if (selectedCharacterNameText == null) selectedCharacterNameText = FindText("Content/TopContent/CharacterSummaryPanel/Header/Texts/Name");
        if (selectedCharacterJobText == null) selectedCharacterJobText = FindText("Content/TopContent/CharacterSummaryPanel/Header/Texts/Job");
        if (selectedCharacterStatsText == null) selectedCharacterStatsText = FindText("Content/TopContent/CharacterSummaryPanel/StatsScrollView/Viewport/Content/Stats");

        if (selectedWeaponIcon == null) selectedWeaponIcon = FindImage("Content/TopContent/WeaponDetailPanel/Header/Icon");
        if (selectedWeaponNameText == null) selectedWeaponNameText = FindText("Content/TopContent/WeaponDetailPanel/Header/Texts/Name");
        if (selectedWeaponTypeText == null) selectedWeaponTypeText = FindText("Content/TopContent/WeaponDetailPanel/Header/Texts/Type");
        if (selectedWeaponGradeText == null) selectedWeaponGradeText = FindText("Content/TopContent/WeaponDetailPanel/Header/Texts/Grade");
        if (selectedWeaponDescriptionText == null) selectedWeaponDescriptionText = FindText("Content/TopContent/WeaponDetailPanel/Description");

        if (gridContent == null)
        {
            Transform content = transform.Find("Content/BottomContent/WeaponGridPanel/GridScrollView/Viewport/Content");
            gridContent = content as RectTransform;
        }

        if (gridLayoutGroup == null && gridContent != null)
        {
            gridLayoutGroup = gridContent.GetComponent<GridLayoutGroup>();
        }

        if (titleText != null && string.IsNullOrEmpty(titleText.text))
        {
            titleText.text = "武器选择";
        }
    }

    private void ResolveCharacter(object args)
    {
        if (args is CharacterData characterArg)
        {
            _selectedCharacter = characterArg;
            RunStartContext.Instance.SetCharacter(characterArg.id);
            return;
        }

        int selectedCharacterId = RunStartContext.Instance.SelectedCharacterId;
        if (selectedCharacterId > 0)
        {
            _selectedCharacter = CharacterDataController.Instance.GetCharacterById(selectedCharacterId);
            return;
        }

        List<CharacterData> allCharacters = CharacterDataController.Instance.GetAllData();
        _selectedCharacter = allCharacters.Count > 0 ? allCharacters[0] : null;
        if (_selectedCharacter != null)
        {
            RunStartContext.Instance.SetCharacter(_selectedCharacter.id);
        }
    }

    private void BindButtons()
    {
        UnbindButtons();
        UIButtonBinder.Bind(backButton, BackToCharacterSelect);
        UIButtonBinder.Bind(confirmButton, HandleConfirm);
    }

    private void UnbindButtons()
    {
        if (backButton != null) backButton.onClick.RemoveAllListeners();
        if (confirmButton != null) confirmButton.onClick.RemoveAllListeners();
    }


    private void BuildWeaponGrid()
    {
        if (gridContent == null) return;

        ClearWeaponButtons();
        _availableWeapons.Clear();
        GameObject slotPrefab = Resources.Load<GameObject>(SelectionSlotPrefabPath);
        if (slotPrefab == null) return;

        if (_selectedCharacter == null) return;

        IReadOnlyList<int> weaponIds = CharacterStartWeaponProvider.GetStartingWeaponIds(_selectedCharacter);
        for (int i = 0; i < weaponIds.Count; i++)
        {
            WeaponConfigData weapon = WeaponDataController.Instance.GetWeaponData(weaponIds[i]);
            if (weapon == null) continue;

            _availableWeapons.Add(weapon);
            SelectionSlot slot = CreateWeaponItem(gridContent, slotPrefab, weapon);
            if (slot != null) _weaponSlots.Add(slot);
        }
    }

    private void ClearWeaponButtons()
    {
        for (int i = gridContent != null ? gridContent.childCount - 1 : -1; i >= 0; i--)
        {
            Destroy(gridContent.GetChild(i).gameObject);
        }

        _weaponSlots.Clear();
    }

    private SelectionSlot CreateWeaponItem(Transform parent, GameObject slotPrefab, WeaponConfigData weapon)
    {
        GameObject slotObject = Object.Instantiate(slotPrefab, parent);
        slotObject.name = $"Weapon_{weapon.id}";

        RectTransform rect = slotObject.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.sizeDelta = gridLayoutGroup != null ? gridLayoutGroup.cellSize : new Vector2(96f, 96f);
        }

        SelectionSlot slot = slotObject.GetComponent<SelectionSlot>();
        if (slot == null) return null;

        slot.Bind(LoadWeaponSprite(weapon.icon_path), weapon.name, () => SelectWeapon(weapon));
        return slot;
    }

    private void ClearWeaponSelection()
    {
        _selectedWeapon = null;
        RunStartContext.Instance.ResetWeaponSelection();
        RunStartContext.Instance.ResetDifficultySelection();
        RefreshWeaponDetail();
    }

    private void SelectWeapon(WeaponConfigData weapon)
    {
        _selectedWeapon = weapon;
        if (_selectedWeapon != null)
        {
            RunStartContext.Instance.SetWeapon(_selectedWeapon.id);
        }

        RefreshWeaponDetail();
    }

    private void RefreshCharacterSummary()
    {
        if (_selectedCharacter == null)
        {
            SetText(selectedCharacterNameText, "请选择角色");
            SetText(selectedCharacterJobText, string.Empty);
            SetText(selectedCharacterStatsText, "当前没有可用角色数据。");
            if (selectedCharacterIcon != null)
            {
                selectedCharacterIcon.sprite = null;
                selectedCharacterIcon.enabled = false;
            }
            return;
        }

        SetText(selectedCharacterNameText, _selectedCharacter.characterName);
        SetText(selectedCharacterJobText, _selectedCharacter.job);
        SetText(selectedCharacterStatsText, BuildCharacterSummary(_selectedCharacter));
        if (selectedCharacterIcon != null)
        {
            selectedCharacterIcon.enabled = true;
            selectedCharacterIcon.sprite = LoadCharacterSprite(_selectedCharacter.characterImage);
            selectedCharacterIcon.color = Color.white;
            selectedCharacterIcon.preserveAspect = true;
        }
    }

    private void RefreshWeaponDetail()
    {
        if (_selectedWeapon == null)
        {
            SetText(selectedWeaponNameText, "请选择武器");
            SetText(selectedWeaponTypeText, string.Empty);
            SetText(selectedWeaponGradeText, string.Empty);
            SetText(selectedWeaponDescriptionText, "当前没有可用武器数据。");
            if (selectedWeaponIcon != null)
            {
                selectedWeaponIcon.sprite = null;
                selectedWeaponIcon.enabled = false;
            }
            if (confirmButton != null) confirmButton.interactable = false;
            UpdateWeaponButtonStates();
            return;
        }

        SetText(selectedWeaponNameText, _selectedWeapon.name);
        SetText(selectedWeaponTypeText, BuildWeaponTypeLabel(_selectedWeapon));
        SetText(selectedWeaponGradeText, BuildWeaponGradeLabel(_selectedWeapon));
        SetText(selectedWeaponDescriptionText, BuildWeaponDescription(_selectedWeapon));
        if (selectedWeaponIcon != null)
        {
            selectedWeaponIcon.enabled = true;
            selectedWeaponIcon.sprite = LoadWeaponSprite(_selectedWeapon.icon_path);
            selectedWeaponIcon.color = Color.white;
            selectedWeaponIcon.preserveAspect = true;
        }

        if (confirmButton != null) confirmButton.interactable = true;
        UpdateWeaponButtonStates();
    }

    private void UpdateWeaponButtonStates()
    {
        for (int i = 0; i < _weaponSlots.Count && i < _availableWeapons.Count; i++)
        {
            SelectionSlot slot = _weaponSlots[i];
            if (slot == null) continue;

            WeaponConfigData data = _availableWeapons[i];
            bool selected = _selectedWeapon != null && data.id == _selectedWeapon.id;
            slot.SetSelected(selected);
        }
    }

    private string BuildCharacterSummary(CharacterData character)
    {
        string modifiers = BuildModifierLines(character);
        if (string.IsNullOrEmpty(modifiers))
        {
            modifiers = "暂无属性描述。";
        }

        return $"职业：{character.job}\n\n{modifiers}";
    }

    private string BuildModifierLines(CharacterData character)
    {
        if (character.attrIds == null || character.attrData == null) return string.Empty;

        int count = Mathf.Min(character.attrIds.Length, character.attrData.Length, 6);
        List<string> lines = new List<string>();
        for (int i = 0; i < count; i++)
        {
            int propertyId = character.attrIds[i];
            int value = character.attrData[i];
            BasicProperties basicProperty = BasicPropertiesDataController.Instance.GetDataByKey(propertyId);
            string displayName = basicProperty != null && !string.IsNullOrEmpty(basicProperty.Description)
                ? basicProperty.Description
                : ((PropertyType)propertyId).ToString();

            string formattedValue = basicProperty != null && basicProperty.ValueType == ValueType.Percentage
                ? $"{value:+0;-0;0}%"
                : $"{value:+0;-0;0}";

            lines.Add($"{displayName} {formattedValue}");
        }

        return string.Join("\n", lines);
    }

    private string BuildWeaponTypeLabel(WeaponConfigData weapon)
    {
        return weapon.weapon_type == "Ranged" ? "远程武器" : "近战武器";
    }

    private string BuildWeaponGradeLabel(WeaponConfigData weapon)
    {
        return $"品阶：{BuildGradeName(weapon.grade)}";
    }

    private string BuildWeaponDescription(WeaponConfigData weapon)
    {
        string attackSpeedText = weapon.attack_speed > 0f ? $"{1f / weapon.attack_speed:0.##} 次/秒" : "--";
        string critChanceText = $"{weapon.crit_chance * 100f:0.#}%";
        return $"类型：{BuildWeaponTypeLabel(weapon)}\n" +
               $"伤害：{weapon.damage:0.#}\n" +
               $"攻速：{attackSpeedText}\n" +
               $"范围：{weapon.range:0.#}\n" +
               $"暴击率：{critChanceText}\n" +
               $"击退：{weapon.knockback:0.#}";
    }

    private string BuildGradeName(int grade)
    {
        switch (grade)
        {
            case 1: return "普通";
            case 2: return "稀有";
            case 3: return "史诗";
            case 4: return "神话";
            default: return grade.ToString();
        }
    }

    private void BackToCharacterSelect()
    {
        if (UIManager.Instance == null) return;

        UIManager.Instance.ClosePanel<WeaponSelectionPanel>();
        UIManager.Instance.OpenPanel<CharacterSelectPanel>("UI/Panels/CharacterSelect", UILayer.Panel);
    }

    private void HandleConfirm()
    {
        if (_selectedCharacter == null || _selectedWeapon == null)
        {
            Debug.LogWarning("[WeaponSelectionPanel] 缺少角色或武器选择，无法确认。");
            return;
        }

        RunStartContext.Instance.SetCharacter(_selectedCharacter.id);
        RunStartContext.Instance.SetWeapon(_selectedWeapon.id);
        //Debug.Log($"[WeaponSelectionPanel] 已确认角色 {_selectedCharacter.characterName} 的起始武器：{_selectedWeapon.name}");

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ClosePanel<WeaponSelectionPanel>();
            UIManager.Instance.OpenPanel<DifficultySelectionPanel>("UI/Panels/DifficultySelection", UILayer.Panel);
        }
        else
        {
            Debug.LogWarning("[WeaponSelectionPanel] 找不到 UIManager，无法打开难度选择页。");
        }
    }

    private Button FindButton(string path)
    {
        Transform child = transform.Find(path);
        return child != null ? child.GetComponent<Button>() : null;
    }

    private Text FindText(string path)
    {
        Transform child = transform.Find(path);
        return child != null ? child.GetComponent<Text>() : null;
    }

    private Image FindImage(string path)
    {
        Transform child = transform.Find(path);
        return child != null ? child.GetComponent<Image>() : null;
    }

    private void SetText(Text target, string value)
    {
        if (target != null) target.text = value;
    }

    private static Sprite LoadCharacterSprite(string characterImage)
    {
        if (string.IsNullOrWhiteSpace(characterImage)) return null;

        Sprite sprite = Resources.Load<Sprite>($"Icon/{characterImage}");
        if (sprite != null) return sprite;

        sprite = Resources.Load<Sprite>($"Icon/{characterImage.ToLowerInvariant()}");
        if (sprite != null) return sprite;

        sprite = Resources.Load<Sprite>($"UI/Panels/CharacterSelection/Characters/{characterImage}");
        if (sprite != null) return sprite;

        return Resources.Load<Sprite>(characterImage);
    }

    private static Sprite LoadWeaponSprite(string iconPath)
    {
        if (string.IsNullOrWhiteSpace(iconPath)) return null;
        return Resources.Load<Sprite>(iconPath);
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DifficultySelectionPanel : BasePanel
{
    private const string SelectionSlotPrefabPath = "UI/Panels/Common/SelectionSlot";

    [System.Serializable]
    public class DifficultyIconEntry
    {
        public int level;
        public Sprite sprite;
    }

    [Header("Top")]
    public Button backButton;
    public Text titleText;

    [Header("Character Summary")]
    public Image selectedCharacterIcon;
    public Text selectedCharacterNameText;
    public Text selectedCharacterJobText;
    public Text selectedCharacterStatsText;

    [Header("Weapon Summary")]
    public Image selectedWeaponIcon;
    public Text selectedWeaponNameText;
    public Text selectedWeaponTypeText;
    public Text selectedWeaponGradeText;
    public Text selectedWeaponDescriptionText;

    [Header("Difficulty Detail")]
    public Text selectedDifficultyNameText;
    public Text selectedDifficultyDescriptionText;
    public Toggle endlessToggle;
    public Toggle banSystemToggle;

    [Header("Actions")]
    public Button confirmButton;

    [Header("Grid")]
    public RectTransform gridContent;
    public GridLayoutGroup gridLayoutGroup;

    [Header("Difficulty Icons")]
    public DifficultyIconEntry[] difficultyIcons;

    private readonly List<SelectionSlot> _difficultySlots = new List<SelectionSlot>();
    private readonly List<int> _availableLevels = new List<int>();

    private CharacterData _selectedCharacter;
    private WeaponConfigData _selectedWeapon;
    private int _selectedLevel;

    protected override void OnOpen(object args)
    {
        CharacterDataController.Instance.Init();
        BasicPropertiesDataController.Instance.Init();
        WeaponDataController.Initialize();
        EnsureReferences();
        BindControls();
        ResolveContext();
        BuildDifficultyGrid();
        RestoreSelection();
        RefreshView();
    }

    protected override void OnClose()
    {
        UnbindControls();
        ClearDifficultyButtons();
    }

    protected override void OnRefresh(object args)
    {
        ResolveContext();
        BuildDifficultyGrid();
        RestoreSelection();
        RefreshView();
    }

    private void EnsureReferences()
    {
        if (backButton == null) backButton = FindButton("TopBar/BackButton");
        if (confirmButton == null) confirmButton = FindButton("BottomBar/ConfirmButton");
        if (titleText == null) titleText = FindText("TopBar/Title");

        if (selectedCharacterIcon == null) selectedCharacterIcon = FindImage("Content/TopContent/CharacterSummaryPanel/Header/Icon");
        if (selectedCharacterNameText == null) selectedCharacterNameText = FindText("Content/TopContent/CharacterSummaryPanel/Header/Texts/Name");
        if (selectedCharacterJobText == null) selectedCharacterJobText = FindText("Content/TopContent/CharacterSummaryPanel/Header/Texts/Job");
        if (selectedCharacterStatsText == null) selectedCharacterStatsText = FindText("Content/TopContent/CharacterSummaryPanel/Stats");

        if (selectedWeaponIcon == null) selectedWeaponIcon = FindImage("Content/TopContent/WeaponSummaryPanel/Header/Icon");
        if (selectedWeaponNameText == null) selectedWeaponNameText = FindText("Content/TopContent/WeaponSummaryPanel/Header/Texts/Name");
        if (selectedWeaponTypeText == null) selectedWeaponTypeText = FindText("Content/TopContent/WeaponSummaryPanel/Header/Texts/Type");
        if (selectedWeaponGradeText == null) selectedWeaponGradeText = FindText("Content/TopContent/WeaponSummaryPanel/Header/Texts/Grade");
        if (selectedWeaponDescriptionText == null) selectedWeaponDescriptionText = FindText("Content/TopContent/WeaponSummaryPanel/Description");

        if (selectedDifficultyNameText == null) selectedDifficultyNameText = FindText("Content/BottomContent/DifficultyDetailPanel/Name");
        if (selectedDifficultyDescriptionText == null) selectedDifficultyDescriptionText = FindText("Content/BottomContent/DifficultyDetailPanel/Description");
        if (endlessToggle == null) endlessToggle = FindToggle("Content/BottomContent/DifficultyDetailPanel/Modes/EndlessToggle/Toggle");
        if (banSystemToggle == null) banSystemToggle = FindToggle("Content/BottomContent/DifficultyDetailPanel/Modes/BanSystemToggle/Toggle");

        if (gridContent == null)
        {
            Transform content = transform.Find("Content/BottomContent/DifficultyGridPanel/GridScrollView/Viewport/Content");
            gridContent = content as RectTransform;
        }

        if (gridLayoutGroup == null && gridContent != null)
        {
            gridLayoutGroup = gridContent.GetComponent<GridLayoutGroup>();
        }

        if (titleText != null && string.IsNullOrEmpty(titleText.text))
        {
            titleText.text = "难度选择";
        }
    }

    private void BindControls()
    {
        UnbindControls();
        UIButtonBinder.Bind(backButton, BackToWeaponSelection);
        UIButtonBinder.Bind(confirmButton, HandleConfirm);
        if (endlessToggle != null) endlessToggle.onValueChanged.AddListener(HandleEndlessChanged);
        if (banSystemToggle != null) banSystemToggle.onValueChanged.AddListener(HandleBanSystemChanged);
    }

    private void UnbindControls()
    {
        if (backButton != null) backButton.onClick.RemoveAllListeners();
        if (confirmButton != null) confirmButton.onClick.RemoveAllListeners();
        if (endlessToggle != null) endlessToggle.onValueChanged.RemoveAllListeners();
        if (banSystemToggle != null) banSystemToggle.onValueChanged.RemoveAllListeners();
    }


    private void ResolveContext()
    {
        RunStartContext context = RunStartContext.Instance;
        if (context.SelectedCharacterId > 0)
        {
            _selectedCharacter = CharacterDataController.Instance.GetCharacterById(context.SelectedCharacterId);
        }

        if (context.SelectedWeaponId > 0)
        {
            _selectedWeapon = WeaponDataController.Instance.GetWeaponData(context.SelectedWeaponId);
        }
    }

    private void BuildDifficultyGrid()
    {
        if (gridContent == null) return;

        ClearDifficultyButtons();
        _availableLevels.Clear();

        GameObject slotPrefab = Resources.Load<GameObject>(SelectionSlotPrefabPath);
        if (slotPrefab == null) return;

        List<int> availableLevels = DifficultySelectionService.GetAvailableLevels();
        for (int i = 0; i < availableLevels.Count; i++)
        {
            int level = availableLevels[i];
            _availableLevels.Add(level);
            SelectionSlot slot = CreateDifficultyItem(gridContent, slotPrefab, level);
            if (slot != null) _difficultySlots.Add(slot);
        }
    }

    private void ClearDifficultyButtons()
    {
        for (int i = gridContent != null ? gridContent.childCount - 1 : -1; i >= 0; i--)
        {
            Destroy(gridContent.GetChild(i).gameObject);
        }

        _difficultySlots.Clear();
    }

    private SelectionSlot CreateDifficultyItem(Transform parent, GameObject slotPrefab, int level)
    {
        GameObject slotObject = Object.Instantiate(slotPrefab, parent);
        slotObject.name = $"Difficulty_{level}";

        RectTransform rect = slotObject.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.sizeDelta = gridLayoutGroup != null ? gridLayoutGroup.cellSize : new Vector2(96f, 96f);
        }

        SelectionSlot slot = slotObject.GetComponent<SelectionSlot>();
        if (slot == null) return null;

        slot.Bind(LoadDifficultySprite(level), BuildDifficultyLabel(level), () => SelectDifficulty(level));
        return slot;
    }

    private void RestoreSelection()
    {
        int contextLevel = RunStartContext.Instance.SelectedLevel;
        if (_availableLevels.Count == 0)
        {
            _selectedLevel = 0;
        }
        else if (_availableLevels.Contains(contextLevel))
        {
            _selectedLevel = contextLevel;
        }
        else
        {
            _selectedLevel = 0;
            RunStartContext.Instance.SetLevel(0);
        }

        if (endlessToggle != null) endlessToggle.isOn = RunStartContext.Instance.IsEndless;
        if (banSystemToggle != null) banSystemToggle.isOn = RunStartContext.Instance.IsBanSystemEnabled;
    }

    private void SelectDifficulty(int level)
    {
        _selectedLevel = level;
        RunStartContext.Instance.SetLevel(level);
        RefreshDifficultySelection();
    }

    private void RefreshView()
    {
        RefreshCharacterSummary();
        RefreshWeaponSummary();
        RefreshDifficultySelection();
    }

    private void RefreshCharacterSummary()
    {
        if (_selectedCharacter == null)
        {
            SetText(selectedCharacterNameText, "未选择角色");
            SetText(selectedCharacterJobText, string.Empty);
            SetText(selectedCharacterStatsText, "请先返回上一步选择角色。\n");
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

    private void RefreshWeaponSummary()
    {
        if (_selectedWeapon == null)
        {
            SetText(selectedWeaponNameText, "未选择武器");
            SetText(selectedWeaponTypeText, string.Empty);
            SetText(selectedWeaponGradeText, string.Empty);
            SetText(selectedWeaponDescriptionText, "请先返回上一步选择武器。");
            if (selectedWeaponIcon != null)
            {
                selectedWeaponIcon.sprite = null;
                selectedWeaponIcon.enabled = false;
            }
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
    }

    private void RefreshDifficultySelection()
    {
        if (_selectedLevel <= 0)
        {
            SetText(selectedDifficultyNameText, "请选择难度");
            SetText(selectedDifficultyDescriptionText, BuildDifficultyDescription(0));
            if (confirmButton != null) confirmButton.interactable = false;
        }
        else
        {
            SetText(selectedDifficultyNameText, BuildDifficultyLabel(_selectedLevel));
            SetText(selectedDifficultyDescriptionText, BuildDifficultyDescription(_selectedLevel));
            if (confirmButton != null) confirmButton.interactable = _selectedCharacter != null && _selectedWeapon != null;
        }

        UpdateDifficultyButtonStates();
    }

    private void UpdateDifficultyButtonStates()
    {
        for (int i = 0; i < _difficultySlots.Count && i < _availableLevels.Count; i++)
        {
            SelectionSlot slot = _difficultySlots[i];
            if (slot == null) continue;

            slot.SetSelected(_availableLevels[i] == _selectedLevel);
        }
    }

    private void HandleEndlessChanged(bool isOn)
    {
        RunStartContext.Instance.SetModes(isOn, banSystemToggle != null && banSystemToggle.isOn);
    }

    private void HandleBanSystemChanged(bool isOn)
    {
        RunStartContext.Instance.SetModes(endlessToggle != null && endlessToggle.isOn, isOn);
    }

    private void BackToWeaponSelection()
    {
        if (UIManager.Instance == null) return;

        UIManager.Instance.ClosePanel<DifficultySelectionPanel>();
        UIManager.Instance.OpenPanel<WeaponSelectionPanel>("UI/Panels/WeaponSelection", UILayer.Panel, _selectedCharacter);
    }

    private void HandleConfirm()
    {
        if (_selectedCharacter == null || _selectedWeapon == null || _selectedLevel <= 0)
        {
            Debug.LogWarning("[DifficultySelectionPanel] 角色、武器或难度选择不完整，无法开始游戏。");
            return;
        }

        RunStartContext.Instance.SetCharacter(_selectedCharacter.id);
        RunStartContext.Instance.SetWeapon(_selectedWeapon.id);
        RunStartContext.Instance.SetLevel(_selectedLevel);
        RunStartContext.Instance.SetModes(endlessToggle != null && endlessToggle.isOn, banSystemToggle != null && banSystemToggle.isOn);
        //Debug.Log($"[DifficultySelectionPanel] 已确认开局：角色 {_selectedCharacter.characterName}，武器 {_selectedWeapon.name}，难度 {_selectedLevel}");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame(_selectedLevel);
        }
        else
        {
            Debug.LogWarning("[DifficultySelectionPanel] 找不到 GameManager，无法进入游戏。");
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

    private string BuildDifficultyLabel(int level)
    {
        return $"难度 {level}";
    }

    private string BuildDifficultyDescription(int level)
    {
        if (level <= 0)
        {
            return "请选择一个当前已配置的难度，再开始本局。\n\n无尽模式与禁用系统开关会一并写入开局上下文。";
        }

        return $"当前选择：难度 {level}\n\n本项目现阶段会根据已配置的关卡数据自动开放可选难度，避免进入未配置关卡。";
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

    private Toggle FindToggle(string path)
    {
        Transform child = transform.Find(path);
        return child != null ? child.GetComponent<Toggle>() : null;
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

        return Resources.Load<Sprite>(characterImage);
    }

    private static Sprite LoadWeaponSprite(string iconPath)
    {
        if (string.IsNullOrWhiteSpace(iconPath)) return null;

        Sprite sprite = Resources.Load<Sprite>(iconPath);
        if (sprite != null) return sprite;

        string fileName = System.IO.Path.GetFileNameWithoutExtension(iconPath);
        if (string.IsNullOrEmpty(fileName)) return null;

        sprite = Resources.Load<Sprite>($"Icon/{fileName}");
        if (sprite != null) return sprite;

        return Resources.Load<Sprite>($"WeaponSelection/weapon_icons/{fileName}");
    }

    private Sprite LoadDifficultySprite(int level)
    {
        if (difficultyIcons == null) return null;

        for (int i = 0; i < difficultyIcons.Length; i++)
        {
            DifficultyIconEntry entry = difficultyIcons[i];
            if (entry != null && entry.level == level)
            {
                return entry.sprite;
            }
        }

        return null;
    }
}

public static class DifficultySelectionService
{
    public static List<int> GetAvailableLevels()
    {
        List<int> levels = new List<int>();
        WaveShopConfigDataController controller = new WaveShopConfigDataController();
        controller.LoadData("Config/DataJson/WaveShopConfig");

        List<WaveShopConfigData> allRows = controller.GetAllData();
        for (int i = 0; i < allRows.Count; i++)
        {
            int level = allRows[i].level;
            if (level <= 0 || levels.Contains(level)) continue;
            levels.Add(level);
        }

        levels.Sort();
        return levels;
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectPanel : BasePanel
{
    private const string SelectionSlotPrefabPath = "UI/Panels/Common/SelectionSlot";

    [Header("Top")]
    public Button backButton;
    public Text titleText;

    [Header("Detail")]
    public Image selectedCharacterIcon;
    public Text selectedCharacterNameText;
    public Text selectedCharacterJobText;
    public Text selectedCharacterDescriptionText;

    [Header("Records")]
    public Text maxDifficultyText;
    public Text maxEndlessText;

    [Header("Actions")]
    public Button startButton;

    [Header("Grid")]
    public RectTransform gridContent;
    public GridLayoutGroup gridLayoutGroup;

    private readonly List<SelectionSlot> _characterSlots = new List<SelectionSlot>();
    private readonly List<CharacterData> _characterDataList = new List<CharacterData>();
    private CharacterData _selectedCharacter;

    protected override void OnOpen(object args)
    {
        CharacterDataController.Instance.Init();
        BasicPropertiesDataController.Instance.Init();
        EnsureReferences();
        BindButtons();
        BuildCharacterGrid();
        ClearSelection();
    }

    protected override void OnClose()
    {
        UnbindButtons();
        ClearCharacterButtons();
    }

    protected override void OnRefresh(object args)
    {
        RefreshSelectionView();
    }

    private void EnsureReferences()
    {
        if (backButton == null) backButton = FindButton("TopBar/BackButton");
        if (startButton == null) startButton = FindButton("BottomBar/StartButton");
        if (titleText == null) titleText = FindText("TopBar/Title");

        if (selectedCharacterIcon == null) selectedCharacterIcon = FindImage("Content/CharacterDetailPanel/Header/Icon");
        if (selectedCharacterNameText == null) selectedCharacterNameText = FindText("Content/CharacterDetailPanel/Header/Texts/Name");
        if (selectedCharacterJobText == null) selectedCharacterJobText = FindText("Content/CharacterDetailPanel/Header/Texts/Job");
        if (selectedCharacterDescriptionText == null) selectedCharacterDescriptionText = FindText("Content/CharacterDetailPanel/Description");

        if (maxDifficultyText == null) maxDifficultyText = FindText("Content/SideColumn/InfoPanel/Body/MaxDifficulty");
        if (maxEndlessText == null) maxEndlessText = FindText("Content/SideColumn/InfoPanel/Body/MaxEndless");

        if (gridContent == null)
        {
            Transform content = transform.Find("Content/CharacterGridPanel/GridScrollView/Viewport/Content");
            gridContent = content as RectTransform;
        }

        if (gridLayoutGroup == null && gridContent != null)
        {
            gridLayoutGroup = gridContent.GetComponent<GridLayoutGroup>();
        }

        if (titleText != null && string.IsNullOrEmpty(titleText.text))
        {
            titleText.text = "CHARACTER_SELECTION";
        }
    }

    private void BindButtons()
    {
        UnbindButtons();
        UIButtonBinder.Bind(backButton, BackToMenu);
        UIButtonBinder.Bind(startButton, HandleStartPlaceholder);
    }

    private void UnbindButtons()
    {
        if (backButton != null) backButton.onClick.RemoveAllListeners();
        if (startButton != null) startButton.onClick.RemoveAllListeners();
    }

    private void BuildCharacterGrid()
    {
        if (gridContent == null) return;

        ClearCharacterButtons();
        _characterDataList.Clear();
        GameObject slotPrefab = Resources.Load<GameObject>(SelectionSlotPrefabPath);
        if (slotPrefab == null) return;

        List<CharacterData> characters = CharacterDataController.Instance.GetAllData();
        for (int i = 0; i < characters.Count; i++)
        {
            CharacterData character = characters[i];
            _characterDataList.Add(character);
            SelectionSlot slot = CreateCharacterItem(gridContent, slotPrefab, character);
            if (slot != null) _characterSlots.Add(slot);
        }
    }

    private void ClearCharacterButtons()
    {
        for (int i = gridContent != null ? gridContent.childCount - 1 : -1; i >= 0; i--)
        {
            Destroy(gridContent.GetChild(i).gameObject);
        }

        _characterSlots.Clear();
    }

    private SelectionSlot CreateCharacterItem(Transform parent, GameObject slotPrefab, CharacterData character)
    {
        GameObject slotObject = Object.Instantiate(slotPrefab, parent);
        slotObject.name = $"Character_{character.id}";

        RectTransform rect = slotObject.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.sizeDelta = gridLayoutGroup != null ? gridLayoutGroup.cellSize : new Vector2(96f, 96f);
        }

        SelectionSlot slot = slotObject.GetComponent<SelectionSlot>();
        if (slot == null) return null;

        slot.Bind(LoadCharacterSprite(character.characterImage), character.characterName, () => SelectCharacter(character));
        return slot;
    }

    private void ClearSelection()
    {
        _selectedCharacter = null;
        RunStartContext.Instance.ClearCharacter();
        RunStartContext.Instance.ResetWeaponSelection();
        RunStartContext.Instance.ResetDifficultySelection();
        RefreshSelectionView();
    }

    private void SelectCharacter(CharacterData character)
    {
        _selectedCharacter = character;
        if (_selectedCharacter != null)
        {
            RunStartContext.Instance.SetCharacter(_selectedCharacter.id);
        }

        RefreshSelectionView();
    }

    private void RefreshSelectionView()
    {
        if (_selectedCharacter == null)
        {
            SetText(selectedCharacterNameText, "请选择角色");
            SetText(selectedCharacterJobText, string.Empty);
            SetText(selectedCharacterDescriptionText, "当前没有可用角色数据。");
            if (selectedCharacterIcon != null)
            {
                selectedCharacterIcon.sprite = null;
                selectedCharacterIcon.enabled = false;
            }
            SetText(maxDifficultyText, "最高通关难度: --");
            SetText(maxEndlessText, "最高无尽波数: --");
            if (startButton != null) startButton.interactable = false;
            UpdateCharacterButtonStates();
            return;
        }

        SetText(selectedCharacterNameText, _selectedCharacter.characterName);
        SetText(selectedCharacterJobText, _selectedCharacter.job);
        SetText(selectedCharacterDescriptionText, BuildCharacterDescription(_selectedCharacter));
        if (selectedCharacterIcon != null)
        {
            selectedCharacterIcon.enabled = true;
            selectedCharacterIcon.sprite = LoadCharacterSprite(_selectedCharacter.characterImage);
            selectedCharacterIcon.color = Color.white;
            selectedCharacterIcon.preserveAspect = true;
        }

        SetText(maxDifficultyText, "最高通关难度: --");
        SetText(maxEndlessText, "最高无尽波数: --");
        if (startButton != null) startButton.interactable = true;
        UpdateCharacterButtonStates();
    }

    private void UpdateCharacterButtonStates()
    {
        for (int i = 0; i < _characterSlots.Count && i < _characterDataList.Count; i++)
        {
            SelectionSlot slot = _characterSlots[i];
            if (slot == null) continue;

            CharacterData data = _characterDataList[i];
            bool selected = _selectedCharacter != null && data.id == _selectedCharacter.id;
            slot.SetSelected(selected);
        }
    }

    private string BuildCharacterDescription(CharacterData character)
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

        int count = Mathf.Min(character.attrIds.Length, character.attrData.Length);
        List<string> lines = new List<string>();
        for (int i = 0; i < count; i++)
        {
            int propertyId = character.attrIds[i];
            float value = character.attrData[i];
            BasicProperties basicProperty = BasicPropertiesDataController.Instance.GetDataByKey(propertyId);
            string displayName = basicProperty != null && !string.IsNullOrEmpty(basicProperty.Description)
                ? basicProperty.Description
                : ((PropertyType)propertyId).ToString();

            string formattedValue = basicProperty != null && basicProperty.ValueType == ValueType.Percentage
                ? $"{value:+0.#;-0.#;0}%"
                : $"{value:+0.#;-0.#;0}";

            lines.Add($"{displayName} {formattedValue}");
        }

        return string.Join("\n", lines);
    }

    private void BackToMenu()
    {
        if (UIManager.Instance == null) return;

        UIManager.Instance.ClosePanel<CharacterSelectPanel>();
        UIManager.Instance.OpenPanel<MainMenuPanel>("UI/Panels/MainMenu", UILayer.Panel);
    }

    private void HandleStartPlaceholder()
    {
        if (_selectedCharacter == null)
        {
            Debug.LogWarning("[CharacterSelectPanel] 当前没有选中的角色，无法进入武器选择。");
            return;
        }

        RunStartContext.Instance.SetCharacter(_selectedCharacter.id);
        RunStartContext.Instance.ResetWeaponSelection();
        RunStartContext.Instance.ResetDifficultySelection();

        if (UIManager.Instance == null)
        {
            Debug.LogWarning("[CharacterSelectPanel] 找不到 UIManager，无法打开武器选择页。");
            return;
        }

        UIManager.Instance.ClosePanel<CharacterSelectPanel>();
        UIManager.Instance.OpenPanel<WeaponSelectionPanel>("UI/Panels/WeaponSelection", UILayer.Panel, _selectedCharacter);
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

        sprite = Resources.Load<Sprite>($"UI/Panels/CharacterSelection/Characters/{characterImage}");
        if (sprite != null) return sprite;

        return Resources.Load<Sprite>(characterImage);
    }
}

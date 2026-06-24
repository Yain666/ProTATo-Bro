using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectPanel : BasePanel
{
    private const string CharacterSlotBackgroundPath = "UI/Panels/ShopPanel/Textures/slot_empty";

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

    private readonly List<Button> _characterButtons = new List<Button>();
    private readonly List<CharacterData> _characterDataList = new List<CharacterData>();
    private CharacterData _selectedCharacter;
    private Sprite _slotBackground;

    protected override void OnOpen(object args)
    {
        CharacterDataController.Instance.Init();
        BasicPropertiesDataController.Instance.Init();
        EnsureReferences();
        BindButtons();
        BuildCharacterGrid();
        SelectDefaultCharacter();
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
        if (backButton != null) backButton.onClick.AddListener(BackToMenu);
        if (startButton != null) startButton.onClick.AddListener(HandleStartPlaceholder);
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
        _slotBackground = Resources.Load<Sprite>(CharacterSlotBackgroundPath);

        List<CharacterData> characters = CharacterDataController.Instance.GetAllData();
        for (int i = 0; i < characters.Count; i++)
        {
            CharacterData character = characters[i];
            _characterDataList.Add(character);
            Button button = CreateCharacterItem(gridContent, character);
            _characterButtons.Add(button);
        }
    }

    private void ClearCharacterButtons()
    {
        for (int i = gridContent != null ? gridContent.childCount - 1 : -1; i >= 0; i--)
        {
            Destroy(gridContent.GetChild(i).gameObject);
        }

        _characterButtons.Clear();
    }

    private Button CreateCharacterItem(Transform parent, CharacterData character)
    {
        GameObject buttonObject = new GameObject($"Character_{character.id}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.sizeDelta = gridLayoutGroup != null ? gridLayoutGroup.cellSize : new Vector2(96f, 96f);

        Image background = buttonObject.GetComponent<Image>();
        background.sprite = _slotBackground;
        background.type = Image.Type.Simple;
        background.color = new Color(0.35f, 0.4f, 0.45f, 1f);

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = background;
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.35f, 0.4f, 0.45f, 1f);
        colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        colors.selectedColor = new Color(0.95f, 0.95f, 0.95f, 1f);
        colors.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
        colors.disabledColor = new Color(0.25f, 0.25f, 0.25f, 0.6f);
        button.colors = colors;

        CreateCharacterItemIcon(buttonObject.transform, character.characterImage);
        CreateCharacterItemLabel(buttonObject.transform, character.characterName);

        button.onClick.AddListener(() => SelectCharacter(character));
        return button;
    }

    private void CreateCharacterItemIcon(Transform parent, string characterImage)
    {
        GameObject iconObject = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        iconObject.transform.SetParent(parent, false);

        RectTransform rect = iconObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(10f, 10f);
        rect.offsetMax = new Vector2(-10f, -18f);

        Image icon = iconObject.GetComponent<Image>();
        icon.sprite = LoadCharacterSprite(characterImage);
        icon.color = Color.white;
        icon.preserveAspect = true;
        icon.raycastTarget = false;
    }

    private void CreateCharacterItemLabel(Transform parent, string characterName)
    {
        GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        labelObject.transform.SetParent(parent, false);

        RectTransform rect = labelObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(0f, 2f);
        rect.sizeDelta = new Vector2(0f, 20f);

        Text label = labelObject.GetComponent<Text>();
        label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        label.text = characterName;
        label.fontSize = 14;
        label.alignment = TextAnchor.LowerCenter;
        label.color = Color.white;
        label.raycastTarget = false;
    }

    private void SelectDefaultCharacter()
    {
        if (_characterDataList.Count > 0)
        {
            SelectCharacter(_characterDataList[0]);
        }
        else
        {
            _selectedCharacter = null;
            RefreshSelectionView();
        }
    }

    private void SelectCharacter(CharacterData character)
    {
        _selectedCharacter = character;
        RefreshSelectionView();
    }

    private void RefreshSelectionView()
    {
        if (_selectedCharacter == null)
        {
            SetText(selectedCharacterNameText, "请选择角色");
            SetText(selectedCharacterJobText, string.Empty);
            SetText(selectedCharacterDescriptionText, "当前没有可用角色数据。");
            if (selectedCharacterIcon != null) selectedCharacterIcon.sprite = null;
            SetText(maxDifficultyText, "最高通关难度: --");
            SetText(maxEndlessText, "最高无尽波数: --");
            return;
        }

        SetText(selectedCharacterNameText, _selectedCharacter.characterName);
        SetText(selectedCharacterJobText, _selectedCharacter.job);
        SetText(selectedCharacterDescriptionText, BuildCharacterDescription(_selectedCharacter));
        if (selectedCharacterIcon != null)
        {
            selectedCharacterIcon.sprite = LoadCharacterSprite(_selectedCharacter.characterImage);
            selectedCharacterIcon.color = Color.white;
            selectedCharacterIcon.preserveAspect = true;
        }

        SetText(maxDifficultyText, "最高通关难度: --");
        SetText(maxEndlessText, "最高无尽波数: --");
        UpdateCharacterButtonStates();
    }

    private void UpdateCharacterButtonStates()
    {
        for (int i = 0; i < _characterButtons.Count && i < _characterDataList.Count; i++)
        {
            Button button = _characterButtons[i];
            Image image = button != null ? button.targetGraphic as Image : null;
            if (button == null || image == null) continue;

            CharacterData data = _characterDataList[i];
            bool selected = _selectedCharacter != null && data.id == _selectedCharacter.id;
            image.color = selected ? new Color(1f, 1f, 1f, 1f) : new Color(0.35f, 0.4f, 0.45f, 1f);
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
        Debug.Log(_selectedCharacter != null
            ? $"[CharacterSelectPanel] Start reserved with character: {_selectedCharacter.characterName}"
            : "[CharacterSelectPanel] Start reserved without character.");
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

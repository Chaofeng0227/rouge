using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LevelUpUI : MonoBehaviour
{
    private static LevelUpUI instance;

    private readonly Button[] optionButtons = new Button[3];
    private readonly Text[] optionTitles = new Text[3];
    private readonly Text[] optionDescriptions = new Text[3];

    private GameObject hudRoot;
    private GameObject levelUpPanel;
    private Text hudText;
    private Text stageText;
    private Action<PlayerUpgradeChoice> onChoiceSelected;
    private readonly List<PlayerUpgradeChoice> currentChoices = new List<PlayerUpgradeChoice>();

    public static LevelUpUI Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject uiObject = new GameObject("LevelUpUI");
                instance = uiObject.AddComponent<LevelUpUI>();
            }

            return instance;
        }
    }

    public void UpdateHud(int level, int currentExperience, int requiredExperience)
    {
        if (hudText == null)
        {
            return;
        }

        hudText.text = $"Lv.{level}  XP {currentExperience}/{requiredExperience}";
    }

    public void ShowLevelUp(int level, List<PlayerUpgradeChoice> choices, Action<PlayerUpgradeChoice> onSelected)
    {
        EnsureEventSystemExists();

        onChoiceSelected = onSelected;
        currentChoices.Clear();
        currentChoices.AddRange(choices);

        for (int i = 0; i < optionButtons.Length; i++)
        {
            int index = i;
            bool hasChoice = i < currentChoices.Count;

            optionButtons[i].gameObject.SetActive(hasChoice);
            optionButtons[i].onClick.RemoveAllListeners();

            if (!hasChoice)
            {
                continue;
            }

            optionTitles[i].text = currentChoices[i].Title;
            optionDescriptions[i].text = currentChoices[i].Description;
            optionButtons[i].onClick.AddListener(() => SelectChoice(index));
        }

        levelUpPanel.SetActive(true);
        Time.timeScale = 0f;
        UpdateHud(level, PlayerProgression.Instance.CurrentExperience, PlayerProgression.Instance.RequiredExperience);
    }

    void Update()
    {
        if (stageText == null)
        {
            return;
        }

        int stage = EnemyStageDirector.Instance != null ? EnemyStageDirector.Instance.CurrentStage : 0;
        stageText.text = $"Stage {stage}";
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log("LevelUpUI Awake");
        BuildUI();
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    void BuildUI()
    {
        Debug.Log("LevelUpUI BuildUI");

        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 900;

        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        gameObject.AddComponent<GraphicRaycaster>();

        Font font = LoadBuiltInFont();
        if (font == null)
        {
            Debug.LogError("LevelUpUI: failed to load a built-in font.");
            return;
        }

        Debug.Log("LevelUpUI font loaded: " + font.name);

        hudRoot = CreateUIObject("HUDRoot", transform);
        RectTransform hudRootRect = hudRoot.GetComponent<RectTransform>();
        hudRootRect.anchorMin = Vector2.zero;
        hudRootRect.anchorMax = Vector2.one;
        hudRootRect.offsetMin = Vector2.zero;
        hudRootRect.offsetMax = Vector2.zero;

        GameObject hudObject = CreateUIObject("HUD", hudRoot.transform);
        hudText = hudObject.AddComponent<Text>();
        hudText.font = font;
        hudText.fontSize = 24;
        hudText.alignment = TextAnchor.UpperLeft;
        hudText.color = Color.white;

        RectTransform hudRect = hudObject.GetComponent<RectTransform>();
        hudRect.anchorMin = new Vector2(0f, 1f);
        hudRect.anchorMax = new Vector2(0f, 1f);
        hudRect.pivot = new Vector2(0f, 1f);
        hudRect.sizeDelta = new Vector2(360f, 50f);
        hudRect.anchoredPosition = new Vector2(20f, -20f);

        GameObject stagePanelObject = CreateUIObject("StagePanel", hudRoot.transform);
        Image stagePanelImage = stagePanelObject.AddComponent<Image>();
        stagePanelImage.color = new Color(0f, 0f, 0f, 0.72f);

        RectTransform stagePanelRect = stagePanelObject.GetComponent<RectTransform>();
        stagePanelRect.anchorMin = new Vector2(1f, 1f);
        stagePanelRect.anchorMax = new Vector2(1f, 1f);
        stagePanelRect.pivot = new Vector2(1f, 1f);
        stagePanelRect.sizeDelta = new Vector2(220f, 52f);
        stagePanelRect.anchoredPosition = new Vector2(-16f, -16f);

        GameObject stageTextObject = CreateUIObject("StageText", stagePanelObject.transform);
        stageText = stageTextObject.AddComponent<Text>();
        stageText.font = font;
        stageText.text = "Stage 0";
        stageText.fontSize = 24;
        stageText.fontStyle = FontStyle.Bold;
        stageText.alignment = TextAnchor.MiddleRight;
        stageText.color = Color.white;
        stageText.horizontalOverflow = HorizontalWrapMode.Overflow;
        stageText.verticalOverflow = VerticalWrapMode.Overflow;

        RectTransform stageTextRect = stageTextObject.GetComponent<RectTransform>();
        stageTextRect.anchorMin = Vector2.zero;
        stageTextRect.anchorMax = Vector2.one;
        stageTextRect.offsetMin = new Vector2(14f, 0f);
        stageTextRect.offsetMax = new Vector2(-14f, 0f);

        levelUpPanel = CreateUIObject("LevelUpPanel", transform);
        RectTransform panelRect = levelUpPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        GameObject headerObject = CreateUIObject("Header", levelUpPanel.transform);
        Image headerImage = headerObject.AddComponent<Image>();
        headerImage.color = new Color(0.08f, 0.08f, 0.08f, 0.82f);

        RectTransform headerRect = headerObject.GetComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0.5f, 0.5f);
        headerRect.anchorMax = new Vector2(0.5f, 0.5f);
        headerRect.sizeDelta = new Vector2(520f, 110f);
        headerRect.anchoredPosition = new Vector2(0f, 250f);

        GameObject titleObject = CreateUIObject("Title", headerObject.transform);
        Text titleText = titleObject.AddComponent<Text>();
        titleText.font = font;
        titleText.text = "Level Up";
        titleText.fontSize = 40;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = Color.white;

        RectTransform titleRect = titleObject.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 0f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.offsetMin = new Vector2(0f, 18f);
        titleRect.offsetMax = new Vector2(0f, -46f);

        GameObject subtitleObject = CreateUIObject("Subtitle", headerObject.transform);
        Text subtitleText = subtitleObject.AddComponent<Text>();
        subtitleText.font = font;
        subtitleText.text = "Choose 1 upgrade";
        subtitleText.fontSize = 18;
        subtitleText.alignment = TextAnchor.MiddleCenter;
        subtitleText.color = new Color(0.9f, 0.9f, 0.9f, 1f);

        RectTransform subtitleRect = subtitleObject.GetComponent<RectTransform>();
        subtitleRect.anchorMin = new Vector2(0f, 0f);
        subtitleRect.anchorMax = new Vector2(1f, 1f);
        subtitleRect.offsetMin = new Vector2(0f, -18f);
        subtitleRect.offsetMax = new Vector2(0f, -72f);

        for (int i = 0; i < optionButtons.Length; i++)
        {
            GameObject buttonObject = CreateUIObject($"Option{i + 1}", levelUpPanel.transform);
            Image buttonImage = buttonObject.AddComponent<Image>();
            buttonImage.color = GetCardColor(i);

            Button button = buttonObject.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = buttonImage.color;
            colors.highlightedColor = buttonImage.color * 1.08f;
            colors.pressedColor = buttonImage.color * 0.9f;
            colors.selectedColor = colors.highlightedColor;
            button.colors = colors;
            button.targetGraphic = buttonImage;
            optionButtons[i] = button;

            RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
            buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
            buttonRect.sizeDelta = new Vector2(240f, 340f);
            buttonRect.anchoredPosition = new Vector2(-280f + i * 280f, -20f);

            GameObject badgeObject = CreateUIObject("Badge", buttonObject.transform);
            Image badgeImage = badgeObject.AddComponent<Image>();
            badgeImage.color = new Color(0f, 0f, 0f, 0.18f);

            RectTransform badgeRect = badgeObject.GetComponent<RectTransform>();
            badgeRect.anchorMin = new Vector2(0.5f, 1f);
            badgeRect.anchorMax = new Vector2(0.5f, 1f);
            badgeRect.sizeDelta = new Vector2(124f, 32f);
            badgeRect.anchoredPosition = new Vector2(0f, -26f);

            GameObject badgeTextObject = CreateUIObject("BadgeText", badgeObject.transform);
            Text badgeText = badgeTextObject.AddComponent<Text>();
            badgeText.font = font;
            badgeText.text = "UPGRADE";
            badgeText.fontSize = 15;
            badgeText.alignment = TextAnchor.MiddleCenter;
            badgeText.color = new Color(0.1f, 0.08f, 0.05f, 0.95f);

            RectTransform badgeTextRect = badgeTextObject.GetComponent<RectTransform>();
            badgeTextRect.anchorMin = Vector2.zero;
            badgeTextRect.anchorMax = Vector2.one;
            badgeTextRect.offsetMin = Vector2.zero;
            badgeTextRect.offsetMax = Vector2.zero;

            GameObject titleChild = CreateUIObject("Title", buttonObject.transform);
            Text optionTitle = titleChild.AddComponent<Text>();
            optionTitle.font = font;
            optionTitle.fontSize = 26;
            optionTitle.alignment = TextAnchor.UpperCenter;
            optionTitle.color = Color.white;
            optionTitles[i] = optionTitle;

            RectTransform optionTitleRect = titleChild.GetComponent<RectTransform>();
            optionTitleRect.anchorMin = new Vector2(0f, 0f);
            optionTitleRect.anchorMax = new Vector2(1f, 1f);
            optionTitleRect.offsetMin = new Vector2(18f, 132f);
            optionTitleRect.offsetMax = new Vector2(-18f, -76f);

            GameObject descriptionChild = CreateUIObject("Description", buttonObject.transform);
            Text optionDescription = descriptionChild.AddComponent<Text>();
            optionDescription.font = font;
            optionDescription.fontSize = 18;
            optionDescription.alignment = TextAnchor.UpperCenter;
            optionDescription.color = new Color(0.18f, 0.14f, 0.08f, 0.95f);
            optionDescriptions[i] = optionDescription;

            RectTransform optionDescriptionRect = descriptionChild.GetComponent<RectTransform>();
            optionDescriptionRect.anchorMin = new Vector2(0f, 0f);
            optionDescriptionRect.anchorMax = new Vector2(1f, 1f);
            optionDescriptionRect.offsetMin = new Vector2(22f, 36f);
            optionDescriptionRect.offsetMax = new Vector2(-22f, -162f);

            GameObject footerObject = CreateUIObject("Footer", buttonObject.transform);
            Text footerText = footerObject.AddComponent<Text>();
            footerText.font = font;
            footerText.text = "Click to take";
            footerText.fontSize = 18;
            footerText.alignment = TextAnchor.MiddleCenter;
            footerText.color = new Color(0.18f, 0.14f, 0.08f, 0.8f);

            RectTransform footerRect = footerObject.GetComponent<RectTransform>();
            footerRect.anchorMin = new Vector2(0f, 0f);
            footerRect.anchorMax = new Vector2(1f, 0f);
            footerRect.sizeDelta = new Vector2(0f, 34f);
            footerRect.anchoredPosition = new Vector2(0f, 18f);
        }

        levelUpPanel.SetActive(false);
    }

    void SelectChoice(int index)
    {
        if (index < 0 || index >= currentChoices.Count)
        {
            return;
        }

        PlayerUpgradeChoice selectedChoice = currentChoices[index];
        levelUpPanel.SetActive(false);
        Time.timeScale = 1f;
        onChoiceSelected?.Invoke(selectedChoice);
    }

    void EnsureEventSystemExists()
    {
        if (FindObjectOfType<EventSystem>() != null)
        {
            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<StandaloneInputModule>();
    }

    Font LoadBuiltInFont()
    {
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font != null)
        {
            return font;
        }

        return Resources.GetBuiltinResource<Font>("Arial.ttf");
    }

    GameObject CreateUIObject(string objectName, Transform parent)
    {
        GameObject uiObject = new GameObject(objectName);
        uiObject.transform.SetParent(parent, false);
        uiObject.AddComponent<RectTransform>();
        return uiObject;
    }

    Color GetCardColor(int index)
    {
        switch (index)
        {
            case 0:
                return new Color(0.9f, 0.76f, 0.44f, 0.96f);
            case 1:
                return new Color(0.57f, 0.82f, 0.66f, 0.96f);
            default:
                return new Color(0.56f, 0.72f, 0.93f, 0.96f);
        }
    }
}

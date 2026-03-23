using UnityEngine;
using UnityEngine.UI;

public class StageHUD : MonoBehaviour
{
    private static StageHUD instance;

    private Text stageText;
    private GameObject rootPanel;

    public static void EnsureInstance()
    {
        if (instance != null)
        {
            return;
        }

        GameObject hudObject = new GameObject("StageHUD_Runtime");
        instance = hudObject.AddComponent<StageHUD>();
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        BuildUI();
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

    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    void BuildUI()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 850;

        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        gameObject.AddComponent<GraphicRaycaster>();

        Font font = LoadBuiltInFont();
        if (font == null)
        {
            Debug.LogError("StageHUD: failed to load a built-in font.");
            return;
        }

        rootPanel = CreateUIObject("RootPanel", transform);
        RectTransform rootRect = rootPanel.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        GameObject panelObject = CreateUIObject("Panel", rootPanel.transform);
        Image panelImage = panelObject.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.72f);

        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(1f, 1f);
        panelRect.anchorMax = new Vector2(1f, 1f);
        panelRect.pivot = new Vector2(1f, 1f);
        panelRect.sizeDelta = new Vector2(220f, 52f);
        panelRect.anchoredPosition = new Vector2(-16f, -16f);

        GameObject textObject = CreateUIObject("Text", panelObject.transform);
        stageText = textObject.AddComponent<Text>();
        stageText.font = font;
        stageText.text = "Stage 0";
        stageText.fontSize = 24;
        stageText.fontStyle = FontStyle.Bold;
        stageText.alignment = TextAnchor.MiddleRight;
        stageText.color = Color.white;
        stageText.horizontalOverflow = HorizontalWrapMode.Overflow;
        stageText.verticalOverflow = VerticalWrapMode.Overflow;

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(14f, 0f);
        textRect.offsetMax = new Vector2(-14f, 0f);
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
}

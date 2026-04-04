using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class VictoryUI : MonoBehaviour
{
    private static VictoryUI instance;
    private GameObject rootPanel;

    public static void Show()
    {
        if (instance == null)
        {
            GameObject uiObject = new GameObject("VictoryUI");
            instance = uiObject.AddComponent<VictoryUI>();
            DontDestroyOnLoad(uiObject);
        }

        instance.ShowInternal();
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
        HideImmediate();
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
        canvas.sortingOrder = 1100;

        gameObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        gameObject.AddComponent<GraphicRaycaster>();

        EnsureEventSystemExists();

        Font font = LoadBuiltInFont();
        if (font == null)
        {
            return;
        }

        rootPanel = CreateUIObject("Panel", transform);
        Image panelImage = rootPanel.AddComponent<Image>();
        panelImage.color = new Color(0.05f, 0.08f, 0.05f, 0.84f);

        RectTransform panelRect = rootPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        GameObject titleObject = CreateUIObject("Title", rootPanel.transform);
        Text titleText = titleObject.AddComponent<Text>();
        titleText.font = font;
        titleText.text = "VICTORY";
        titleText.fontSize = 42;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = new Color(0.97f, 0.93f, 0.72f, 1f);

        RectTransform titleRect = titleObject.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.5f);
        titleRect.anchorMax = new Vector2(0.5f, 0.5f);
        titleRect.sizeDelta = new Vector2(520f, 80f);
        titleRect.anchoredPosition = new Vector2(0f, 60f);

        GameObject subtitleObject = CreateUIObject("Subtitle", rootPanel.transform);
        Text subtitleText = subtitleObject.AddComponent<Text>();
        subtitleText.font = font;
        subtitleText.text = "Boss defeated on Floor 5";
        subtitleText.fontSize = 24;
        subtitleText.alignment = TextAnchor.MiddleCenter;
        subtitleText.color = Color.white;

        RectTransform subtitleRect = subtitleObject.GetComponent<RectTransform>();
        subtitleRect.anchorMin = new Vector2(0.5f, 0.5f);
        subtitleRect.anchorMax = new Vector2(0.5f, 0.5f);
        subtitleRect.sizeDelta = new Vector2(560f, 50f);
        subtitleRect.anchoredPosition = new Vector2(0f, 10f);

        GameObject buttonObject = CreateUIObject("RestartButton", rootPanel.transform);
        Image buttonImage = buttonObject.AddComponent<Image>();
        buttonImage.color = new Color(0.94f, 0.77f, 0.26f, 1f);

        Button restartButton = buttonObject.AddComponent<Button>();
        ColorBlock colors = restartButton.colors;
        colors.normalColor = buttonImage.color;
        colors.highlightedColor = new Color(1f, 0.86f, 0.35f, 1f);
        colors.pressedColor = new Color(0.84f, 0.65f, 0.18f, 1f);
        colors.selectedColor = colors.highlightedColor;
        restartButton.colors = colors;
        restartButton.targetGraphic = buttonImage;
        restartButton.onClick.AddListener(RestartGame);

        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.sizeDelta = new Vector2(220f, 64f);
        buttonRect.anchoredPosition = new Vector2(0f, -60f);

        GameObject buttonTextObject = CreateUIObject("Text", buttonObject.transform);
        Text buttonText = buttonTextObject.AddComponent<Text>();
        buttonText.font = font;
        buttonText.text = "Restart";
        buttonText.fontSize = 28;
        buttonText.alignment = TextAnchor.MiddleCenter;
        buttonText.color = new Color(0.14f, 0.1f, 0.02f, 1f);

        RectTransform buttonTextRect = buttonTextObject.GetComponent<RectTransform>();
        buttonTextRect.anchorMin = Vector2.zero;
        buttonTextRect.anchorMax = Vector2.one;
        buttonTextRect.offsetMin = Vector2.zero;
        buttonTextRect.offsetMax = Vector2.zero;
    }

    void ShowInternal()
    {
        EnsureEventSystemExists();
        if (rootPanel == null)
        {
            BuildUI();
        }

        if (rootPanel == null)
        {
            return;
        }

        rootPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    void HideImmediate()
    {
        if (rootPanel != null)
        {
            rootPanel.SetActive(false);
        }
    }

    void RestartGame()
    {
        Time.timeScale = 1f;
        HideImmediate();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void EnsureEventSystemExists()
    {
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() != null)
        {
            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystemObject.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
    }

    GameObject CreateUIObject(string objectName, Transform parent)
    {
        GameObject uiObject = new GameObject(objectName);
        uiObject.transform.SetParent(parent, false);
        uiObject.AddComponent<RectTransform>();
        return uiObject;
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
}

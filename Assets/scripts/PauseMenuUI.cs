using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuUI : MonoBehaviour
{
    private static PauseMenuUI instance;

    private GameObject rootPanel;
    private bool isVisible;

    public static void EnsureInstance()
    {
        if (instance != null)
        {
            return;
        }

        GameObject uiObject = new GameObject("PauseMenuUI");
        instance = uiObject.AddComponent<PauseMenuUI>();
        DontDestroyOnLoad(uiObject);
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

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Toggle();
        }
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    void Toggle()
    {
        if (isVisible)
        {
            ResumeGame();
        }
        else
        {
            ShowInternal();
        }
    }

    void BuildUI()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1050;

        gameObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        gameObject.AddComponent<GraphicRaycaster>();

        EnsureEventSystemExists();

        Font font = LoadBuiltInFont();
        if (font == null)
        {
            Debug.LogError("PauseMenuUI: failed to load a built-in font.");
            return;
        }

        rootPanel = CreateUIObject("Panel", transform);
        Image panelImage = rootPanel.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.72f);

        RectTransform panelRect = rootPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        GameObject titleObject = CreateUIObject("Title", rootPanel.transform);
        Text titleText = titleObject.AddComponent<Text>();
        titleText.font = font;
        titleText.text = "PAUSED";
        titleText.fontSize = 40;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = Color.white;

        RectTransform titleRect = titleObject.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.5f);
        titleRect.anchorMax = new Vector2(0.5f, 0.5f);
        titleRect.sizeDelta = new Vector2(420f, 70f);
        titleRect.anchoredPosition = new Vector2(0f, 110f);

        CreateMenuButton(rootPanel.transform, font, "ResumeButton", "Resume", new Vector2(0f, 25f), ResumeGame);
        CreateMenuButton(rootPanel.transform, font, "RestartButton", "Restart", new Vector2(0f, -55f), RestartGame);
        CreateMenuButton(rootPanel.transform, font, "QuitButton", "Quit", new Vector2(0f, -135f), QuitGame);
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
        isVisible = true;
        Time.timeScale = 0f;
    }

    void HideImmediate()
    {
        if (rootPanel != null)
        {
            rootPanel.SetActive(false);
        }

        isVisible = false;
    }

    void ResumeGame()
    {
        Time.timeScale = 1f;
        HideImmediate();
    }

    void RestartGame()
    {
        Time.timeScale = 1f;
        HideImmediate();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void QuitGame()
    {
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void CreateMenuButton(Transform parent, Font font, string objectName, string label, Vector2 anchoredPosition, UnityEngine.Events.UnityAction onClick)
    {
        GameObject buttonObject = CreateUIObject(objectName, parent);
        Image buttonImage = buttonObject.AddComponent<Image>();
        buttonImage.color = new Color(0.92f, 0.76f, 0.24f, 1f);

        Button button = buttonObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = buttonImage.color;
        colors.highlightedColor = new Color(1f, 0.84f, 0.32f, 1f);
        colors.pressedColor = new Color(0.85f, 0.68f, 0.18f, 1f);
        colors.selectedColor = colors.highlightedColor;
        button.colors = colors;
        button.targetGraphic = buttonImage;
        button.onClick.AddListener(onClick);

        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.sizeDelta = new Vector2(220f, 62f);
        buttonRect.anchoredPosition = anchoredPosition;

        GameObject textObject = CreateUIObject("Text", buttonObject.transform);
        Text buttonText = textObject.AddComponent<Text>();
        buttonText.font = font;
        buttonText.text = label;
        buttonText.fontSize = 26;
        buttonText.alignment = TextAnchor.MiddleCenter;
        buttonText.color = new Color(0.14f, 0.1f, 0.02f, 1f);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
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

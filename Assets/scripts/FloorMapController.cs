using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class FloorMapController : MonoBehaviour
{
    private static FloorMapController instance;

    [SerializeField] private KeyCode toggleKey = KeyCode.Tab;
    [SerializeField] private Vector2 panelSize = new Vector2(720f, 520f);
    [SerializeField] private Vector2 mapAreaSize = new Vector2(620f, 360f);
    [SerializeField] private float mapPadding = 36f;
    [SerializeField] private float roomNodeSize = 18f;
    [SerializeField] private float markerSize = 22f;
    [SerializeField] private float connectionThickness = 6f;

    private DungeonGenerator generator;
    private Font uiFont;

    private GameObject overlayRoot;
    private GameObject mapContentRoot;
    private RectTransform mapAreaRect;
    private Text titleText;
    private RectTransform playerMarkerRect;
    private RectTransform portalMarkerRect;
    private NextLevelPortal currentPortal;

    private Vector2Int minGrid;
    private Vector2Int maxGrid;
    private int lastRoomCount = -1;
    private int lastLevel = -1;

    public static void EnsureInstance()
    {
        if (instance != null)
        {
            return;
        }

        GameObject controllerObject = new GameObject("FloorMapController");
        instance = controllerObject.AddComponent<FloorMapController>();
        DontDestroyOnLoad(controllerObject);
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
        BuildUI();
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleMap();
        }

        ResolveGenerator();

        if (generator == null)
        {
            return;
        }

        int roomCount = generator.Graph != null ? generator.Graph.Count : 0;
        if (roomCount != lastRoomCount || generator.currentLevel != lastLevel)
        {
            RebuildMap();
        }

        if (overlayRoot != null && overlayRoot.activeSelf)
        {
            UpdateDynamicMarkers();
        }
    }

    void ToggleMap()
    {
        if (overlayRoot == null)
        {
            return;
        }

        bool shouldShow = !overlayRoot.activeSelf;
        overlayRoot.SetActive(shouldShow);

        if (shouldShow)
        {
            ResolveGenerator();
            RebuildMap();
            UpdateDynamicMarkers();
        }
    }

    void ResolveGenerator()
    {
        if (generator == null)
        {
            generator = Object.FindFirstObjectByType<DungeonGenerator>();
        }

        if (currentPortal == null)
        {
            currentPortal = Object.FindFirstObjectByType<NextLevelPortal>();
        }
    }

    void BuildUI()
    {
        uiFont = LoadBuiltInFont();
        if (uiFont == null)
        {
            Debug.LogError("FloorMapController: failed to load built-in font.");
            return;
        }

        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1200;

        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        gameObject.AddComponent<GraphicRaycaster>();

        overlayRoot = CreateUIObject("OverlayRoot", transform);
        Image overlayImage = overlayRoot.AddComponent<Image>();
        overlayImage.color = new Color(0f, 0f, 0f, 0.58f);

        RectTransform overlayRect = overlayRoot.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;

        GameObject panelObject = CreateUIObject("Panel", overlayRoot.transform);
        Image panelImage = panelObject.AddComponent<Image>();
        panelImage.color = new Color(0.08f, 0.11f, 0.16f, 0.96f);

        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = panelSize;
        panelRect.anchoredPosition = Vector2.zero;

        GameObject titleObject = CreateUIObject("Title", panelObject.transform);
        titleText = titleObject.AddComponent<Text>();
        titleText.font = uiFont;
        titleText.fontSize = 28;
        titleText.fontStyle = FontStyle.Bold;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = Color.white;
        titleText.text = "Floor Map";

        RectTransform titleRect = titleObject.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.sizeDelta = new Vector2(0f, 54f);
        titleRect.anchoredPosition = new Vector2(0f, -18f);

        GameObject hintObject = CreateUIObject("Hint", panelObject.transform);
        Text hintText = hintObject.AddComponent<Text>();
        hintText.font = uiFont;
        hintText.fontSize = 18;
        hintText.alignment = TextAnchor.MiddleCenter;
        hintText.color = new Color(0.82f, 0.9f, 1f, 0.92f);
        hintText.text = "Tab Close  |  Yellow: You  |  Cyan: Exit";

        RectTransform hintRect = hintObject.GetComponent<RectTransform>();
        hintRect.anchorMin = new Vector2(0f, 0f);
        hintRect.anchorMax = new Vector2(1f, 0f);
        hintRect.pivot = new Vector2(0.5f, 0f);
        hintRect.sizeDelta = new Vector2(0f, 40f);
        hintRect.anchoredPosition = new Vector2(0f, 16f);

        GameObject mapFrameObject = CreateUIObject("MapFrame", panelObject.transform);
        Image mapFrameImage = mapFrameObject.AddComponent<Image>();
        mapFrameImage.color = new Color(0.14f, 0.18f, 0.25f, 1f);

        mapAreaRect = mapFrameObject.GetComponent<RectTransform>();
        mapAreaRect.anchorMin = new Vector2(0.5f, 0.5f);
        mapAreaRect.anchorMax = new Vector2(0.5f, 0.5f);
        mapAreaRect.pivot = new Vector2(0.5f, 0.5f);
        mapAreaRect.sizeDelta = mapAreaSize;
        mapAreaRect.anchoredPosition = new Vector2(0f, -8f);

        mapContentRoot = CreateUIObject("MapContent", mapFrameObject.transform);
        RectTransform contentRect = mapContentRoot.GetComponent<RectTransform>();
        contentRect.anchorMin = Vector2.zero;
        contentRect.anchorMax = Vector2.one;
        contentRect.offsetMin = Vector2.zero;
        contentRect.offsetMax = Vector2.zero;

        overlayRoot.SetActive(false);
    }

    void RebuildMap()
    {
        if (generator == null || mapContentRoot == null || mapAreaRect == null)
        {
            return;
        }

        Dictionary<Vector2Int, HashSet<Vector2Int>> graph = generator.Graph;
        if (graph == null || graph.Count == 0)
        {
            return;
        }

        foreach (Transform child in mapContentRoot.transform)
        {
            Destroy(child.gameObject);
        }

        List<Vector2Int> roomPositions = new List<Vector2Int>(graph.Keys);
        minGrid = roomPositions[0];
        maxGrid = roomPositions[0];

        for (int i = 1; i < roomPositions.Count; i++)
        {
            minGrid = Vector2Int.Min(minGrid, roomPositions[i]);
            maxGrid = Vector2Int.Max(maxGrid, roomPositions[i]);
        }

        titleText.text = $"Floor Map  |  Floor {generator.currentLevel}";
        lastRoomCount = graph.Count;
        lastLevel = generator.currentLevel;

        foreach (KeyValuePair<Vector2Int, HashSet<Vector2Int>> pair in graph)
        {
            foreach (Vector2Int neighbor in pair.Value)
            {
                if (pair.Key.x < neighbor.x || (pair.Key.x == neighbor.x && pair.Key.y < neighbor.y))
                {
                    CreateConnection(pair.Key, neighbor);
                }
            }
        }

        foreach (Vector2Int room in roomPositions)
        {
            Color color = new Color(0.78f, 0.83f, 0.9f, 1f);
            if (room == Vector2Int.zero)
            {
                color = new Color(0.42f, 0.93f, 0.55f, 1f);
            }
            else if (room == generator.EndRoomPos)
            {
                color = new Color(0.28f, 0.88f, 0.96f, 1f);
            }

            CreateNode(room, color, roomNodeSize);
        }

        portalMarkerRect = CreateNode(generator.EndRoomPos, new Color(0.11f, 0.97f, 1f, 1f), markerSize + 2f);
        playerMarkerRect = CreateNode(Vector2Int.zero, new Color(1f, 0.85f, 0.2f, 1f), markerSize);
    }

    void UpdateDynamicMarkers()
    {
        if (generator == null)
        {
            return;
        }

        if (portalMarkerRect != null)
        {
            if (currentPortal == null)
            {
                currentPortal = Object.FindFirstObjectByType<NextLevelPortal>();
            }

            if (currentPortal != null)
            {
                portalMarkerRect.anchoredPosition = GetMapPosition(currentPortal.transform.position);
            }
            else
            {
                portalMarkerRect.anchoredPosition = GetMapPosition(generator.EndRoomPos);
            }
        }

        if (playerMarkerRect == null)
        {
            return;
        }

        Transform playerTransform = generator.CurrentPlayerTransform;
        if (playerTransform == null)
        {
            playerMarkerRect.gameObject.SetActive(false);
            return;
        }

        playerMarkerRect.gameObject.SetActive(true);

        playerMarkerRect.anchoredPosition = GetMapPosition(playerTransform.position);
    }

    RectTransform CreateNode(Vector2Int gridPosition, Color color, float size)
    {
        GameObject nodeObject = CreateUIObject($"Node_{gridPosition.x}_{gridPosition.y}", mapContentRoot.transform);
        Image nodeImage = nodeObject.AddComponent<Image>();
        nodeImage.color = color;

        RectTransform nodeRect = nodeObject.GetComponent<RectTransform>();
        nodeRect.anchorMin = new Vector2(0.5f, 0.5f);
        nodeRect.anchorMax = new Vector2(0.5f, 0.5f);
        nodeRect.pivot = new Vector2(0.5f, 0.5f);
        nodeRect.sizeDelta = new Vector2(size, size);
        nodeRect.anchoredPosition = GetMapPosition(gridPosition);
        return nodeRect;
    }

    void CreateConnection(Vector2Int from, Vector2Int to)
    {
        Vector2 fromPos = GetMapPosition(from);
        Vector2 toPos = GetMapPosition(to);
        Vector2 delta = toPos - fromPos;
        float length = delta.magnitude;

        GameObject lineObject = CreateUIObject($"Line_{from.x}_{from.y}_{to.x}_{to.y}", mapContentRoot.transform);
        Image lineImage = lineObject.AddComponent<Image>();
        lineImage.color = new Color(0.32f, 0.48f, 0.7f, 0.95f);

        RectTransform lineRect = lineObject.GetComponent<RectTransform>();
        lineRect.anchorMin = new Vector2(0.5f, 0.5f);
        lineRect.anchorMax = new Vector2(0.5f, 0.5f);
        lineRect.pivot = new Vector2(0.5f, 0.5f);
        lineRect.sizeDelta = new Vector2(length, connectionThickness);
        lineRect.anchoredPosition = (fromPos + toPos) * 0.5f;
        lineRect.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
    }

    Vector2 GetMapPosition(Vector2Int gridPosition)
    {
        return GetMapPosition((Vector2)gridPosition);
    }

    Vector2 GetMapPosition(Vector3 worldPosition)
    {
        if (generator == null || Mathf.Approximately(generator.StepSize, 0f))
        {
            return Vector2.zero;
        }

        Vector2 gridPosition = new Vector2(
            worldPosition.x / generator.StepSize,
            worldPosition.y / generator.StepSize);

        return GetMapPosition(gridPosition);
    }

    Vector2 GetMapPosition(Vector2 gridPosition)
    {
        float width = mapAreaRect.rect.width > 0f ? mapAreaRect.rect.width : mapAreaRect.sizeDelta.x;
        float height = mapAreaRect.rect.height > 0f ? mapAreaRect.rect.height : mapAreaRect.sizeDelta.y;

        float usableWidth = width - mapPadding * 2f;
        float usableHeight = height - mapPadding * 2f;

        int spanX = Mathf.Max(1, maxGrid.x - minGrid.x);
        int spanY = Mathf.Max(1, maxGrid.y - minGrid.y);

        float normalizedX = spanX == 0 ? 0.5f : (gridPosition.x - minGrid.x) / spanX;
        float normalizedY = spanY == 0 ? 0.5f : (gridPosition.y - minGrid.y) / spanY;

        normalizedX = Mathf.Clamp01(normalizedX);
        normalizedY = Mathf.Clamp01(normalizedY);

        float x = Mathf.Lerp(-usableWidth * 0.5f, usableWidth * 0.5f, normalizedX);
        float y = Mathf.Lerp(-usableHeight * 0.5f, usableHeight * 0.5f, normalizedY);
        return new Vector2(x, y);
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

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

// Builds the Main Menu, HUD and Game Over screens in code.
// Added automatically by GameManager.
public class GameUI : MonoBehaviour
{
    private GameObject mainMenuPanel;
    private GameObject gameOverPanel;
    private GameObject hudPanel;
    private Image healthBarFill;
    private Font font;
    private Health playerHealth;

    private void Awake()
    {
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        EnsureEventSystem();

        var canvasGO = new GameObject("GameUI Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGO.transform.SetParent(transform, false);
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        BuildHud(canvasGO.transform);
        mainMenuPanel = BuildMainMenu(canvasGO.transform);
        gameOverPanel = BuildGameOver(canvasGO.transform);
    }

    private void Update()
    {
        if (playerHealth == null)
        {
            playerHealth = FindFirstObjectByType<Health>();
        }
        if (healthBarFill != null && playerHealth != null)
        {
            healthBarFill.fillAmount = playerHealth.CurrentHealth / playerHealth.MaxHealth;
        }
    }

    public void ShowMainMenu()
    {
        SetPanels(menu: true, hud: false, gameOver: false);
    }

    public void ShowPlaying()
    {
        SetPanels(menu: false, hud: true, gameOver: false);
    }

    public void ShowGameOver()
    {
        SetPanels(menu: false, hud: false, gameOver: true);
    }

    private void SetPanels(bool menu, bool hud, bool gameOver)
    {
        mainMenuPanel.SetActive(menu);
        hudPanel.SetActive(hud);
        gameOverPanel.SetActive(gameOver);
    }

    private GameObject BuildMainMenu(Transform parent)
    {
        var panel = CreatePanel("MainMenu Panel", parent, new Color(0.05f, 0.05f, 0.08f, 0.95f));

        CreateText(panel.transform, "COPPER STORY", 120, new Vector2(0, 250), Color.white);
        CreateButton(panel.transform, "Play", new Vector2(0, 40), GameManager.RequestStartGame);
        CreateButton(panel.transform, "Quit", new Vector2(0, -60), GameManager.RequestQuit);

        return panel;
    }

    private GameObject BuildGameOver(Transform parent)
    {
        var panel = CreatePanel("GameOver Panel", parent, new Color(0f, 0f, 0f, 0.75f));

        CreateText(panel.transform, "GAME OVER", 130, new Vector2(0, 200), new Color(0.9f, 0.2f, 0.2f));
        CreateButton(panel.transform, "Restart", new Vector2(0, 0), GameManager.RequestRestart);
        CreateButton(panel.transform, "Main Menu", new Vector2(0, -100), GameManager.RequestMainMenu);

        return panel;
    }

    private void BuildHud(Transform parent)
    {
        hudPanel = new GameObject("HUD Panel", typeof(RectTransform));
        hudPanel.transform.SetParent(parent, false);
        Stretch(hudPanel.GetComponent<RectTransform>());

        // Health bar background
        var barBg = new GameObject("HealthBar Background", typeof(RectTransform), typeof(Image));
        barBg.transform.SetParent(hudPanel.transform, false);
        var bgRect = barBg.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0f, 1f);
        bgRect.anchorMax = new Vector2(0f, 1f);
        bgRect.pivot = new Vector2(0f, 1f);
        bgRect.anchoredPosition = new Vector2(30, -30);
        bgRect.sizeDelta = new Vector2(200, 24);
        barBg.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.5f);

        // Health bar fill
        var barFill = new GameObject("HealthBar Fill", typeof(RectTransform), typeof(Image));
        barFill.transform.SetParent(barBg.transform, false);
        var fillRect = barFill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        var fillImage = barFill.GetComponent<Image>();
        fillImage.color = new Color(0.9f, 0.2f, 0.2f);
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = 0;
        healthBarFill = fillImage;

        // HP label
        var label = CreateText(barBg.transform, "HP", 20, new Vector2(-60, 0), Color.white);
        var labelRect = label.GetComponent<RectTransform>();
        labelRect.anchorMin = labelRect.anchorMax = new Vector2(0f, 0.5f);
        labelRect.pivot = new Vector2(1f, 0.5f);
        labelRect.anchoredPosition = new Vector2(-10, 0);
        label.GetComponent<Text>().alignment = TextAnchor.MiddleRight;

        hudPanel.SetActive(false);
    }

    private GameObject CreatePanel(string name, Transform parent, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        Stretch(go.GetComponent<RectTransform>());
        go.GetComponent<Image>().color = color;
        return go;
    }

    private GameObject CreateText(Transform parent, string content, int size, Vector2 position, Color color)
    {
        var go = new GameObject("Text", typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position;
        rt.sizeDelta = new Vector2(1200, size + 40);

        var text = go.GetComponent<Text>();
        text.font = font;
        text.fontSize = size;
        text.fontStyle = FontStyle.Bold;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = color;
        text.text = content;
        return go;
    }

    private void CreateButton(Transform parent, string label, Vector2 position, UnityEngine.Events.UnityAction onClick)
    {
        var go = new GameObject(label + " Button", typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position;
        rt.sizeDelta = new Vector2(420, 84);

        var image = go.GetComponent<Image>();
        image.color = new Color(0.22f, 0.22f, 0.28f);

        var button = go.GetComponent<Button>();
        button.targetGraphic = image;
        var colors = button.colors;
        colors.highlightedColor = new Color(0.4f, 0.4f, 0.5f);
        colors.pressedColor = new Color(0.15f, 0.15f, 0.2f);
        button.colors = colors;
        button.onClick.AddListener(onClick);

        var text = CreateText(go.transform, label, 44, Vector2.zero, Color.white).GetComponent<Text>();
        Stretch(text.GetComponent<RectTransform>());
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static void EnsureEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() != null)
        {
            return;
        }
        var es = new GameObject("EventSystem", typeof(EventSystem));
        es.AddComponent<InputSystemUIInputModule>();
    }
}
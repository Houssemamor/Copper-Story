using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

// Builds the Main Menu, HUD, Game Over and Win screens in code.
// Added automatically by GameManager.
public class GameUI : MonoBehaviour
{
    private GameObject mainMenuPanel;
    private GameObject gameOverPanel;
    private GameObject winPanel;
    private GameObject hudPanel;
    private Image healthBarFill;
    private GameObject bossHealthBarRoot;
    private Image bossHealthBarFill;
    private Text timerText;
    private Text winTimeText;
    private Text[] leaderboardRows;
    private Font font;
    private Health playerHealth;

    // Called by GameManager after a scene transition so the cached player
    // reference is re-resolved against the newly loaded scene.
    public void ResetPlayerReference()
    {
        playerHealth = null;
    }

    private static readonly Color GoldColor = new Color(1f, 0.85f, 0.3f);
    private static readonly Color WinColor = new Color(0.4f, 1f, 0.5f);

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
        // Match height so UI stays readable on any aspect ratio; width scales to fit.
        scaler.matchWidthOrHeight = 1f;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;

        BuildHud(canvasGO.transform);
        mainMenuPanel = BuildMainMenu(canvasGO.transform);
        gameOverPanel = BuildGameOver(canvasGO.transform);
        winPanel = BuildWin(canvasGO.transform);
    }

    private void Update()
    {
        if (playerHealth == null)
        {
            playerHealth = FindAnyObjectByType<Health>();
        }
        if (healthBarFill != null && playerHealth != null)
        {
            float value = playerHealth.CurrentHealth / playerHealth.MaxHealth;
            SetFillFraction(healthBarFill, value);
        }
        else if (playerHealth == null)
        {
            // Health component not yet present in the loaded scene; keep waiting.
        }

        // Drive the boss health bar from the currently active Masked enemy.
        MaskedEnemy boss = FindAnyObjectByType<MaskedEnemy>();
        if (bossHealthBarRoot != null)
        {
            bool showBossBar = boss != null && !boss.IsDefeated;
            if (bossHealthBarRoot.activeSelf != showBossBar)
            {
                bossHealthBarRoot.SetActive(showBossBar);
            }
            if (showBossBar && bossHealthBarFill != null)
            {
                int maxHp = boss.MaxHitPoints;
                if (maxHp > 0)
                {
                    float bossValue = (float)boss.HitPoints / maxHp;
                    SetFillFraction(bossHealthBarFill, bossValue);
                }
            }
        }

        if (timerText != null && GameManager.Instance != null)
        {
            bool showTimer = GameManager.Instance.TimerEnabled;
            if (timerText.gameObject.activeSelf != showTimer)
            {
                timerText.gameObject.SetActive(showTimer);
            }
            if (showTimer)
            {
                timerText.text = "TIME  " + Leaderboard.FormatTime(GameManager.Instance.ElapsedTime);
            }
        }
    }

    public void ShowMainMenu()
    {
        SetPanels(menu: true, hud: false, gameOver: false, win: false);
    }

    public void ShowPlaying()
    {
        SetPanels(menu: false, hud: true, gameOver: false, win: false);
    }

    public void ShowGameOver()
    {
        SetPanels(menu: false, hud: false, gameOver: true, win: false);
    }

    public void ShowWin(float timeSeconds, string levelKey, int place)
    {
        winTimeText.text = "TIME  " + Leaderboard.FormatTime(timeSeconds);

        var times = Leaderboard.GetTimes(levelKey);
        for (int i = 0; i < leaderboardRows.Length; i++)
        {
            bool hasTime = i < times.Count;
            string value = hasTime ? Leaderboard.FormatTime(times[i]) : "--:--.--";
            leaderboardRows[i].text = (i + 1) + ".   " + value;

            if (i == place)
            {
                leaderboardRows[i].color = GoldColor;
            }
            else
            {
                leaderboardRows[i].color = hasTime ? Color.white : new Color(0.5f, 0.5f, 0.5f);
            }
        }

        SetPanels(menu: false, hud: false, gameOver: false, win: true);
    }

    private void SetPanels(bool menu, bool hud, bool gameOver, bool win)
    {
        mainMenuPanel.SetActive(menu);
        hudPanel.SetActive(hud);
        gameOverPanel.SetActive(gameOver);
        winPanel.SetActive(win);
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

    private GameObject BuildWin(Transform parent)
    {
        var panel = CreatePanel("Win Panel", parent, new Color(0.02f, 0.06f, 0.04f, 0.92f));

        CreateText(panel.transform, "YOU WIN!", 130, new Vector2(0, 380), WinColor);
        winTimeText = CreateText(panel.transform, "TIME  00:00.00", 80, new Vector2(0, 265), Color.white).GetComponent<Text>();
        CreateText(panel.transform, "BEST TIMES", 50, new Vector2(0, 165), GoldColor);

        leaderboardRows = new Text[Leaderboard.MaxEntries];
        for (int i = 0; i < leaderboardRows.Length; i++)
        {
            var row = CreateText(panel.transform, "", 40, new Vector2(0, 100 - i * 52), Color.white);
            leaderboardRows[i] = row.GetComponent<Text>();
        }

        CreateButton(panel.transform, "Restart", new Vector2(0, -230), GameManager.RequestRestart);
        CreateButton(panel.transform, "Main Menu", new Vector2(0, -340), GameManager.RequestMainMenu);

        return panel;
    }

    private void BuildHud(Transform parent)
    {
        hudPanel = new GameObject("HUD Panel", typeof(RectTransform));
        hudPanel.transform.SetParent(parent, false);
        Stretch(hudPanel.GetComponent<RectTransform>());

        // Health bar background (bigger than before for readability)
        var barBg = new GameObject("HealthBar Background", typeof(RectTransform), typeof(Image));
        barBg.transform.SetParent(hudPanel.transform, false);
        var bgRect = barBg.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0f, 1f);
        bgRect.anchorMax = new Vector2(0f, 1f);
        bgRect.pivot = new Vector2(0f, 1f);
        bgRect.anchoredPosition = new Vector2(40, -40);
        bgRect.sizeDelta = new Vector2(520, 56);
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
        healthBarFill = fillImage;
        SetFillFraction(healthBarFill, 1f);

        // HP label
        var label = CreateText(barBg.transform, "HP", 32, new Vector2(-60, 0), Color.white);
        var labelRect = label.GetComponent<RectTransform>();
        labelRect.anchorMin = labelRect.anchorMax = new Vector2(0f, 0.5f);
        labelRect.pivot = new Vector2(1f, 0.5f);
        labelRect.anchoredPosition = new Vector2(-12, 0);
        label.GetComponent<Text>().alignment = TextAnchor.MiddleRight;

        // Win timer, shown under the health bar on levels that use it
        var timerGO = CreateText(hudPanel.transform, "TIME  00:00.00", 44, Vector2.zero, Color.white);
        var timerRect = timerGO.GetComponent<RectTransform>();
        timerRect.anchorMin = timerRect.anchorMax = timerRect.pivot = new Vector2(0f, 1f);
        timerRect.anchoredPosition = new Vector2(40, -110);
        timerRect.sizeDelta = new Vector2(600, 70);
        timerText = timerGO.GetComponent<Text>();
        timerText.alignment = TextAnchor.MiddleLeft;
        timerGO.SetActive(false);

        // Boss health bar. Shown only while a Masked enemy exists in the scene.
        bossHealthBarRoot = new GameObject("BossHealthBar Root", typeof(RectTransform));
        bossHealthBarRoot.transform.SetParent(hudPanel.transform, false);
        var bossRootRect = bossHealthBarRoot.GetComponent<RectTransform>();
        bossRootRect.anchorMin = new Vector2(0.5f, 1f);
        bossRootRect.anchorMax = new Vector2(0.5f, 1f);
        bossRootRect.pivot = new Vector2(0.5f, 1f);
        bossRootRect.anchoredPosition = new Vector2(0f, -40f);
        bossRootRect.sizeDelta = new Vector2(720f, 48f);

        var bossLabel = CreateText(bossHealthBarRoot.transform, "BOSS", 28, new Vector2(-300, 0), Color.white);
        var bossLabelRect = bossLabel.GetComponent<RectTransform>();
        bossLabelRect.anchorMin = bossLabelRect.anchorMax = new Vector2(0f, 0.5f);
        bossLabelRect.pivot = new Vector2(0.5f, 0.5f);
        bossLabelRect.anchoredPosition = new Vector2(- (720f / 2f) + 50f, 0f);
        bossLabel.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;

        var bossBg = new GameObject("BossHealthBar Background", typeof(RectTransform), typeof(Image));
        bossBg.transform.SetParent(bossHealthBarRoot.transform, false);
        var bossBgRect = bossBg.GetComponent<RectTransform>();
        bossBgRect.anchorMin = new Vector2(0f, 0f);
        bossBgRect.anchorMax = new Vector2(1f, 1f);
        bossBgRect.offsetMin = new Vector2(90f, 0f);
        bossBgRect.offsetMax = Vector2.zero;
        bossBg.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.5f);

        var bossFill = new GameObject("BossHealthBar Fill", typeof(RectTransform), typeof(Image));
        bossFill.transform.SetParent(bossBg.transform, false);
        var bossFillRect = bossFill.GetComponent<RectTransform>();
        bossFillRect.anchorMin = Vector2.zero;
        bossFillRect.anchorMax = Vector2.one;
        bossFillRect.offsetMin = Vector2.zero;
        bossFillRect.offsetMax = Vector2.zero;
        var bossFillImage = bossFill.GetComponent<Image>();
        bossFillImage.color = new Color(0.9f, 0.2f, 0.2f);
        bossHealthBarFill = bossFillImage;
        SetFillFraction(bossHealthBarFill, 1f);

        bossHealthBarRoot.SetActive(false);

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

    // Sizes a bar fill by moving its right edge, so it works without a sprite.
    // (Image.fillAmount / Filled mode draws nothing when the Image has no sprite.)
    private static void SetFillFraction(Image fill, float fraction)
    {
        if (fill == null)
        {
            return;
        }
        RectTransform rt = fill.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = new Vector2(Mathf.Clamp01(fraction), 1f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
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
        if (FindAnyObjectByType<EventSystem>() != null)
        {
            return;
        }

        var es = new GameObject("EventSystem", typeof(EventSystem));
        es.AddComponent<InputSystemUIInputModule>();

        // The HUD canvas persists across scenes; its event system must persist too,
        // otherwise UI clicks (menus, restart, main menu) stop working after the
        // first level transition because the scene's EventSystem is destroyed.
        DontDestroyOnLoad(es);
    }
}

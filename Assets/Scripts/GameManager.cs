using UnityEngine;
using UnityEngine.SceneManagement;

// Controls the game flow: Main Menu -> Playing -> Game Over / Won.
// Created automatically when a scene loads, so no scene setup is required.
public class GameManager : MonoBehaviour
{
    public enum State { MainMenu, Playing, GameOver, Won }

    public static GameManager Instance { get; private set; }

    // True when gameplay should be running. Also true when no GameManager exists,
    // so the game still works if this script is removed.
    public static bool IsPlaying => Instance == null || Instance.state == State.Playing;

    public State CurrentState => state;

    // Time spent playing this level. Only counts while the game is in the Playing state.
    public float ElapsedTime { get; private set; }

    // Set by levels that use the win timer (shows the timer on the HUD).
    public bool TimerEnabled { get; private set; }

    // Static so it survives the scene reload used for Restart / Main Menu.
    private static bool showMenuOnNextLoad = true;

    private State state = State.MainMenu;
    private GameUI ui;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance != null)
        {
            return;
        }
        new GameObject("GameManager").AddComponent<GameManager>();
    }

    private void Awake()
    {
        // Enforce a single, persistent GameManager across scene loads so the HUD
        // (and the rest of the UI) survive level transitions.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        ui = gameObject.AddComponent<GameUI>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode != LoadSceneMode.Single)
        {
            return;
        }

        // A freshly loaded level starts from a clean run state.
        ElapsedTime = 0f;
        TimerEnabled = false;
        Time.timeScale = 1f;

        // Re-bind the UI to the (new) player in this scene.
        if (ui != null)
        {
            ui.ResetPlayerReference();
        }
        ResetPlayerForScene();

        if (showMenuOnNextLoad)
        {
            state = State.MainMenu;
            ShowMainMenu();
        }
        else
        {
            state = State.Playing;
            StartGame();
        }
    }

    // Re-attach / reset player health after a scene load, since the previous
    // player object was destroyed and a new one exists in the new scene.
    private void ResetPlayerForScene()
    {
        PlayerMovement player = FindAnyObjectByType<PlayerMovement>();
        if (player == null)
        {
            return;
        }

        Health health = player.GetComponent<Health>();
        if (health == null)
        {
            health = player.gameObject.AddComponent<Health>();
        }
        health.ResetHealth();
    }

    private void Start()
    {
        var player = FindFirstObjectByType<PlayerMovement>();
        if (player != null)
        {
            var health = player.GetComponent<Health>();
            if (health == null)
            {
                health = player.gameObject.AddComponent<Health>();
            }
            health.ResetHealth();
        }

        Time.timeScale = 1f;

        if (showMenuOnNextLoad)
        {
            ShowMainMenu();
        }
        else
        {
            StartGame();
        }
    }

    private void Update()
    {
        if (state == State.Playing)
        {
            ElapsedTime += Time.deltaTime;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
        Time.timeScale = 1f;
    }

    public static void RequestStartGame()
    {
        if (Instance != null)
        {
            Instance.StartGame();
        }
    }

    public static void RequestRestart()
    {
        showMenuOnNextLoad = false;
        ReloadLevelStatic();
    }

    public static void RequestMainMenu()
    {
        showMenuOnNextLoad = true;
        ReloadLevelStatic();
    }

    public static void RequestQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private static void ReloadLevelStatic()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ShowMainMenu()
    {
        state = State.MainMenu;
        Time.timeScale = 0f;
        ui?.ShowMainMenu();
    }

    public void StartGame()
    {
        // Starting gameplay means subsequent scene loads should also play,
        // not show the main menu.
        showMenuOnNextLoad = false;
        state = State.Playing;
        Time.timeScale = 1f;
        ui?.ShowPlaying();
    }

    public void EnableWinTimer()
    {
        TimerEnabled = true;
    }

    public void GameOver()
    {
        if (state != State.Playing)
        {
            return;
        }
        state = State.GameOver;
        Time.timeScale = 0f;
        ui?.ShowGameOver();
    }

    // Called when the level is completed. Saves the time to the device leaderboard
    // and shows the win screen.
    public void Win(string levelKey)
    {
        if (state != State.Playing)
        {
            return;
        }

        state = State.Won;
        Time.timeScale = 0f;

        int place = Leaderboard.Submit(levelKey, ElapsedTime);
        ui?.ShowWin(ElapsedTime, levelKey, place);
    }

    public void Restart()
    {
        RequestRestart();
    }

    public void ReturnToMainMenu()
    {
        RequestMainMenu();
    }

    public void QuitGame()
    {
        RequestQuit();
    }
}

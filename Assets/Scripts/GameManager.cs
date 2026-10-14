using UnityEngine;
using UnityEngine.SceneManagement;

// Controls the game flow: Main Menu -> Playing -> Game Over.
// Created automatically when a scene loads, so no scene setup is required.
public class GameManager : MonoBehaviour
{
    public enum State { MainMenu, Playing, GameOver }

    public static GameManager Instance { get; private set; }

    // True when gameplay should be running. Also true when no GameManager exists,
    // so the game still works if this script is removed.
    public static bool IsPlaying => Instance == null || Instance.state == State.Playing;

    public State CurrentState => state;

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
        Instance = this;
        ui = gameObject.AddComponent<GameUI>();
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
        state = State.Playing;
        Time.timeScale = 1f;
        ui?.ShowPlaying();
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

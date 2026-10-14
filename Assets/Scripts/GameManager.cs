using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioClip themeSong;
    [SerializeField] private float themeSongVolume = 0.5f;
    [SerializeField] private bool playThemeOnStart = true;

    public enum State { MainMenu, Playing, GameOver, Won }

    public static GameManager Instance { get; private set; }

    public static bool IsPlaying => Instance == null || Instance.state == State.Playing;

    public State CurrentState => state;

    public float ElapsedTime { get; private set; }

    public bool TimerEnabled { get; private set; }

    private static bool showMenuOnNextLoad = true;

    private State state = State.MainMenu;
    private GameUI ui;
    private AudioSource musicSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        ui = gameObject.AddComponent<GameUI>();

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.spatialBlend = 0.0f;
        musicSource.volume = themeSongVolume;

        if (themeSong != null && playThemeOnStart)
        {
            musicSource.clip = themeSong;
            musicSource.Play();
        }
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

        ElapsedTime = 0.0f;
        TimerEnabled = false;
        Time.timeScale = 1.0f;

        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            CameraFollow follow = mainCam.GetComponent<CameraFollow>();
            if (follow == null)
            {
                mainCam.gameObject.AddComponent<CameraFollow>();
            }
        }

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
        PlayerMovement player = FindAnyObjectByType<PlayerMovement>();
        if (player != null)
        {
            Health health = player.GetComponent<Health>();
            if (health == null)
            {
                health = player.gameObject.AddComponent<Health>();
            }
            health.ResetHealth();
        }

        Time.timeScale = 1.0f;

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
        Time.timeScale = 1.0f;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance != null)
        {
            return;
        }
        new GameObject("GameManager").AddComponent<GameManager>();
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
        Application.Quit();
    }

    private static void ReloadLevelStatic()
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ShowMainMenu()
    {
        state = State.MainMenu;
        Time.timeScale = 0.0f;
        ui?.ShowMainMenu();
    }

    public void StartGame()
    {
        showMenuOnNextLoad = false;
        state = State.Playing;
        Time.timeScale = 1.0f;
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
        Time.timeScale = 0.0f;
        ui?.ShowGameOver();
    }

    public void Win(string levelKey)
    {
        if (state != State.Playing)
        {
            return;
        }

        state = State.Won;
        Time.timeScale = 0.0f;

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
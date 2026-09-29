using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

// Owns the game state machine, score, lives and the per-life timer.
public class GameManager : MonoBehaviour
{
    private enum GameState
    {
        Title,
        Playing,
        LifeLost,
        RoundClear,
        GameOver
    }

    private const string HighScoreKey = "HighScore";

    [SerializeField] private GameConfig config;

    private PlayerController _player;
    private HomeRow _homeRow;
    private LaneController[] _lanes;
    private UIManager _ui;
    private AudioManager _audio;
    private ScreenShake _screenShake;

    private GameState _state;
    private int _score;
    private int _lives;
    private int _time;
    private int _highScore;
    private int _round;
    private float _gameOverTime;
    private Coroutine _timerRoutine;

    public GameConfig Config => config;

    private void Awake()
    {
        _homeRow = new HomeRow(FindObjectsByType<Home>(FindObjectsSortMode.None));
        _player = FindAnyObjectByType<PlayerController>();
        _lanes = FindObjectsByType<LaneController>(FindObjectsSortMode.None);
        _ui = GetComponent<UIManager>();
        _audio = GetComponent<AudioManager>();

        Camera mainCamera = Camera.main;
        _screenShake = mainCamera.GetComponent<ScreenShake>();
        if (_screenShake == null)
        {
            _screenShake = mainCamera.gameObject.AddComponent<ScreenShake>();
        }

        _highScore = PlayerPrefs.GetInt(HighScoreKey, 0);
    }

    private void Start()
    {
        EnterTitle();
    }

    private void Update()
    {
        switch (_state)
        {
            case GameState.Title:
                HandleMenuInput();
                break;
            case GameState.GameOver:
                if (Time.unscaledTime - _gameOverTime >= config.restartLockout)
                {
                    HandleMenuInput();
                }
                break;
        }
    }

    private void HandleMenuInput()
    {
        if (StartPressed())
        {
            NewGame();
        }
        else if (QuitPressed())
        {
            Quit();
        }
    }

    private static bool StartPressed()
    {
        Keyboard keyboard = Keyboard.current;
        Gamepad gamepad = Gamepad.current;
        return (keyboard != null && (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame)) ||
               (gamepad != null && gamepad.buttonSouth.wasPressedThisFrame);
    }

    private static bool QuitPressed()
    {
        Keyboard keyboard = Keyboard.current;
        return keyboard != null && keyboard.escapeKey.wasPressedThisFrame;
    }

    private static void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void EnterTitle()
    {
        _state = GameState.Title;
        Time.timeScale = 1f;
        _player.gameObject.SetActive(false);
        _homeRow.Clear();
        SetScore(0);
        SetLives(config.startingLives);
        SetTime(config.perLifeTimer);
        _ui.ShowTitle(_highScore);
        _audio.PlayTitleMusic();
    }

    private void NewGame()
    {
        Time.timeScale = 1f;
        _round = 0;
        SetLaneSpeedMultiplier(1f);
        SetScore(0);
        SetLives(config.startingLives);
        _ui.HideOverlays();
        _audio.PlayGameplayMusic();
        _homeRow.Clear();
        Respawn();
    }

    private void Respawn()
    {
        _state = GameState.Playing;
        _player.Respawn();
        StopTimer();
        _timerRoutine = StartCoroutine(Timer());
    }

    private IEnumerator Timer()
    {
        SetTime(config.perLifeTimer);
        while (_time > 0)
        {
            yield return new WaitForSeconds(1);
            SetTime(_time - 1);
        }
        _player.Death();
    }

    // Also silences the low-time alarm: death, reaching a home and game over all end the countdown.
    private void StopTimer()
    {
        if (_timerRoutine != null)
        {
            StopCoroutine(_timerRoutine);
            _timerRoutine = null;
        }
        _audio.StopTimerLow();
    }

    public void Died(bool drowned)
    {
        if (_state != GameState.Playing) return;

        _state = GameState.LifeLost;
        StopTimer();
        SetLives(_lives - 1);
        _audio.PlayDeath(drowned);
        _screenShake.Shake();
        StartCoroutine(AfterDeath());
    }

    private IEnumerator AfterDeath()
    {
        yield return new WaitForSeconds(config.deathAnimDuration);

        if (_lives > 0)
        {
            Respawn();
        }
        else
        {
            GameOver();
        }
    }

    private void GameOver()
    {
        _state = GameState.GameOver;
        _gameOverTime = Time.unscaledTime;
        Time.timeScale = 0f;

        if (_score > _highScore)
        {
            _highScore = _score;
            PlayerPrefs.SetInt(HighScoreKey, _highScore);
            PlayerPrefs.Save();
        }

        _ui.ShowGameOver(_highScore);
        _audio.StopMusic();
        _audio.PlayGameOver();
    }

    public void HomeHasBeenOccupied()
    {
        if (_state != GameState.Playing) return;

        StopTimer();
        _player.gameObject.SetActive(false);
        _audio.PlayHomeFilled();
        SetScore(_score + config.homeScore + _time * config.timeBonusPerSecond);

        if (_homeRow.IsFull)
        {
            StartCoroutine(RoundClear());
        }
        else
        {
            StartCoroutine(RespawnAfterHome());
        }
    }

    // The frog must be gone for a moment before it respawns, or the home it just
    // filled sees it again and counts it as jumping into an occupied slot.
    private IEnumerator RespawnAfterHome()
    {
        yield return new WaitForSeconds(config.homeRespawnDelay);
        Respawn();
    }

    private IEnumerator RoundClear()
    {
        _state = GameState.RoundClear;
        SetScore(_score + config.roundClearBonus);
        _audio.PlayRoundClear();

        yield return _homeRow.Flash(config.roundClearFlashDuration);

        _homeRow.Clear();
        _round++;
        SetLaneSpeedMultiplier(Mathf.Pow(config.roundSpeedMultiplier, _round));
        Respawn();
    }

    public void AdvancedRow()
    {
        if (_state != GameState.Playing) return;

        SetScore(_score + config.rowScore);
    }

    private void SetLaneSpeedMultiplier(float multiplier)
    {
        foreach (LaneController lane in _lanes)
        {
            lane.SetSpeedMultiplier(multiplier);
        }
    }

    private void SetScore(int score)
    {
        _score = score;
        _ui.SetScore(_score);
    }

    private void SetLives(int lives)
    {
        _lives = lives;
        _ui.SetLives(_lives);
    }

    private void SetTime(int time)
    {
        _time = time;
        _ui.SetTime(_time, _time <= config.timerWarningSeconds);

        if (_state == GameState.Playing && _time == config.timerWarningSeconds)
        {
            _audio.PlayTimerLow();
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Draws score, timer, lives and the title / game-over overlays.
public class UIManager : MonoBehaviour
{
    private const float PromptBlinkPeriod = 1f;

    [Header("HUD")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private Image[] lifeIcons;
    [SerializeField] private Color timerWarningColor = Color.red;

    [Header("Overlays")]
    [SerializeField] private GameObject gameOverMenu;
    [SerializeField] private GameObject gameOverPrompt;
    [SerializeField] private TMP_Text gameOverHighScoreText;
    [SerializeField] private GameObject titleMenu;
    [SerializeField] private GameObject titlePrompt;
    [SerializeField] private TMP_Text highScoreText;

    [Header("Game over flash")]
    [Tooltip("Score and HI-SCORE cycle through these together on the game-over screen.")]
    [SerializeField] private Color[] gameOverColors =
    {
        Color.white, Color.yellow, Color.red, Color.cyan, Color.magenta, Color.green
    };
    [SerializeField] private float gameOverColorInterval = 0.1f;

    private Color _timeColor;
    private Color _scoreColor;
    private Color _gameOverHighScoreColor;
    private bool _isTitleShown;
    private bool _isGameOverShown;

    // Unscaled, because the board is frozen with timeScale 0 on game over.
    private static bool IsBlinkOn => Mathf.Repeat(Time.unscaledTime, PromptBlinkPeriod) < PromptBlinkPeriod / 2f;

    private void Awake()
    {
        _timeColor = timeText.color;
        _scoreColor = scoreText.color;
        if (gameOverHighScoreText != null)
        {
            _gameOverHighScoreColor = gameOverHighScoreText.color;
        }
    }

    private void Update()
    {
        if (_isTitleShown && titlePrompt != null)
        {
            titlePrompt.SetActive(IsBlinkOn);
        }

        if (_isGameOverShown)
        {
            if (gameOverPrompt != null)
            {
                gameOverPrompt.SetActive(IsBlinkOn);
            }
            FlashGameOverColors();
        }
    }

    private void FlashGameOverColors()
    {
        if (gameOverColors.Length == 0) return;

        int index = (int)(Time.unscaledTime / gameOverColorInterval) % gameOverColors.Length;
        Color color = gameOverColors[index];
        scoreText.color = color;
        if (gameOverHighScoreText != null)
        {
            gameOverHighScoreText.color = color;
        }
    }

    public void SetScore(int score)
    {
        scoreText.text = score.ToString();
    }

    public void SetTime(int time, bool isLow)
    {
        timeText.text = time.ToString();
        timeText.color = isLow ? timerWarningColor : _timeColor;
    }

    // One frog icon per life, placed in the scene. Icons are hidden from the end of the list first.
    public void SetLives(int lives)
    {
        for (int i = 0; i < lifeIcons.Length; i++)
        {
            lifeIcons[i].enabled = i < lives;
        }
    }

    public void ShowTitle(int highScore)
    {
        _isTitleShown = true;
        _isGameOverShown = false;
        gameOverMenu.SetActive(false);

        if (titleMenu != null)
        {
            titleMenu.SetActive(true);
        }
        if (highScoreText != null)
        {
            highScoreText.text = $"HI-SCORE {highScore}";
        }
    }

    public void ShowGameOver(int highScore)
    {
        _isGameOverShown = true;
        gameOverMenu.SetActive(true);

        if (gameOverHighScoreText != null)
        {
            gameOverHighScoreText.text = $"HI-SCORE {highScore}";
        }
    }

    public void HideOverlays()
    {
        _isTitleShown = false;
        _isGameOverShown = false;
        gameOverMenu.SetActive(false);
        scoreText.color = _scoreColor;
        if (gameOverHighScoreText != null)
        {
            gameOverHighScoreText.color = _gameOverHighScoreColor;
        }

        if (titleMenu != null)
        {
            titleMenu.SetActive(false);
        }
    }
}

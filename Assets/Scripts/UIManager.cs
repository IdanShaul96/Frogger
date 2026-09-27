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
    [SerializeField] private TMP_Text gameOverHighScoreText;
    [SerializeField] private GameObject titleMenu;
    [SerializeField] private GameObject titlePrompt;
    [SerializeField] private TMP_Text highScoreText;

    private Color _timeColor;
    private bool _isTitleShown;

    private void Awake()
    {
        _timeColor = timeText.color;
    }

    private void Update()
    {
        if (_isTitleShown && titlePrompt != null)
        {
            titlePrompt.SetActive(Mathf.Repeat(Time.unscaledTime, PromptBlinkPeriod) < PromptBlinkPeriod / 2f);
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
        gameOverMenu.SetActive(true);

        if (gameOverHighScoreText != null)
        {
            gameOverHighScoreText.text = $"HI-SCORE {highScore}";
        }
    }

    public void HideOverlays()
    {
        _isTitleShown = false;
        gameOverMenu.SetActive(false);

        if (titleMenu != null)
        {
            titleMenu.SetActive(false);
        }
    }
}

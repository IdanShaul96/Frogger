using UnityEngine;
using System.Collections;
using TMPro;
public class GameManager : MonoBehaviour
{
    private Frogger _frogger;
    private Home[] _homes;
    private int _score;
    private int _lives;
    private int _time;
    public GameObject GameOverMenu;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text livesText;
    private void Awake()
    {
        _homes = FindObjectsByType<Home>(FindObjectsSortMode.None);
        _frogger = FindAnyObjectByType<Frogger>();
    }
    
    private void Start()
    {
        NewGame();
    }

    private void NewGame()
    {
        GameOverMenu.SetActive(false);
        SetScore(0);
        SetLives(3);
        NewLevel();
    }
    private void NewLevel()
    {
        foreach (Home home in _homes)
        {
            home.enabled = false;
        }
        Respawn();
    }

    private void Respawn()
    {
        _frogger.Respawn();
        StopAllCoroutines();
        StartCoroutine(Timer(30));
    }

    private IEnumerator Timer(int duration)
    {
        SetTime(duration);
        while (_time > 0)
        {
            yield return new WaitForSeconds(1);
            SetTime(_time - 1);
        }
        _frogger.Death();
    }

    public void Died()
    {
        StopAllCoroutines();
        SetLives(_lives - 1);

        if (_lives > 0)
        {
            Invoke(nameof(Respawn), 1f);
        }
        else
        {
            Invoke(nameof(GameOver), 1f);
        }
    }

    private void GameOver()
    {   
        _frogger.gameObject.SetActive(false);
        GameOverMenu.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(Retry());
    }

    private IEnumerator Retry()
    {
        bool playingAgain = false;
        while (!playingAgain)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                playingAgain = true;
            }
            
            yield return null;
        }
        NewGame();
    }
    public void HomeHasBeenOccupied()
    {
        var extraPointsForTime = _time * 20;
        StopAllCoroutines();
        _frogger.gameObject.SetActive(false);
        SetScore((_score + 50 + extraPointsForTime));

        if (LevelCleared())
        {
            SetScore((_score + 500));
            Invoke(nameof(NewLevel),1f);
        }
        else
        {
            Invoke(nameof(Respawn),1f);
        }
    }

    private bool LevelCleared()
    {
        foreach (Home home in _homes)
        {
            if (!home.enabled)
            {
                return false;
            }
        }

        return true;
    }
    private void SetScore(int score)
    {
        _score = score;
        scoreText.text = _score.ToString();
    }
    private void SetLives(int lives)
    {
        _lives = lives;
        livesText.text = _lives.ToString();
    }
    private void SetTime(int time)
    {
        _time = time;
        timeText.text = _time.ToString();
    }

    public void AdvancedRow()
    {
        SetScore(_score + 10);
    }
}

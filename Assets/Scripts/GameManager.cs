using UnityEngine;
using System.Collections;
public class GameManager : MonoBehaviour
{
    private Frogger _frogger;
    private Home[] _homes;
    private int _score;
    private int _lives;
    private int _time;
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
        NewRound();
    }
    private void NewRound()
    {
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
        _time = duration;
        while (_time > 0)
        {
            yield return new WaitForSeconds(1);
            _time--;
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
            Invoke(nameof(NewRound),1f);
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
    }
    private void SetLives(int lives)
    {
        _lives = lives;
    }

    public void AdvancedRow()
    {
        SetScore(_score + 10);
    }
}

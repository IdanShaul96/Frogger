using System.Collections;
using UnityEngine;

// Tracks the five home slots at the top of the board.
public class HomeRow
{
    private const float FlashInterval = 0.15f;

    private readonly Home[] _homes;

    public HomeRow(Home[] homes)
    {
        _homes = homes;
    }

    public bool IsFull
    {
        get
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
    }

    public void Clear()
    {
        foreach (Home home in _homes)
        {
            home.enabled = false;
        }
    }

    public IEnumerator Flash(float duration)
    {
        bool visible = true;
        for (float elapsed = 0f; elapsed < duration; elapsed += FlashInterval)
        {
            visible = !visible;
            foreach (Home home in _homes)
            {
                home.frogPicture.SetActive(visible);
            }
            yield return new WaitForSeconds(FlashInterval);
        }
    }
}

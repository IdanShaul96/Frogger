using UnityEngine;
using System.Collections;

// Drives one turtle group's surfaced -> warning -> submerged cycle.
// Put it on a turtle group (Turtles2 / Turtles3) next to its Rideable.
[RequireComponent(typeof(Rideable))]
public class TurtleDiver : MonoBehaviour
{
    [SerializeField] private Sprite[] swimSprites;
    [SerializeField] private Sprite[] diveSprites;
    [SerializeField] private float frameDuration = 0.25f;
    [SerializeField] private float safeTime = 5f;
    [SerializeField] private float warningTime = 1.5f;
    [SerializeField] private float diveTime = 2f;
    [SerializeField] private float startDelay;

    private SpriteRenderer[] _renderers;
    private Rideable _rideable;

    private void Awake()
    {
        _renderers = GetComponentsInChildren<SpriteRenderer>();
        _rideable = GetComponent<Rideable>();
    }

    private void Start()
    {
        StartCoroutine(DiveCycle());
    }

    private IEnumerator DiveCycle()
    {
        yield return new WaitForSeconds(startDelay);

        while (true)
        {
            // Surfaced and safe
            int frame = 0;
            for (float elapsed = 0f; elapsed < safeTime; elapsed += frameDuration)
            {
                SetSprite(swimSprites[frame % swimSprites.Length]);
                frame++;
                yield return new WaitForSeconds(frameDuration);
            }

            // Warning: half-submerged, still safe to stand on
            float warningFrameDuration = warningTime / diveSprites.Length;
            foreach (Sprite sprite in diveSprites)
            {
                SetSprite(sprite);
                yield return new WaitForSeconds(warningFrameDuration);
            }

            // Fully submerged: this group counts as open water
            SetSubmerged(true);
            yield return new WaitForSeconds(diveTime);
            SetSubmerged(false);

            // Resurface: dive frames in reverse, already safe
            for (int i = diveSprites.Length - 1; i >= 0; i--)
            {
                SetSprite(diveSprites[i]);
                yield return new WaitForSeconds(frameDuration);
            }
        }
    }

    private void SetSprite(Sprite sprite)
    {
        foreach (SpriteRenderer spriteRenderer in _renderers)
        {
            spriteRenderer.sprite = sprite;
        }
    }

    private void SetSubmerged(bool submerged)
    {
        _rideable.IsSafe = !submerged;

        foreach (SpriteRenderer spriteRenderer in _renderers)
        {
            spriteRenderer.enabled = !submerged;
        }
    }
}

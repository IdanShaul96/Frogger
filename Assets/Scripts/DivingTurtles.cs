using UnityEngine;
using System.Collections;

// Put this on a turtle group (Turtles2 / Turtles3) that should dive.
// All turtles in the group dive and surface together.
public class DivingTurtles : MonoBehaviour
{
    [SerializeField] private Sprite[] swimSprites;
    [SerializeField] private Sprite[] diveSprites;
    [SerializeField] private float frameDuration = 0.25f;
    [SerializeField] private float swimDuration = 4f;
    [SerializeField] private float underwaterDuration = 1.5f;
    [SerializeField] private float startDelay;

    private SpriteRenderer[] _renderers;
    private Collider2D[] _colliders;
    private Frogger _frogger;
    private bool _isUnderwater;

    private void Awake()
    {
        _renderers = GetComponentsInChildren<SpriteRenderer>();
        _colliders = GetComponentsInChildren<Collider2D>();
        _frogger = FindAnyObjectByType<Frogger>();
    }

    private void Start()
    {
        StartCoroutine(DiveCycle());
    }

    private void Update()
    {
        if (_isUnderwater && IsFrogOnTurtles())
        {
            _frogger.Death();
        }
    }

    private bool IsFrogOnTurtles()
    {
        return _frogger.enabled &&
               _frogger.transform.parent != null &&
               _frogger.transform.IsChildOf(transform);
    }

    private IEnumerator DiveCycle()
    {
        yield return new WaitForSeconds(startDelay);

        while (true)
        {
            // Swim on the surface
            float elapsed = 0f;
            int frame = 0;
            while (elapsed < swimDuration)
            {
                SetSprite(swimSprites[frame % swimSprites.Length]);
                frame++;
                yield return new WaitForSeconds(frameDuration);
                elapsed += frameDuration;
            }

            // Dive, the frog can still stand on them
            foreach (Sprite sprite in diveSprites)
            {
                SetSprite(sprite);
                yield return new WaitForSeconds(frameDuration);
            }

            // Underwater, not a platform anymore
            SetUnderwater(true);
            yield return new WaitForSeconds(underwaterDuration);
            SetUnderwater(false);

            // Surface, dive frames in reverse
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

    private void SetUnderwater(bool underwater)
    {
        _isUnderwater = underwater;

        foreach (SpriteRenderer spriteRenderer in _renderers)
        {
            spriteRenderer.enabled = !underwater;
        }

        foreach (Collider2D turtleCollider in _colliders)
        {
            turtleCollider.enabled = !underwater;
        }
    }
}

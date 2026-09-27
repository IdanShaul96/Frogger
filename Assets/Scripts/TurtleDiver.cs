using UnityEngine;

// Drives one turtle group's surfaced -> warning -> submerged cycle.
// The timings come from GameConfig; the LaneController that owns the group sets its start delay.
// Driven by a clock rather than a coroutine, so the cycle survives the group being recycled by the pool.
[RequireComponent(typeof(Rideable))]
public class TurtleDiver : MonoBehaviour
{
    [SerializeField] private Sprite[] swimSprites;
    [SerializeField] private Sprite[] diveSprites;
    [SerializeField] private float frameDuration = 0.25f;

    private SpriteRenderer[] _renderers;
    private Rideable _rideable;
    private GameConfig _config;
    private float _startDelay;
    private float _clock;
    private bool _isSubmerged;

    private void Awake()
    {
        _renderers = GetComponentsInChildren<SpriteRenderer>();
        _rideable = GetComponent<Rideable>();
    }

    public void Configure(GameConfig config, float startDelay)
    {
        _config = config;
        _startDelay = startDelay;
        _clock = 0f;
    }

    private void Update()
    {
        if (_config == null) return;

        _clock += Time.deltaTime;
        float time = _clock - _startDelay;
        if (time < 0f)
        {
            Swim();
            return;
        }

        float resurfaceTime = diveSprites.Length * frameDuration;
        float cycle = _config.turtleSafeTime + _config.turtleWarningTime + _config.turtleDiveTime + resurfaceTime;
        time %= cycle;

        // Surfaced and safe
        if (time < _config.turtleSafeTime)
        {
            Swim();
            return;
        }
        time -= _config.turtleSafeTime;

        // Warning: half-submerged, still safe to stand on
        if (time < _config.turtleWarningTime)
        {
            float warningFrameDuration = _config.turtleWarningTime / diveSprites.Length;
            SetSubmerged(false);
            SetSprite(diveSprites[Frame(time, warningFrameDuration)]);
            return;
        }
        time -= _config.turtleWarningTime;

        // Fully submerged: this group counts as open water
        if (time < _config.turtleDiveTime)
        {
            SetSubmerged(true);
            return;
        }
        time -= _config.turtleDiveTime;

        // Resurface: dive frames in reverse, already safe
        SetSubmerged(false);
        SetSprite(diveSprites[diveSprites.Length - 1 - Frame(time, frameDuration)]);
    }

    private void Swim()
    {
        SetSubmerged(false);
        SetSprite(swimSprites[(int)(_clock / frameDuration) % swimSprites.Length]);
    }

    private int Frame(float time, float duration)
    {
        return Mathf.Min((int)(time / duration), diveSprites.Length - 1);
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
        if (submerged == _isSubmerged) return;

        _isSubmerged = submerged;
        _rideable.IsSafe = !submerged;

        foreach (SpriteRenderer spriteRenderer in _renderers)
        {
            spriteRenderer.enabled = !submerged;
        }
    }
}

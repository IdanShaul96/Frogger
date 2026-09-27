using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

// Reads input, performs the grid-snapped hop, and reports what the frog collided with.
public class PlayerController : MonoBehaviour
{
    private static readonly Vector2 PlatformCheckSize = new Vector2(0.25f, 0.25f);

    public Sprite idleSprite;
    public Sprite leapSprite;
    public Sprite deathSprite;

    private SpriteRenderer _spriteRenderer;
    private GameManager _gameManager;
    private AudioManager _audio;
    private GameConfig _config;
    private int _obstacleLayer;
    private int _barrierMask;
    private int _platformMask;
    private int _obstacleMask;

    private Vector3 _spawnPoint;
    private float _farthestRow;

    private bool _isLeaping;
    private bool _isHoppingHome;
    private Rideable _rideable;
    private Vector3 _bufferedDirection;
    private bool _hasBufferedInput;
    private bool _stickWasDeflected;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _gameManager = FindAnyObjectByType<GameManager>();
        _audio = FindAnyObjectByType<AudioManager>();
        _config = _gameManager.Config;

        _obstacleLayer = LayerMask.NameToLayer("Obstacle");
        _barrierMask = LayerMask.GetMask("Barrier");
        _platformMask = LayerMask.GetMask("Platform");
        _obstacleMask = LayerMask.GetMask("Obstacle");

        _spawnPoint = transform.position;
    }

    private void Update()
    {
        ReadInput();

        if (_rideable == null) return;

        if (!_rideable.IsSafe || IsOffScreen())
        {
            Death(true);
            return;
        }

        if (!_isLeaping)
        {
            transform.position += _rideable.Velocity * Time.deltaTime;
        }
    }

    // Input is read on press in Update and applied here, so no press is lost between physics steps.
    private void FixedUpdate()
    {
        if (_isLeaping || !_hasBufferedInput) return;

        _hasBufferedInput = false;
        Hop(_bufferedDirection);
    }

    private void ReadInput()
    {
        Vector3 direction = ReadKeyboardDirection();
        Vector3 stickDirection = ReadGamepadDirection();
        if (direction == Vector3.zero)
        {
            direction = stickDirection;
        }

        // Buffered for one hop only: a press during a full buffer is dropped, never queued.
        if (direction == Vector3.zero || _hasBufferedInput) return;

        _bufferedDirection = direction;
        _hasBufferedInput = true;
    }

    private static Vector3 ReadKeyboardDirection()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return Vector3.zero;

        return ToDirection(
            keyboard.wKey.wasPressedThisFrame || keyboard.upArrowKey.wasPressedThisFrame,
            keyboard.sKey.wasPressedThisFrame || keyboard.downArrowKey.wasPressedThisFrame,
            keyboard.aKey.wasPressedThisFrame || keyboard.leftArrowKey.wasPressedThisFrame,
            keyboard.dKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame);
    }

    private Vector3 ReadGamepadDirection()
    {
        Gamepad gamepad = Gamepad.current;
        if (gamepad == null) return Vector3.zero;

        Vector3 dpadDirection = ToDirection(
            gamepad.dpad.up.wasPressedThisFrame,
            gamepad.dpad.down.wasPressedThisFrame,
            gamepad.dpad.left.wasPressedThisFrame,
            gamepad.dpad.right.wasPressedThisFrame);

        // One hop per flick of the stick past the deadzone.
        Vector2 stick = gamepad.leftStick.ReadValue();
        bool isDeflected = stick.magnitude > _config.stickDeadzone;
        bool isFlick = isDeflected && !_stickWasDeflected;
        _stickWasDeflected = isDeflected;

        if (dpadDirection != Vector3.zero || !isFlick) return dpadDirection;

        if (Mathf.Abs(stick.x) > Mathf.Abs(stick.y))
        {
            return stick.x > 0f ? Vector3.right : Vector3.left;
        }
        return stick.y > 0f ? Vector3.up : Vector3.down;
    }

    // Opposite directions pressed together cancel out; there are no diagonal hops.
    private static Vector3 ToDirection(bool up, bool down, bool left, bool right)
    {
        if (up && down)
        {
            up = false;
            down = false;
        }
        if (left && right)
        {
            left = false;
            right = false;
        }

        if (up) return Vector3.up;
        if (down) return Vector3.down;
        if (left) return Vector3.left;
        if (right) return Vector3.right;
        return Vector3.zero;
    }

    private void Hop(Vector3 direction)
    {
        transform.rotation = RotationFor(direction);

        Vector3 destination = transform.position + direction * _config.cellSize;
        Collider2D barrier = Physics2D.OverlapBox(destination, Vector2.zero, 0f, _barrierMask);
        Collider2D platform = Physics2D.OverlapBox(destination, PlatformCheckSize, 0f, _platformMask);
        Collider2D obstacle = Physics2D.OverlapBox(destination, Vector2.zero, 0f, _obstacleMask);

        if (barrier != null) return;

        // The frog counts as being in the destination cell from the start of the hop.
        _rideable = platform != null ? platform.GetComponentInParent<Rideable>() : null;
        bool isOnSafePlatform = _rideable != null && _rideable.IsSafe;

        // The home slots sit inside the water collider; Home decides what happens there.
        _isHoppingHome = IsHomeSlot(destination);

        if (obstacle != null && !isOnSafePlatform && !_isHoppingHome)
        {
            _rideable = null;
            transform.position = destination;
            Death(IsWater(obstacle));
            return;
        }

        if (destination.y > _farthestRow)
        {
            _farthestRow = destination.y;
            _gameManager.AdvancedRow();
        }

        _audio.PlayHop();
        StartCoroutine(Leap(destination));
    }

    private static bool IsHomeSlot(Vector3 position)
    {
        foreach (Collider2D hit in Physics2D.OverlapBoxAll(position, PlatformCheckSize, 0f))
        {
            if (hit.GetComponent<Home>() != null)
            {
                return true;
            }
        }
        return false;
    }

    private static Quaternion RotationFor(Vector3 direction)
    {
        if (direction == Vector3.down) return Quaternion.Euler(0, 0, 180);
        if (direction == Vector3.left) return Quaternion.Euler(0, 0, 90);
        if (direction == Vector3.right) return Quaternion.Euler(0, 0, -90);
        return Quaternion.identity;
    }

    private IEnumerator Leap(Vector3 destination)
    {
        _isLeaping = true;
        _spriteRenderer.sprite = leapSprite;

        Vector3 startPosition = transform.position;
        Vector3 drift = Vector3.zero;
        float elapsed = 0f;
        while (elapsed < _config.hopDuration)
        {
            transform.position = Vector3.Lerp(startPosition, destination, elapsed / _config.hopDuration) + drift;
            yield return null;
            elapsed += Time.deltaTime;
            if (_rideable != null)
            {
                drift += _rideable.Velocity * Time.deltaTime;
            }
        }

        transform.position = destination + drift;
        _spriteRenderer.sprite = idleSprite;
        _isLeaping = false;
    }

    private bool IsOffScreen()
    {
        return transform.position.x < _config.BoardLeftX ||
               transform.position.x > _config.BoardRightX;
    }

    // Moving obstacles are vehicles; the only still obstacle is the water.
    private static bool IsWater(Collider2D obstacle)
    {
        return obstacle.GetComponentInParent<MovingPatterns>() == null;
    }

    public void Death(bool drowned = false)
    {
        if (!enabled) return;

        StopAllCoroutines();
        _isLeaping = false;
        _isHoppingHome = false;
        _rideable = null;
        _hasBufferedInput = false;
        transform.rotation = Quaternion.identity;
        _spriteRenderer.sprite = deathSprite;
        enabled = false;
        _gameManager.Died(drowned);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (enabled && other.gameObject.layer == _obstacleLayer && _rideable == null && !_isHoppingHome)
        {
            Death(IsWater(other));
        }
    }

    public void Respawn()
    {
        StopAllCoroutines();
        _isLeaping = false;
        _isHoppingHome = false;
        _rideable = null;
        _hasBufferedInput = false;
        transform.position = _spawnPoint;
        transform.rotation = Quaternion.identity;
        _farthestRow = _spawnPoint.y;
        _spriteRenderer.sprite = idleSprite;
        gameObject.SetActive(true);
        enabled = true;
    }
}

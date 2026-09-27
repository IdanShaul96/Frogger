using System.Collections.Generic;
using UnityEngine;

// Moves one lane's vehicles or platforms at a constant speed and recycles them through a LanePool:
// an object that leaves by the exit edge is handed back and the next one enters behind the last.
public class LaneController : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    [Tooltip("+1 scrolls right, -1 scrolls left.")]
    [SerializeField] private int direction = 1;
    [Tooltip("Cells per second.")]
    [SerializeField] private float speed = 1f;
    [Tooltip("Gap in cells between the end of one object and the start of the next.")]
    [SerializeField] private float spacing = 3f;
    [Tooltip("Centre x of the first object at the start of the game, in cells.")]
    [SerializeField] private float startOffset;
    [Tooltip("Turtle lanes only: dive start delay per group, in spawn order, so the lane never dives all at once.")]
    [SerializeField] private float[] diveStartDelays;

    private readonly Queue<Transform> _objects = new Queue<Transform>();
    private LanePool _pool;
    private Transform _trailing;
    // Extent of one object left and right of its pivot; logs and turtles are not centred on theirs.
    private float _minOffset;
    private float _maxOffset;
    private float _leftEdgeX;
    private float _rightEdgeX;
    private float _speedMultiplier = 1f;

    public Vector3 Velocity => Vector3.right * (direction * speed * _speedMultiplier);

    private float Period => _maxOffset - _minOffset + spacing;

    // Rounds get faster: GameManager scales every lane when all five homes are filled.
    public void SetSpeedMultiplier(float multiplier)
    {
        _speedMultiplier = multiplier;
    }

    private void Start()
    {
        // Wrap at the board edges, not the camera edges, so a wider screen never changes the lanes.
        GameConfig config = FindAnyObjectByType<GameManager>().Config;
        _leftEdgeX = config.BoardLeftX;
        _rightEdgeX = config.BoardRightX;
        MeasureExtents();

        // Every position that is on the board, plus one spare on each side.
        int first = Mathf.FloorToInt((_leftEdgeX - _maxOffset - Period - startOffset) / Period);
        int last = Mathf.CeilToInt((_rightEdgeX - _minOffset + Period - startOffset) / Period);
        int count = last - first + 1;

        _pool = new LanePool(prefab, transform, count);

        // Queue order is leading (nearest the exit edge) to trailing.
        for (int i = 0; i < count; i++)
        {
            int index = direction > 0 ? last - i : first + i;
            Transform instance = Spawn(startOffset + index * Period);
            ConfigureDiver(instance, index, config);
        }
    }

    private void Update()
    {
        Vector3 step = Velocity * Time.deltaTime;
        foreach (Transform instance in _objects)
        {
            instance.position += step;
        }

        Transform leading = _objects.Peek();
        if (!HasLeftBoard(leading)) return;

        _objects.Dequeue();
        _pool.Release(leading.gameObject);
        Spawn(_trailing.position.x - direction * Period);
    }

    private bool HasLeftBoard(Transform instance)
    {
        return direction > 0
            ? instance.position.x + _minOffset > _rightEdgeX
            : instance.position.x + _maxOffset < _leftEdgeX;
    }

    private Transform Spawn(float x)
    {
        Transform instance = _pool.Get().transform;
        instance.position = new Vector3(x, transform.position.y, transform.position.z);
        _objects.Enqueue(instance);
        _trailing = instance;
        return instance;
    }

    private void ConfigureDiver(Transform instance, int index, GameConfig config)
    {
        TurtleDiver diver = instance.GetComponent<TurtleDiver>();
        if (diver == null) return;

        float delay = 0f;
        if (diveStartDelays != null && diveStartDelays.Length > 0)
        {
            int length = diveStartDelays.Length;
            delay = diveStartDelays[(index % length + length) % length];
        }
        diver.Configure(config, delay);
    }

    private void MeasureExtents()
    {
        SpriteRenderer[] renderers = prefab.GetComponentsInChildren<SpriteRenderer>(true);
        if (renderers.Length == 0)
        {
            Debug.LogWarning($"{name}: prefab has no SpriteRenderer, using one cell as its width.");
            _minOffset = -0.5f;
            _maxOffset = 0.5f;
            return;
        }

        // Measured on the prefab asset, relative to its root, so it does not depend on where the lane is.
        _minOffset = float.MaxValue;
        _maxOffset = float.MinValue;
        foreach (SpriteRenderer spriteRenderer in renderers)
        {
            if (spriteRenderer.sprite == null) continue;

            Bounds sprite = spriteRenderer.sprite.bounds;
            Vector3 offset = spriteRenderer.transform.position - prefab.transform.position;
            Vector3 scale = spriteRenderer.transform.lossyScale;
            _minOffset = Mathf.Min(_minOffset, offset.x + sprite.min.x * scale.x);
            _maxOffset = Mathf.Max(_maxOffset, offset.x + sprite.max.x * scale.x);
        }
    }
}

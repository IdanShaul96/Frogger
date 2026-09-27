using UnityEngine;

public class MovingPatterns : MonoBehaviour
{
    public Vector2 direction = Vector2.right;
    public float speed = 1f;
    [SerializeField] private float wrapPadding = 1f;

    
    private float halfWidth;
    private float leftEdgeX;
    private float rightEdgeX;
    private float _speedMultiplier = 1f;

    public Vector3 Velocity => transform.TransformDirection(direction * CurrentSpeed);

    private float CurrentSpeed => speed * _speedMultiplier;

    // Rounds get faster: GameManager scales every lane when all five homes are filled.
    public void SetSpeedMultiplier(float multiplier)
    {
        _speedMultiplier = multiplier;
    }

    private void Start()
    {
        halfWidth = CalculateHalfWidth() + wrapPadding;
        
        // Wrap at the board edges, not the camera edges, so a wider screen never changes the lanes.
        GameConfig config = FindAnyObjectByType<GameManager>().Config;
        leftEdgeX = config.BoardLeftX;
        rightEdgeX = config.BoardRightX;
    }

    private float CalculateHalfWidth()
    {
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();

        if (renderers.Length == 0)
        {
            Debug.LogWarning($"{name}: no SpriteRenderer found, using 0.5 as half width.");
            return 0.5f;
        }

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        return bounds.extents.x;
    }

    private void Update()
    {
        if (direction.x > 0f && transform.position.x - halfWidth > rightEdgeX)
        {
            Vector3 position = transform.position;
            position.x = leftEdgeX - halfWidth;
            transform.position = position;
        }
        else if (direction.x < 0f && transform.position.x + halfWidth < leftEdgeX)
        {
            Vector3 position = transform.position;
            position.x = rightEdgeX + halfWidth;
            transform.position = position;
        }

        transform.Translate(direction * (CurrentSpeed * Time.deltaTime));
    }
}
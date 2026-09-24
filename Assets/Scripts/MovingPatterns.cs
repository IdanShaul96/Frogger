using UnityEngine;

public class MovingPatterns : MonoBehaviour
{
    public Vector2 direction = Vector2.right;
    public float speed = 1f;
    [SerializeField] private float wrapPadding = 1f;

    
    private float halfWidth;
    private Vector3 leftEdge;
    private Vector3 rightEdge;

    private void Start()
    {
        halfWidth = CalculateHalfWidth() + wrapPadding;
        
        leftEdge  = Camera.main.ViewportToWorldPoint(Vector3.zero);
        rightEdge = Camera.main.ViewportToWorldPoint(Vector3.right);
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
        if (direction.x > 0f && transform.position.x - halfWidth > rightEdge.x)
        {
            Vector3 position = transform.position;
            position.x = leftEdge.x - halfWidth;
            transform.position = position;
        }
        else if (direction.x < 0f && transform.position.x + halfWidth < leftEdge.x)
        {
            Vector3 position = transform.position;
            position.x = rightEdge.x + halfWidth;
            transform.position = position;
        }

        transform.Translate(direction * (speed * Time.deltaTime));
    }
}
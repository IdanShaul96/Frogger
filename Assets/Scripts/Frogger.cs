using UnityEngine;
using System.Collections;

public class Frogger : MonoBehaviour
{
    [SerializeField] private float leapDuration = 0.15f;
    
    private SpriteRenderer _spriteRenderer;
    private int _obstacleLayer;
    public Sprite idleSprite;
    public Sprite leapSprite;
    public Sprite deathSprite;
    
    private Vector3 _spawnPoint;
    private GameManager _gameManager;
    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _obstacleLayer = LayerMask.NameToLayer("Obstacle");
        _spawnPoint = transform.position;
        _gameManager = FindAnyObjectByType<GameManager>();
    }

    private float _leftEdgeX;
    private float _rightEdgeX;
    private float farthestRow;
    private void Start()
    {
        _leftEdgeX = Camera.main.ViewportToWorldPoint(Vector3.zero).x;
        _rightEdgeX = Camera.main.ViewportToWorldPoint(Vector3.right).x;
    }

    private void Update()
    {
        if (transform.parent != null && IsTouchingScreenEdge())
        {
            Death();
            return;
        }

        if (_isLeaping) return;
        
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
            Move(Vector3.up);
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            transform.rotation = Quaternion.Euler(0, 0, 180);
            Move(Vector3.down);
        }
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            transform.rotation = Quaternion.Euler(0, 0, 90);
            Move(Vector3.left);
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            transform.rotation = Quaternion.Euler(0, 0, -90);
            Move(Vector3.right);
        }
        
    }
    
   private void Move(Vector3 direction)
   {
       Vector3 destination = transform.position + direction;
       Collider2D barrier = Physics2D.OverlapBox(destination, Vector2.zero, 0f, LayerMask.GetMask(("Barrier")));
       Collider2D platform = Physics2D.OverlapBox(destination, Vector2.zero, 0f, LayerMask.GetMask(("Platform")));
       Collider2D obstacle = Physics2D.OverlapBox(destination, Vector2.zero, 0f, LayerMask.GetMask(("Obstacle")));

       if (barrier != null)
       {
           return;
       }

       if (platform != null)
       {
           transform.SetParent((platform.transform));
       }
       else
       {
           transform.SetParent(null);
       }
       
       if (obstacle != null && platform == null)
       {
           transform.position = destination;
           Death();
       }
       else
       {
           if (destination.y > farthestRow)
           {
               farthestRow = destination.y;
               FindAnyObjectByType<GameManager>().AdvancedRow();
           }
           
           StartCoroutine(Leap(destination));
       }
   }

   
   private bool _isLeaping;
   private IEnumerator Leap(Vector3 destination)
   {
       _isLeaping = true;
       Vector3 startPosition = transform.position;
       float elapsed = 0f;
       _spriteRenderer.sprite = leapSprite;
       while (elapsed < leapDuration)
       {
           float t =  elapsed / leapDuration;
           transform.position = Vector3.Lerp(startPosition, destination, t);
           elapsed += Time.deltaTime;
           yield return null;
       }
       transform.position = destination;
       _spriteRenderer.sprite = idleSprite;
       _isLeaping = false;
   }

   private bool IsTouchingScreenEdge()
   {
       float halfWidth = _spriteRenderer.bounds.extents.x;
       return transform.position.x - halfWidth < _leftEdgeX ||
              transform.position.x + halfWidth > _rightEdgeX;
   }

   public void Death()
   {
       StopAllCoroutines();
       transform.SetParent(null);
       transform.rotation = Quaternion.identity;
       _spriteRenderer.sprite = deathSprite;
       enabled = false;
       _gameManager.Died();
   }

   private void OnTriggerEnter2D(Collider2D other)
   {
       if (enabled && other.gameObject.layer == _obstacleLayer && transform.parent == null)
       {
           Death();
       }
   }
   
   public void Respawn()
   {
       StopAllCoroutines();
       _isLeaping = false;
       transform.SetParent(null);
       transform.position = _spawnPoint;
       transform.rotation = Quaternion.identity;
       farthestRow = _spawnPoint.y;
       _spriteRenderer.sprite = idleSprite;
       gameObject.SetActive(true);
       enabled = true;
   }
}

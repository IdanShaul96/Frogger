using UnityEngine;

// Marks a log or turtle group the frog can ride.
// The frog asks it for its velocity and whether it is safe to stand on.
public class Rideable : MonoBehaviour
{
    private MovingPatterns _movingPatterns;

    public bool IsSafe { get; set; } = true;

    public Vector3 Velocity => _movingPatterns != null ? _movingPatterns.Velocity : Vector3.zero;

    private void Awake()
    {
        _movingPatterns = GetComponent<MovingPatterns>();
    }
}

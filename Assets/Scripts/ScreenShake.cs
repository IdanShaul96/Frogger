using System.Collections;
using UnityEngine;

// Short camera shake on death. Added to the main camera at runtime by GameManager.
public class ScreenShake : MonoBehaviour
{
    [SerializeField] private float duration = 0.2f;
    [SerializeField] private float strength = 0.15f;

    private Vector3 _origin;
    private Coroutine _shakeRoutine;

    private void Awake()
    {
        _origin = transform.localPosition;
    }

    public void Shake()
    {
        if (_shakeRoutine != null)
        {
            StopCoroutine(_shakeRoutine);
        }
        _shakeRoutine = StartCoroutine(ShakeRoutine());
    }

    private IEnumerator ShakeRoutine()
    {
        for (float elapsed = 0f; elapsed < duration; elapsed += Time.unscaledDeltaTime)
        {
            transform.localPosition = _origin + (Vector3)(Random.insideUnitCircle * strength);
            yield return null;
        }
        transform.localPosition = _origin;
    }
}

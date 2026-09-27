using UnityEngine;
using UnityEngine.Pool;

// Hands out and takes back one lane's objects. Every instance is created up front,
// so nothing is instantiated or destroyed during play.
public class LanePool
{
    private readonly ObjectPool<GameObject> _pool;

    public LanePool(GameObject prefab, Transform parent, int size)
    {
        _pool = new ObjectPool<GameObject>(
            createFunc: () => Object.Instantiate(prefab, parent),
            actionOnGet: instance => instance.SetActive(true),
            actionOnRelease: instance => instance.SetActive(false),
            actionOnDestroy: Object.Destroy,
            collectionCheck: false,
            defaultCapacity: size,
            maxSize: size);

        Prewarm(size);
    }

    public GameObject Get()
    {
        return _pool.Get();
    }

    public void Release(GameObject instance)
    {
        _pool.Release(instance);
    }

    private void Prewarm(int size)
    {
        var instances = new GameObject[size];
        for (int i = 0; i < size; i++)
        {
            instances[i] = _pool.Get();
        }
        foreach (GameObject instance in instances)
        {
            _pool.Release(instance);
        }
    }
}

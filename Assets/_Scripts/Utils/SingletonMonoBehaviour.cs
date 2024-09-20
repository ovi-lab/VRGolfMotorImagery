using UnityEngine;

public abstract class SingletonMonoBehavior<T>: MonoBehaviour where T : MonoBehaviour, new()
{
    private static T _instance;
    public static T Instance => _instance;

    protected virtual void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = GetComponent<T>();
    }
}
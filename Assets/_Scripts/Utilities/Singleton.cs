using UnityEngine;

/// <summary>
/// StaticInstance クラスは、MonoBehaviour を継承するシングルトンの基底クラスです。
/// </summary>
/// <typeparam name="T">シングルトンとする型</typeparam>
public abstract class StaticInstance<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instance { get; private set; }

    protected virtual void Awake()
    {
        Instance = this as T;
    }

    protected virtual void OnApplicationQuit()
    {
        Instance = null;
        Destroy(gameObject);
    }
}

/// <summary>
/// Singleton クラスは、StaticInstance を拡張し、シーン遷移時にも破棄されないシングルトンを実現します。
/// </summary>
/// <typeparam name="T">シングルトンとする型</typeparam>
public class Singleton<T> : StaticInstance<T> where T : MonoBehaviour
{
    protected override void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        base.Awake();
    }
}

/// <summary>
/// PresistentSingleton クラスは、Singleton をさらに拡張し、常駐するシングルトンの基底クラスとして使用します。
/// </summary>
/// <typeparam name="T">シングルトンとする型</typeparam>
public abstract class PresistentSingleton<T> : Singleton<T> where T : MonoBehaviour
{
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }
}

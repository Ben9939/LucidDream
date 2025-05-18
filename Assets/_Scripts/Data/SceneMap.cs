using System;
using System.Collections.Generic;

/// <summary>
/// SceneMap クラスは、双方向のマッピングを提供します。
/// T は Enum 型でなければならず、キーと値の双方から相互変換が可能です。
/// </summary>
public class SceneMap<T> where T : Enum
{
    private Dictionary<T, T> forward = new Dictionary<T, T>();
    private Dictionary<T, T> reverse = new Dictionary<T, T>();

    /// <summary>
    /// キーと値のペアを追加します。
    /// 同じキーまたは値がすでに存在する場合、例外をスローします。
    /// </summary>
    /// <param name="key">追加するキー</param>
    /// <param name="value">追加する値</param>
    public void Add(T key, T value)
    {
        if (forward.ContainsKey(key) || reverse.ContainsKey(value))
        {
            throw new ArgumentException("Key or value already exists in the map.");
        }
        forward.Add(key, value);
        reverse.Add(value, key);
    }

    /// <summary>
    /// 指定したキーまたは値に対応するペアを取得します。
    /// </summary>
    /// <param name="keyOrValue">キーまたは値</param>
    /// <returns>対応するキーと値のペア</returns>
    public KeyValuePair<T, T> GetPair(T keyOrValue)
    {
        if (forward.ContainsKey(keyOrValue))
        {
            return new KeyValuePair<T, T>(keyOrValue, forward[keyOrValue]);
        }
        else if (reverse.ContainsKey(keyOrValue))
        {
            return new KeyValuePair<T, T>(reverse[keyOrValue], keyOrValue);
        }
        else
        {
            throw new KeyNotFoundException($"Key or value '{keyOrValue}' not found in the SceneMap.");
        }
    }

    /// <summary>
    /// 指定したキーまたは値に対応する値を取得します。
    /// </summary>
    /// <param name="keyOrValue">キーまたは値</param>
    /// <returns>対応する値</returns>
    public T GetValue(T keyOrValue)
    {
        if (forward.ContainsKey(keyOrValue))
        {
            return forward[keyOrValue];
        }
        else if (reverse.ContainsKey(keyOrValue))
        {
            return reverse[keyOrValue];
        }
        else
        {
            throw new KeyNotFoundException($"Key or value '{keyOrValue}' not found in the SceneMap.");
        }
    }
}

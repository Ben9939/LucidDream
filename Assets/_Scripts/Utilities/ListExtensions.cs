using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ListExtensions クラスは、IList の拡張メソッドを提供します。
/// ここでは、リストをシャッフルする拡張メソッドを実装しています。
/// </summary>
public static class ListExtensions
{
    /// <summary>
    /// IList の要素をランダムにシャッフルします。
    /// </summary>
    /// <typeparam name="T">リストの要素の型</typeparam>
    /// <param name="list">シャッフル対象のリスト</param>
    public static void Shuffle<T>(this IList<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = UnityEngine.Random.Range(i, list.Count);
            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}

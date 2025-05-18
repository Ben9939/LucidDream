using UnityEngine;

/// <summary>
/// ポートレート（立ち絵）のプレハブ情報を提供するインターフェイスです。
/// </summary>
public interface IPortraitHolder
{
    /// <summary>
    /// ポートレート用のプレハブを取得します。
    /// </summary>
    GameObject PortraitPrefab { get; }
}

using UnityEngine;
using XNode;

/// <summary>
/// ダイアログの開始ノードです。
/// 出力ポート "exit" を介して次のノードへ接続されます。
/// </summary>
public class Dialog_StartNode : Dialog_BaseNode
{
    [Output] public int exit;

    /// <summary>
    /// ノードの種類を Start として返します。
    /// </summary>
    public override DialogNodeType GetType() => DialogNodeType.Start;
}

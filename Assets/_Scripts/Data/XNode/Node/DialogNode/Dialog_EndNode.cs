using UnityEngine;

/// <summary>
/// ダイアログの終了ノードです。
/// 入力ポート "entry" を受け取り、ダイアログの終了状態を示します。
/// </summary>
public class Dialog_EndNode : Dialog_BaseNode
{
    [Input] public int entry;

    /// <summary>
    /// ノードの種類を End として返します。
    /// </summary>
    public override DialogNodeType GetType() => DialogNodeType.End;
}


// DialogState_ContentNode.cs
using UnityEngine;
using XNode;

/// <summary>
/// ダイアログのコンテンツノードです。
/// 入力ポート "entry" と出力ポート "exit" を持ち、
/// 対応するダイアロググラフおよび必要な状態を保持します。
/// </summary>
public class DialogState_ContentNode : DialogState_BaseNode
{
    [Input] public int entry;
    [Output] public int exit;

    /// <summary>
    /// このノードに対応するダイアロググラフ（実際の対話内容を表示）
    /// </summary>
    public DialogGraph dialogGraph;

    /// <summary>
    /// チェックする必要がある状態
    /// </summary>
    public UnitStateSO requiredUnitState;

    public override DialogStateNodeType GetType() => DialogStateNodeType.Content;
}


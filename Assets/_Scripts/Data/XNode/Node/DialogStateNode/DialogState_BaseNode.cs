using XNode;
/// <summary>
/// ダイアログノードの種類を定義します。
/// </summary>
public enum DialogStateNodeType
{
    None,
    Start,
    Content,
    Branch,
    End
}

/// <summary>
/// すべてのダイアログノードの基底クラスです。
/// </summary>
public class DialogState_BaseNode : Node
{
    /// <summary>
    /// ノードの種類を取得します。デフォルトは None を返します。
    /// </summary>
    public virtual DialogStateNodeType GetType() => DialogStateNodeType.None;
}

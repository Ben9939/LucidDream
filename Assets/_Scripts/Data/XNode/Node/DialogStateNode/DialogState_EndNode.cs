/// <summary>
/// ダイアログ状態の終了ノードです。
/// 入力ポート "entry" を受け取ります。
/// </summary>
public class DialogState_EndNode : DialogState_BaseNode
{
    [Input] public int entry;

    public override DialogStateNodeType GetType() => DialogStateNodeType.End;
}


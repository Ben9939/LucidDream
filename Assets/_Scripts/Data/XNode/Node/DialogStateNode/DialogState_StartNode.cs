/// <summary>
/// ダイアログ状態の開始ノードです。
/// 出力ポート "exit" を介して次のノードに接続されます。
/// </summary>
public class DialogState_StartNode : DialogState_BaseNode
{
    [Output] public int exit;

    public override DialogStateNodeType GetType() => DialogStateNodeType.Start;
}

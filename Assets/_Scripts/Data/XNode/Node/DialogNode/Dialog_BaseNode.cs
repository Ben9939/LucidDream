using XNode;
public enum DialogNodeType
{
    None,
    Start,
    Line,
    Branch,
    End
}
/// <summary>
/// ダイアログシステムの基本ノードクラス。<br/>
/// すべてのダイアログノードはこのクラスを継承する。
/// </summary>
public class Dialog_BaseNode : Node {

    public virtual DialogNodeType GetType()
    {
        return DialogNodeType.None;
    }
}
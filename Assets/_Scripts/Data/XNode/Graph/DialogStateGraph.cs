using UnityEngine;
using XNode;

/// <summary>
/// DialogStateGraphは、対話状態のグラフを管理するクラスです。<br/>
/// 現在の状態ノード（currentNode）を保持し、対話フローの初期化および次の状態ノードの取得を行います。
/// </summary>
[CreateAssetMenu(fileName = "DialogStateGraph", menuName = "Graph/DialogStateGraph")]
public class DialogStateGraph : NodeGraph
{
    /// <summary>
    /// 現在の状態ノード（進行状況をトラッキングするため）
    /// </summary>
    public DialogState_BaseNode currentNode;

    /// <summary>
    /// 対話フローを初期化し、currentNodeをStartノードに設定します。
    /// </summary>
    public void StartDialog()
    {
        currentNode = GetStartNode();
    }

    /// <summary>
    /// 現在の状態ノードの "exit" 出力ポートから、次に接続されているノードを取得して返します。<br/>
    /// Branchノードの場合は専用のロジックで次のノードを選択します。
    /// </summary>
    /// <returns>次の状態ノード。接続がない場合はnullを返します。</returns>
    public DialogState_BaseNode GetNextNode()
    {
        if (currentNode == null) return null; 

        NodePort outputPort = null;

        // 現在の状態ノードの種類に応じて、対応する出力ポートを取得
        switch (currentNode)
        {
            case DialogState_StartNode startNode:
                outputPort = startNode.GetPort("exit");
                break;
            case DialogState_ContentNode contentNode:
                outputPort = contentNode.GetPort("exit");
                break;
            case DialogState_BranchNode branchNode:
                return branchNode.GetNextBranchNode();
            case DialogState_EndNode endNode:
                return currentNode;
        }

        if (outputPort != null && outputPort.IsConnected)
        {
            return outputPort.Connection.node as DialogState_BaseNode;
        }
        return null;
    }

    /// <summary>
    /// グラフ内のすべてのノードから、最初に見つかったStartノードを返します。
    /// </summary>
    /// <returns>Startノード。存在しない場合はnull。</returns>
    private DialogState_StartNode GetStartNode()
    {
        foreach (var node in nodes)
        {
            if (node is DialogState_StartNode startNode)
                return startNode;
        }
        return null;
    }

    /// <summary>
    /// 内部データをリセットし、currentNodeをnullに設定します。
    /// </summary>
    public void ResetData()
    {
        currentNode = null;
    }
}

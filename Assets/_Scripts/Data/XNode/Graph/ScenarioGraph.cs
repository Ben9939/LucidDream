using UnityEngine;
using XNode;

/// <summary>
/// ScenarioGraph は、シナリオの進行を管理するグラフです。<br/>
/// 現在の状態ノード (currentNode) を保持し、シナリオの開始や次の状態ノードの取得を行います。
/// </summary>
[CreateAssetMenu(fileName = "Scenario Graph", menuName = "Graph/Scenario Graph")]
public class ScenarioGraph : NodeGraph
{
    /// <summary>
    /// 現在の状態ノード（シナリオ進行状況のトラッキング用）
    /// </summary>
    public Scenario_BaseNode currentNode;

    /// <summary>
    /// シナリオを初期化し、currentNode を Start ノードに設定します。
    /// </summary>
    public void StartScenario()
    {
        currentNode = GetStartNode();
    }

    /// <summary>
    /// 現在の状態ノードの "exit" 出力ポートから、次の接続ノードを取得して返します。<br/>
    /// Branch ノードの場合は、専用ロジックで次のノードを選択します。
    /// </summary>
    /// <returns>次の状態ノード。接続が存在しない場合は null を返します。</returns>
    public Scenario_BaseNode GetNextNode()
    {
        if (currentNode == null)
            return null;

        NodePort outputPort = null;

        // 現在の状態ノードの種類に応じて、対応する出力ポートを取得する
        switch (currentNode)
        {
            case Scenario_StartNode startNode:
                outputPort = startNode.GetPort("exit");
                break;
            case Scenario_ConditionNode contentNode:
                outputPort = contentNode.GetPort("exit");
                break;
            case Scenario_BranchNode branchNode:
                return branchNode.GetNextBranchNode();
        }

        if (outputPort != null && outputPort.IsConnected)
        {
            return outputPort.Connection.node as Scenario_BaseNode;
        }
        return null;
    }

    /// <summary>
    /// グラフ内の全ノードから、最初に見つかった Start ノードを返します。
    /// </summary>
    /// <returns>Start ノード。存在しない場合は null。</returns>
    private Scenario_StartNode GetStartNode()
    {
        foreach (var node in nodes)
        {
            if (node is Scenario_StartNode startNode)
                return startNode;
        }
        return null;
    }

    /// <summary>
    /// 内部データをリセットし、currentNode を null に設定します。
    /// </summary>
    public void ResetData()
    {
        currentNode = null;
    }
}

using XNode;
using UnityEngine;

/// <summary>
/// Scenario_StartNode は、シナリオの開始ノードを表します。
/// 出口ポートを持ち、シナリオの最初の接続先を示します。
/// </summary>
public class Scenario_StartNode : Scenario_BaseNode
{
    [Output] public int exit;
}

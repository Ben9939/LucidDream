using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

/// <summary>
/// Scenario_EndingNode は、シナリオの終了ノードを表します。
/// 対話グラフ（EventDialog）を保持し、シナリオの完了時に使用されます。
/// </summary>
public class Scenario_EndingNode : Scenario_BaseNode
{
    [Input] public int entry;
    [Output] public int exit;
    [SerializeField] public DialogGraph EventDialog = null;
}

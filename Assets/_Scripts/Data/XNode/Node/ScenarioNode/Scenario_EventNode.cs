using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

/// <summary>
/// Scenario_EventNode は、特定のイベント対話を実行するノードです。
/// EventDialog を保持し、対話イベントの発生に対応します。
/// </summary>
public class Scenario_EventNode : Scenario_BaseNode
{
    [Input] public int entry;
    [Output] public int exit;
    [SerializeField] public DialogGraph EventDialog = null;
}


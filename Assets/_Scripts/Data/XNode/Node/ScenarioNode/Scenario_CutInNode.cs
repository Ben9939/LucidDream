using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

/// <summary>
/// Scenario_CutInNode は、カットイン演出を実行するノードです。
/// カットインのタイミングで表示される映像などを管理します。
/// </summary>
public class Scenario_CutInNode : Scenario_BaseNode
{
    [Input] public int entry;
    [Output] public int exit;
}


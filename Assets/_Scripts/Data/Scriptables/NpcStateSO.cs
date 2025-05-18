using System.Collections.Generic;

/// <summary>
/// NPCまたはオブジェクトの対話進行と内部変数を管理するクラスです。
/// NPCのID、現在の対話ノード、及び任意の変数を保持します。
/// </summary>
public class NpcStateSO
{
    /// <summary>NPCの識別子</summary>
    public string NpcID;

    /// <summary>現在の対話状態グラフのインデックス</summary>
    public int ActiveStateGraphIndex = 0;

    /// <summary>現在の対話ノードのID</summary>
    public string CurrentStateNodeID;

    // 内部のNPC変数を保持するディクショナリ
    private Dictionary<string, int> npcVariables = new Dictionary<string, int>();

    /// <summary>
    /// コンストラクタ：NPCのIDを設定します。
    /// </summary>
    public NpcStateSO(string id)
    {
        NpcID = id;
    }

    /// <summary>
    /// 指定されたキーに対応する変数の値を設定します。
    /// </summary>
    public void SetVariable(string key, int value)
    {
        npcVariables[key] = value;
    }

    /// <summary>
    /// 指定されたキーの変数の値を取得します（存在しない場合は0を返す）。
    /// </summary>
    public int GetVariable(string key)
    {
        return npcVariables.TryGetValue(key, out int value) ? value : 0;
    }

    /// <summary>
    /// 指定されたキーの変数が存在するかを確認します。
    /// </summary>
    public bool HasVariable(string key)
    {
        return npcVariables.ContainsKey(key);
    }
}

//using System.Collections.Generic;

///// <summary>
///// 存儲 NPC 或物件的對話進度與變數
///// </summary>
//public class NpcStateSO
//{
//    public string npcID;
//    public int activeStateGraphIndex = 0;  // 當前 `DialogStateGraph` 索引
//    public string currentStateNodeID;      // 當前對話節點的 ID
//    private Dictionary<string, int> npcVariables = new Dictionary<string, int>();

//    public NpcStateSO(string id)
//    {
//        npcID = id;
//    }

//    public void SetVariable(string key, int value)
//    {
//        npcVariables[key] = value;
//    }

//    public int GetVariable(string key)
//    {
//        return npcVariables.TryGetValue(key, out int value) ? value : 0;
//    }

//    public bool HasVariable(string key)
//    {
//        return npcVariables.ContainsKey(key);
//    }
//}

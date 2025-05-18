using System.Collections;
using UnityEngine;

/// <summary>
/// NpcMovementTypeSO は、NPC の移動パターンの基底クラスとして使用する ScriptableObject です。
/// 各移動タイプはこのクラスを継承し、独自の移動ロジックを実装します。
/// </summary>
public abstract class NpcMovementTypeSO : ScriptableObject
{
    /// <summary>
    /// 指定された NPCController に対して移動処理を実行するコルーチンを返します。
    /// </summary>
    /// <param name="npc">移動対象の NPCController</param>
    /// <returns>移動処理のコルーチン</returns>
    public abstract IEnumerator Move(NPCController npc);
}

//using UnityEngine;
//using System.Collections;

//public abstract class NpcMovementTypeSO : ScriptableObject
//{
//    public abstract IEnumerator Move(NPCController npc);
//}

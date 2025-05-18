using UnityEngine;

/// <summary>
/// 金鑰アイテムの能力を管理するScriptableObjectです。  
/// 対象が正しいオブジェクトID（例："Chest"）であるか検証し、  
/// 必要であれば金鑰による開錠処理を実行します。
/// </summary>
[CreateAssetMenu(fileName = "NewKeyAbility", menuName = "Items/Abilities/Key Ability")]
public class KeyAbilitySO : ItemAbilitySO
{
    [Tooltip("金鑰が作用可能な対象のID（例：Chest）")]
    public string validTargetID = "Chest";

    /// <summary>
    /// 対象のオブジェクトが金鑰の作用可能な対象であるかを検証します。
    /// </summary>
    public override bool ValidateTarget(ObjectDataSO target)
    {
        if (target == null)
            return false;

        // ObjectDataSOはunitIDプロパティを持つ前提で検証
        return target.unitID == validTargetID;
    }

    /// <summary>
    /// 金鑰の能力を発動します。  
    /// ここでは、宝箱の状態を変更する処理などを実装する想定です。
    /// </summary>
    public override void Activate(ObjectDataSO target)
    {
        // 例: 対象の宝箱の状態を「開錠済み」に更新する処理を実装
        // target.SetStateValue(ChestStateNames.HasKey, 1);
        // Debug.Log("金鑰を使用して宝箱を開錠しました。");
    }
}

//using UnityEngine;

//[CreateAssetMenu(fileName = "NewKeyAbility", menuName = "Items/Abilities/Key Ability")]
//public class KeyAbilitySO : ItemAbilitySO
//{
//    // 假設這裡定義了金鑰能夠作用的目標標識，例如 "Chest"
//    public string validTargetID = "Chest";

//    public override bool ValidateTarget(ObjectDataSO target)
//    {
//        if (target == null)
//            return false;

//        // 假設 ObjectDataSO 有一個 objectID 屬性
//        return target.unitID == validTargetID;
//    }

//    public override void Activate(ObjectDataSO target)
//    {
//        //// 此處執行金鑰的作用
//        //target.SetStateValue(ChestStateNames.HasKey, 1);
//        //Debug.Log("使用金鑰打開了寶箱，寶箱狀態已更新。");
//    }
//}

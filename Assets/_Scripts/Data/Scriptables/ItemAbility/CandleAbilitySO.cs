using UnityEngine;

/// <summary>
/// キャンドルアイテムの能力を管理するScriptableObjectです。  
/// アイテム使用時に、プレイヤーのライトアップ処理を呼び出します。
/// </summary>
[CreateAssetMenu(fileName = "NewCandleAbility", menuName = "Items/Abilities/Candle Ability")]
public class CandleAbilitySO : ItemAbilitySO
{
    /// <summary>
    /// 指定された対象に対してキャンドルの能力を発動します。  
    /// この例では、プレイヤーのライトアップ処理を実行します。
    /// </summary>
    /// <param name="target">アイテム使用対象のオブジェクトデータ</param>
    public override void Activate(ObjectDataSO target)
    {
    }
}

//using UnityEngine;

//[CreateAssetMenu(fileName = "NewCandleAbility", menuName = "Items/Abilities/Candle Ability")]
//public class CandleAbilitySO : ItemAbilitySO
//{
//    public override void Activate(ObjectDataSO target)
//    {
//        PlayerController.Instance.HnedleLightUp();
//    }
//}

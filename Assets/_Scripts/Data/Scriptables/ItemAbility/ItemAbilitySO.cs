using UnityEngine;

/// <summary>
/// アイテムの能力を管理する抽象ScriptableObjectクラスです。  
/// アイテム使用時に対象の検証と実際の発動処理を行います。
/// </summary>
public abstract class ItemAbilitySO : ScriptableObject
{
    /// <summary>
    /// 対象が必要かどうか（trueの場合、対象が必須）。
    /// </summary>
    public bool RequiresTarget = false;

    /// <summary>
    /// 対象の有効性を検証します。  
    /// デフォルトではnullチェックのみ行いますが、必要に応じて子クラスでオーバーライド可能です。
    /// </summary>
    /// <param name="target">対象のオブジェクトデータ</param>
    /// <returns>有効ならtrue、無効ならfalse</returns>
    public virtual bool ValidateTarget(ObjectDataSO target)
    {
        return target != null;
    }

    /// <summary>
    /// 対象の検証に成功した場合、能力を発動します。  
    /// </summary>
    /// <param name="target">対象のオブジェクトデータ</param>
    /// <returns>発動成功ならtrue、失敗ならfalse</returns>
    public bool TryActivate(ObjectDataSO target)
    {
        if (RequiresTarget && !ValidateTarget(target))
        {
            Debug.LogWarning($"{this.name} は対象検証に失敗しました。");
            return false;
        }
        Activate(target);
        return true;
    }

    /// <summary>
    /// 能力を発動する抽象メソッドです。  
    /// 子クラスで具体的な処理を実装してください。
    /// </summary>
    public abstract void Activate(ObjectDataSO target);
}

//using UnityEngine;

//public abstract class ItemAbilitySO : ScriptableObject
//{
//    public bool RequiresTarget = false;

//    /// <summary>
//    /// 檢查目標是否有效，預設只驗證非 null，如有特殊需求，可在子類中覆寫
//    /// </summary>
//    /// <param name="target">目標對象</param>
//    /// <returns>有效則返回 true，否則返回 false</returns>
//    public virtual bool ValidateTarget(ObjectDataSO target)
//    {
//        return target != null;
//    }

//    public bool TryActivate(ObjectDataSO target)
//    {
//        if (RequiresTarget && !ValidateTarget(target))
//        {
//            Debug.LogWarning($"{this.name} 無法對該目標執行（目標驗證失敗）！");
//            return false;
//        }
//        Activate(target);
//        return true;
//    }

//    public abstract void Activate(ObjectDataSO target);
//}

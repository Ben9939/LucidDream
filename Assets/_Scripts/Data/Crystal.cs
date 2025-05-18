using UnityEngine;

/// <summary>
/// CrystalAttribute 列挙型は、クリスタルの属性を定義します。
/// </summary>
public enum CrystalAttribute
{
    Heal,
    Attack,
    IncreaseMana,
    IncreaseDefense
}

/// <summary>
/// Crystal クラスは、クリスタルオブジェクトの動作と属性効果を管理します。
/// クリスタルは、各属性に応じた効果を適用します。
/// </summary>
public class Crystal : MonoBehaviour
{
    // クリスタルの属性（public は PascalCase）
    public CrystalAttribute CrystalAttribute;

    // クリスタルのスキル用スプライト
    public Sprite SkillSprite;

    /// <summary>
    /// クリスタルのスキルスプライトを取得します。
    /// </summary>
    public Sprite Sprite
    {
        get { return SkillSprite; }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 戦闘アクションを実行するクラス。
/// このクラスは、攻撃、回復、mp増加の各処理を行います。
/// ・攻撃処理では、対象のFighterStatsからmpと攻撃力（melee）を参照し、ダメージを計算してアニメーションと効果音を再生します。
/// ・回復処理では、回復力（healing）を基にhpを回復し、mpの消費を反映します。
/// ・mp増加処理では、回復力を元に計算した量だけmpを増加させます。
/// </summary>
public class ActionScript : MonoBehaviour
{
    // 所有者のゲームオブジェクト。攻撃や回復の実行元となる。
    public GameObject Owner;

    // 魔法コスト：各アクションに必要なmpの消費量
    [SerializeField] private float magicCost;

    // 攻撃者および対象のFighterStatsコンポーネントを保持する変数
    private FighterStats attackerStats;
    private FighterStats targetStats;

    // 攻撃によって与えるダメージを保持
    private float damage = 0.0f;

    /// <summary>
    /// 攻撃処理を実行するメソッド。
    /// 【処理の流れ】
    /// 1. 所有者（攻撃者）と攻撃対象（victim）のFighterStatsコンポーネントを取得する。
    /// 2. 攻撃者のmpが、アクション実行に必要なmagicCost以上あるかをチェックする。
    /// 3. 攻撃ダメージを、攻撃力（melee）とmultiplierの積で計算する。
    /// 4. 攻撃アニメーション「Melee」を再生し、対象にダメージを与えるコルーチンを開始する。
    /// 5. 効果音再生のためのコルーチンを開始し、mpの消費を反映する。
    /// </summary>
    /// <param name="victim">攻撃対象のゲームオブジェクト</param>
    /// <param name="multiplier">攻撃の強度を表すマルチプライヤ</param>
    public void Attack(GameObject victim, int multiplier)
    {
        // 攻撃者と対象のステータスコンポーネントを取得
        attackerStats = Owner.GetComponent<FighterStats>();
        targetStats = victim.GetComponent<FighterStats>();

        // mpが十分にあるか確認（FighterStatsのmagicプロパティはmpとして扱う）
        if (attackerStats.magic >= magicCost)
        {
            // 攻撃ダメージの計算（攻撃力(melee)にmultiplierを乗算）
            damage = multiplier * attackerStats.melee;
            Debug.Log("Performing attack. Calculated damage: " + damage);

            // 攻撃アニメーション「Melee」を再生
            Owner.GetComponent<Animator>().Play("Melee");

            // 対象にダメージを与える処理をコルーチンで実行（ダメージは切り上げ処理）
            StartCoroutine(targetStats.ReceiveDamage(Mathf.CeilToInt(damage)));

            // 攻撃効果音の再生コルーチンを開始
            StartCoroutine(PlaySe(multiplier));

            // mpの消費を更新
            attackerStats.UpdateMagicFill(magicCost);
        }
        else
        {
            Debug.Log("Not enough mp to perform attack.");
        }
    }

    /// <summary>
    /// レベルに応じた効果音を再生するコルーチン。
    /// 各レベル毎に設定された効果音を再生し、0.5秒の待機時間を設ける。
    /// </summary>
    /// <param name="level">攻撃の強度に対応するレベル</param>
    IEnumerator PlaySe(int level)
    {
        switch (level)
        {
            case 1:
                AudioManager.Instance.PlaySE(SESoundData.SE.Damage_Lv1);
                break;
            case 2:
                AudioManager.Instance.PlaySE(SESoundData.SE.Damage_Lv2);
                break;
            case 3:
                AudioManager.Instance.PlaySE(SESoundData.SE.Damage_Lv3);
                break;
            default:
                AudioManager.Instance.PlaySE(SESoundData.SE.Damage_Lv1);
                break;
        }
        yield return new WaitForSeconds(0.5f);
    }

    /// <summary>
    /// 回復処理を実行するメソッド。
    /// </summary>
    /// <param name="multiplier">回復量の倍率</param>
    public void Healing(int multiplier)
    {
        // 攻撃者（回復を実行する側）のステータスを取得
        attackerStats = Owner.GetComponent<FighterStats>();

        // mpが十分にあるか確認
        if (attackerStats.magic >= magicCost)
        {
            // 回復量の計算（回復力(healing)にmultiplierを乗算）
            float healAmount = attackerStats.healing * multiplier;
            Debug.Log("Performing healing. Heal amount: " + healAmount);

            // hpを回復する処理を実行
            attackerStats.UpdateHealth(healAmount);

            // mpの消費を更新
            attackerStats.UpdateMagicFill(magicCost);
        }
        else
        {
            Debug.Log("Not enough mp to perform healing.");
        }
    }

    /// <summary>
    /// mpを増加させる処理を実行するメソッド。
    /// ※ この処理ではmpの消費チェックは行いません。
    /// </summary>
    /// <param name="multiplier">増加量の倍率</param>
    public void IncreaseMana(int multiplier)
    {
        // 攻撃者のステータスを取得
        attackerStats = Owner.GetComponent<FighterStats>();

        // mp増加量の計算（回復力(healing)にmultiplierを乗算）
        float increaseAmount = attackerStats.healing * multiplier;
        Debug.Log("Increasing mp. Amount: " + increaseAmount);

        // mpの更新処理を実行
        attackerStats.UpdateMagic(increaseAmount);
    }
}


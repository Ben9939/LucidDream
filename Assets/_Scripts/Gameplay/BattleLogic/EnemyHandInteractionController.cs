using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 敵の手の操作を制御するクラス。
/// このクラスは、敵のhpとmpの状態に基づいて最適な水晶ターゲットを選び、手の動作や衝突処理を管理します。
/// エディタ上の表示やログは英語、コメントやドキュメントは日本語で記述されています。
/// </summary>
public class EnemyHandInteractionController : MonoBehaviour
{
    // プライベート変数: 敵オブジェクトへの参照
    private GameObject enemy;

    [Header("Credit Data")]
    [SerializeField] private GameObject handPoints;
    [SerializeField] private List<Sprite> handPointSprite;
    [SerializeField] private List<UnityEngine.Object> skillList; // 予め設定されたスキルアイコンのリスト

    // パブリックメンバー: 衝突検出用レイヤーと検出半径
    public LayerMask InteractablesLayer;
    public float DetectionRadius = 0f;

    // 内部状態管理用の変数
    private static bool isCollisionHandling = false;
    private List<CrystalAttribute> touchedCrystals = new List<CrystalAttribute>(); // 衝突した水晶の属性リスト
    private List<GameObject> crystalList = new List<GameObject>(); // 現在の水晶オブジェクトのリスト
    private List<GameObject> targetList = new List<GameObject>();  // 選定されたターゲット水晶のリスト
    private bool isMoving = false;

    private int currentCrystalCount = 0;
    private int previousCrystalCount = 0;

    // 敵のHPとMP（FighterStatsから取得）
    private float hp;
    private float mp;

    // 依存性注入: BattleLogicManager の参照（外部からセットされる必要があります）
    public BattleLogicManager BattleLogicManager;

    void Awake()
    {
        // タグ "Enemy" を持つオブジェクトを検索して参照を取得
        enemy = GameObject.FindGameObjectWithTag("Enemy");
    }

    /// <summary>
    /// BattleLogicManager が水晶の生成を完了した後に初期化処理を行います。
    /// このメソッドでは、BattleLogicManager と水晶リストが利用可能になるまで待機し、
    /// 敵のFighterStatsコンポーネントからhpとmpの値を取得して、ターゲット水晶リストを設定します。
    /// </summary>
    public void Initialized()
    {
        StartCoroutine(WaitForCrystalsAndInitialize());
    }

    /// <summary>
    /// BattleLogicManagerと水晶リストが準備できるまで待機し、その後の初期化処理を実行するコルーチンです。
    /// 主な処理内容は以下の通り：
    /// - BattleLogicManagerが存在し、水晶リストが生成されるのを待機
    /// - BattleLogicManagerから最新の水晶リストを取得
    /// - 敵オブジェクトのFighterStatsからhpとmpの値を取得
    /// - 現在の水晶数を記録し、ターゲット水晶リストを計算する
    /// </summary>
    private IEnumerator WaitForCrystalsAndInitialize()
    {
        handPoints.GetComponent<Transform>().position = new Vector2(0,0);
        while (BattleLogicManager == null ||
               BattleLogicManager.GetCrystalList() == null ||
               BattleLogicManager.GetCrystalList().Count == 0)
        {
            yield return null;
        }
        // BattleLogicManagerから水晶リストを取得
        crystalList = BattleLogicManager.GetCrystalList();

        // 敵のFighterStatsコンポーネントから現在のhpとmpを取得
        var stats = enemy.GetComponent<FighterStats>().GetStats();
        hp = stats.health;  // 敵のHP
        mp = stats.magic;   // 敵のMP

        // 水晶数の記録とターゲット水晶リストの設定
        currentCrystalCount = crystalList.Count;
        previousCrystalCount = currentCrystalCount;
        targetList = SetTargetList();

        yield break;
    }

    /// <summary>
    /// 毎フレーム呼び出され、以下の処理を実施します：
    /// - BattleLogicManagerから最新の水晶リストを取得
    /// - targetList内の各ターゲットの有効性（位置情報）をチェックし、無効な場合は再計算
    /// - 手が移動中でなく、かつ衝突済み水晶数が3未満の場合、ターゲットに向けた手の移動を開始
    /// - 指定範囲内にある衝突可能なオブジェクトをチェックし、手の開閉アニメーションと衝突処理を実行
    /// </summary>
    public void HandleEnemyHandPointUpdate()
    {
        // 最新の水晶リストをBattleLogicManagerから取得
        crystalList = BattleLogicManager.GetCrystalList();

        // targetList内の各ターゲットの位置を確認し、無効なターゲットがあればリストを再計算
        foreach (var target in targetList)
        {
            if (target != null && !crystalList.Any(crystal => crystal.transform.position == target.transform.position))
            {
                Debug.Log("Recalculating target list.");
                targetList = SetTargetList();
                foreach (var t in targetList)
                {
                    Debug.Log("Target: " + t);
                }
                break;
            }
        }

        // 手が移動中でなく、かつ衝突済み水晶数が3未満の場合、ターゲットに向けて手の移動を開始
        if (!isMoving && touchedCrystals.Count < 3 && targetList != null && targetList.Count > 0)
        {
            StartCoroutine(MoveHand(targetList));
        }

        // DetectionRadius内にある衝突可能なオブジェクトをチェックし、該当すれば手の開閉と衝突処理を実行
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, DetectionRadius, InteractablesLayer);
        if (colliders.Length > 0)
        {
            StartCoroutine(PerformHandOpenClose());
            StartCoroutine(HandleCrystalCollisions(colliders));
        }
    }

    /// <summary>
    /// 敵が衝突した水晶のカウントを減少させるためのメソッドです。
    /// シンプルにカウンタを1減らします。
    /// </summary>
    public void SetCurrentCrystalCount()
    {
        currentCrystalCount--;
    }

    /// <summary>
    /// 現在のhp、mpの状態および既に触れた水晶の数に基づいて、最適なターゲット水晶のリストを決定します。
    /// このメソッドでは以下の処理を行います：
    /// - 各水晶の属性に対して、hpが50未満の場合はHeal属性、mpが50未満の場合はIncreaseMana属性に高い重みを付与
    /// - 重み付けをもとに、ターゲット候補とその他候補に分類
    /// - 既に触れた水晶の数に応じて、必要なターゲット数（3, 2, または1）を決定し、リストを補完
    /// - リストが満たされない場合はnullで補い、最終的に順序を反転して返します
    /// </summary>
    private List<GameObject> SetTargetList()
    {
        var tList = new List<GameObject>();

        // hp, mp に基づいた属性ごとの重み付けを実施
        Dictionary<CrystalAttribute, float> weights = new Dictionary<CrystalAttribute, float>
        {
            { CrystalAttribute.Heal, hp < 50 ? 2.0f : 0.5f },
            { CrystalAttribute.Attack, 1.0f },
            { CrystalAttribute.IncreaseMana, mp < 50 ? 2.0f : 0.5f }
        };

        // 重み付けに基づいて、優先すべき水晶属性を決定
        CrystalAttribute targetType = DetermineTargetType(weights);

        List<GameObject> targetCrystals = new List<GameObject>();
        List<(GameObject crystal, float weight)> others = new List<(GameObject, float)>();

        // 各水晶オブジェクトの属性を確認し、ターゲット候補とその他候補に分類する
        foreach (var crystal in crystalList)
        {
            Crystal c = crystal.GetComponent<Crystal>();
            if (c != null)
            {
                if (c.CrystalAttribute == targetType)
                {
                    targetCrystals.Add(crystal);
                }
                else if (weights.ContainsKey(c.CrystalAttribute))
                {
                    others.Add((crystal, weights[c.CrystalAttribute]));
                }
            }
        }

        // その他候補を重みの高い順に並べ替え
        others = others.OrderByDescending(x => x.weight).ToList();

        // 触れた水晶数に応じて、必要なターゲット数を決定し、リストに追加する
        if (touchedCrystals.Count == 0)
        {
            while (tList.Count < 3 && targetCrystals.Count > 0)
            {
                tList.Add(targetCrystals[0]);
                targetCrystals.RemoveAt(0);
            }
            while (tList.Count < 3 && others.Count > 0)
            {
                tList.Add(others[0].crystal);
                others.RemoveAt(0);
            }
        }
        else if (touchedCrystals.Count == 1)
        {
            while (tList.Count < 2 && targetCrystals.Count > 0)
            {
                tList.Add(targetCrystals[0]);
                targetCrystals.RemoveAt(0);
            }
            while (tList.Count < 2 && others.Count > 0)
            {
                tList.Add(others[0].crystal);
                others.RemoveAt(0);
            }
        }
        else if (touchedCrystals.Count == 2)
        {
            while (tList.Count < 1 && targetCrystals.Count > 0)
            {
                tList.Add(targetCrystals[0]);
                targetCrystals.RemoveAt(0);
            }
            while (tList.Count < 1 && others.Count > 0)
            {
                tList.Add(others[0].crystal);
                others.RemoveAt(0);
            }
        }

        // 必要なターゲット数に満たない場合は、nullでリストを埋める
        while (tList.Count < 3)
        {
            tList.Add(null);
        }

        tList.Reverse();
        return tList;
    }

    /// <summary>
    /// 重み付けに基づいて、優先すべき水晶の属性を決定します。
    /// hpが50未満の場合はHeal属性、mpが50未満の場合はIncreaseMana属性を優先し、
    /// どちらの条件にも当てはまらなければAttack属性を返します。
    /// </summary>
    private CrystalAttribute DetermineTargetType(Dictionary<CrystalAttribute, float> weights)
    {
        if (hp < 50 && weights[CrystalAttribute.Heal] > weights[CrystalAttribute.IncreaseMana])
            return CrystalAttribute.Heal;
        if (mp < 50 && weights[CrystalAttribute.IncreaseMana] > weights[CrystalAttribute.Heal])
            return CrystalAttribute.IncreaseMana;
        return CrystalAttribute.Attack;
    }

    /// <summary>
    /// 指定されたターゲット水晶リストに向けて、手の移動を実行するコルーチンです。
    /// 各ターゲットまで1秒間でリニア補間により移動し、移動が完了したらリストから対象を除去します。
    /// もし衝突した水晶が3つに達した場合、移動処理を中断します。
    /// </summary>
    private IEnumerator MoveHand(List<GameObject> tList)
    {
        isMoving = true;
        Debug.Log("Starting hand movement.");
        if (tList == null || tList.Count == 0)
        {
            Debug.LogWarning("No targets available for movement.");
            isMoving = false;
            yield break;
        }

        for (int i = 0; i < tList.Count; i++)
        {
            var target = tList[i];
            if (target == null)
            {
                continue;
            }
            Vector3 targetPos = target.transform.position;
            float duration = 1f;
            float elapsed = 0f;
            Vector3 startPos = transform.position;
            // 1秒間でターゲット位置へ移動
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                transform.position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
                yield return null;
            }
            // 到達したターゲットはリストから除去
            tList.RemoveAt(i);
            i--;
            // 衝突した水晶が3つになった場合、移動処理を中断
            if (touchedCrystals.Count >= 3)
                break;
        }
        Debug.Log("Hand movement completed.");
        isMoving = false;
    }

    /// <summary>
    /// 手の開閉アニメーションを実行するコルーチンです。
    /// 手の開いた状態と閉じた状態のスプライトを一定時間切り替えることで、アニメーション効果を表現します。
    /// </summary>
    private IEnumerator PerformHandOpenClose()
    {
        SpriteRenderer sr = handPoints.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sprite = handPointSprite[1];
            yield return new WaitForSeconds(0.2f);
            sr.sprite = handPointSprite[0];
        }
    }

    /// <summary>
    /// 指定された衝突可能オブジェクトに対して、水晶との衝突処理を実施するコルーチンです。
    /// 処理内容は以下の通り：
    /// - 衝突処理中であれば中断
    /// - 各Collider2DについてCrystalコンポーネントを確認し、対象の水晶がtargetListに含まれるか検証
    /// - 対象の場合、以下の処理を実行：
    ///   * 衝突した水晶の属性をtouchedCrystalsに追加
    ///   * 対応するスキルアイコンを更新
    ///   * BattleLogicManagerを通じて水晶数を減少させる
    ///   * 衝突音を再生し、対象水晶オブジェクトを破棄する
    ///   * 触れた水晶が3つに達した場合、少し待機してからCrystalActionを実行し、touchedCrystalsをクリアする
    /// </summary>
    private IEnumerator HandleCrystalCollisions(Collider2D[] colliders)
    {
        if (isCollisionHandling)
        {
            Debug.Log("Collision handling already in progress.");
            yield break;
        }
        Debug.Log("Starting collision handling.");
        isCollisionHandling = true;
        foreach (Collider2D collider in colliders)
        {
            Crystal crystalComponent = collider.GetComponent<Crystal>();
            // targetList に含まれる水晶のみ処理を実施
            if (crystalComponent != null && targetList.Contains(crystalComponent.gameObject))
            {
                touchedCrystals.Add(crystalComponent.CrystalAttribute);
                // 対応するスキルアイコンを更新（skillListの順序は触れた順に対応）
                skillList[touchedCrystals.Count - 1].GetComponent<Image>().sprite = crystalComponent.Sprite;
                BattleLogicManager.ReduceCrystalCount(crystalComponent.gameObject);
                AudioManager.Instance.PlaySE(SESoundData.SE.GetPiece);
                Destroy(collider.gameObject);
                // 3つの水晶に触れた場合、0.5秒待機してからアクションを実行
                if (touchedCrystals.Count == 3)
                {
                    yield return new WaitForSeconds(0.5f);
                    CrystalAction();
                    touchedCrystals.Clear();
                }
                break;
            }
        }
        isCollisionHandling = false;
        Debug.Log("Collision handling completed.");
        yield break;
    }

    /// <summary>
    /// 触れた水晶の組み合わせに応じて、適切なアクションを実行するメソッドです。
    /// 最後に触れた水晶の属性を基に、同属性の水晶数（multiplier）を計算し、BattleLogicManagerに対応するアクションを設定します。
    /// </summary>
    public void CrystalAction()
    {
        if (touchedCrystals.Count > 0)
        {
            CrystalAttribute lastCrystal = touchedCrystals[touchedCrystals.Count - 1];
            int multiplier = touchedCrystals.Count(c => c == lastCrystal);
            switch (lastCrystal)
            {
                case CrystalAttribute.Heal:
                    BattleLogicManager.SetEnemyAction("Heal", multiplier);
                    break;
                case CrystalAttribute.Attack:
                    BattleLogicManager.SetEnemyAction("Attack", multiplier);
                    break;
                case CrystalAttribute.IncreaseMana:
                    BattleLogicManager.SetEnemyAction("IncreaseMana", multiplier);
                    break;
                default:
                    Debug.LogWarning("Unhandled Crystal Attribute");
                    break;
            }
        }
    }

    /// <summary>
    /// オブジェクトの状態をリセットし、不要なリソースを解放します。
    /// 具体的には、touchedCrystalsリストをクリアし、手の位置を初期位置（原点）に戻します。
    /// </summary>
    public void CleanupResources()
    {
        touchedCrystals?.Clear();
        if (this.transform != null)
            this.transform.position = Vector3.zero;
    }
}

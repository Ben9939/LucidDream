using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// BattleModeHandInteractionController は、戦闘モードにおける手操作（クリスタル収集等）の制御を行います。
/// 派生クラス用のリソースを基底クラスから注入し、戦闘モード固有の処理を実装します。
/// </summary>
public class BattleModeHandInteractionController : HandInteractionControllerBase
{
    // === 基底クラス用のリソース ===
    [Header("Base Class Resources")]
    [SerializeField] private GameObject myHandTracker;       // handTracker に注入するオブジェクト
    [Header("Hand Point Sprites")]
    [SerializeField] private List<Sprite> myHandPointSprite;   // handPointSprite に注入するスプライト
    [Header("Detection Settings")]
    [SerializeField] private float myDetectionRadius;        // detectionRadius に注入する値

    // === 戦闘モード固有のフィールド ===
    [Header("Battle Mode Fields")]
    [SerializeField] private LayerMask interactablesLayer;   // インタラクト対象レイヤー
    [Header("Skill UI")]
    [SerializeField] private List<GameObject> skillList;       // スキル用 UI リスト
    public BattleLogicManager BattleLogicManager;            // バトルロジック管理クラス

    // プライベート変数（camelCase）
    private List<CrystalAttribute> touchedCrystals = new List<CrystalAttribute>(); // 収集済みクリスタルリスト
    private bool isTouchedCrystalListFull = false;           // クリスタル収集完了フラグ
    private int maxCrystalCount = 3;                         // 最大収集クリスタル数
    private bool isCollisionHandling = false;                // 衝突処理中フラグ

    /// <summary>
    /// Awake：基底クラスの Awake() を呼び、リソースを初期化する
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        handTracker = myHandTracker;
        handPointSprite = myHandPointSprite;
        detectionRadius = myDetectionRadius;
    }

    /// <summary>
    /// モード更新処理を実行します（手の動作・位置更新、クリスタル収集処理）
    /// </summary>
    public override void ModeUpdate()
    {
        UpdateHandMotion();
        UpdateHandPosition();

        Collider2D[] colliders = GetInteractableColliders(interactablesLayer);
        bool currentHandState = handTracker.GetComponent<HandTracking>().handState;

        // 手の状態が変化し、かつクリスタル収集が未完了の場合
        if (currentHandState != lastHandState && !isTouchedCrystalListFull)
        {
            // 手が閉じた場合、クリスタル衝突処理を開始（非同期）
            if (!currentHandState)
            {
                HandleCrystalCollisionsAsync(colliders);
            }
            lastHandState = currentHandState;
        }
    }

    /// <summary>
    /// クリスタル衝突判定処理（async 実装）
    /// </summary>
    private async void HandleCrystalCollisionsAsync(Collider2D[] colliders)
    {
        if (isCollisionHandling)
            return;
        isCollisionHandling = true;

        foreach (Collider2D collider in colliders)
        {
            // "Ok(Clone)" の名前のオブジェクトを処理
            if (collider.name == "Ok(Clone)")
            {
                BattleLogicManager.SetIsCollectPhaseStart(true);
                Destroy(collider.gameObject);
            }
            if (BattleLogicManager.GetIsCollectPhaseStart())
            {
                Crystal crystalComponent = collider.GetComponent<Crystal>();
                if (crystalComponent != null)
                {
                    touchedCrystals.Add(crystalComponent.CrystalAttribute);
                    // UI 上のスキルイメージを更新
                    skillList[touchedCrystals.Count - 1].GetComponent<Image>().sprite = crystalComponent.Sprite;
                    BattleLogicManager.ReduceCrystalCount(collider.gameObject);
                    AudioManager.Instance.PlaySE(SESoundData.SE.GetPiece);
                    Destroy(collider.gameObject);

                    // 最大数集まったら 0.5 秒後にアクション実行
                    if (touchedCrystals.Count == maxCrystalCount)
                    {
                        await Task.Delay(500);
                        CrystalAction();
                        touchedCrystals.Clear();
                        isTouchedCrystalListFull = true;
                    }
                    break;
                }
            }
        }
        isCollisionHandling = false;
    }

    /// <summary>
    /// 収集したクリスタルに基づいてアクションを実行します
    /// </summary>
    private void CrystalAction()
    {
        if (touchedCrystals.Count > 0)
        {
            CrystalAttribute lastCrystal = touchedCrystals[touchedCrystals.Count - 1];
            int multiplier = touchedCrystals.Count(c => c == lastCrystal);
            switch (lastCrystal)
            {
                case CrystalAttribute.Heal:
                    BattleLogicManager.SetPlayerAction("Heal", multiplier);
                    break;
                case CrystalAttribute.Attack:
                    BattleLogicManager.SetPlayerAction("Attack", multiplier);
                    break;
                case CrystalAttribute.IncreaseMana:
                    BattleLogicManager.SetPlayerAction("IncreaseMana", multiplier);
                    break;
                default:
                    Debug.LogWarning("Unhandled crystal attribute");
                    break;
            }
        }
    }

    /// <summary>
    /// リソースのクリーンアップ処理を実行します
    /// </summary>
    public override void CleanupResources()
    {
        touchedCrystals?.Clear();
        lastHandState = false;
        isTouchedCrystalListFull = false;
        transform.position = Vector3.zero;
    }
}
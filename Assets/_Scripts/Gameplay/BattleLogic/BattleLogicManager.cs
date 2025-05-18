using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

/// <summary>
/// BattleLogicManager クラスは、戦闘中のロジック（フェーズ管理、水晶生成、アクション実行など）を統括します。
/// 戦闘は Prepare, CollectCrystals, FirstAction, SecondAction, EndTurn の各フェーズに分かれており、
/// 各フェーズで異なる処理を実施します。
/// </summary>
public class BattleLogicManager : MonoBehaviour
{
    #region Constants
    // スキルアイコン処理の待機時間
    private const float SKILL_CLEAR_DELAY = 0.5f;
    // アクション音声再生後の待機時間
    private const float ACTION_AUDIO_DELAY = 0.5f;
    // アクション後の待機時間
    private const float ACTION_WAIT_TIME = 1f;
    #endregion

    #region Enum & Fields

    // 戦闘フェーズの定義。各フェーズは順次実行され、戦闘全体の流れを制御する。
    public enum BattlePhase
    {
        Prepare,         // 戦闘開始前の準備フェーズ（水晶生成、キャラクター初期化など）
        CollectCrystals, // プレイヤーと敵が水晶を収集するフェーズ。外部入力により収集完了が通知される。
        FirstAction,     // 収集完了後、最初のアクション（例：攻撃、治療）の実行フェーズ
        SecondAction,    // 2 番目のアクション実行フェーズ
        EndTurn          // ターン終了処理フェーズ（次のターンに備えたリセット処理）
    }

    // 現在の戦闘フェーズ。初期フェーズは Prepare とする。
    private BattlePhase currentPhase = BattlePhase.Prepare;

    // 各フェーズで使用されるコルーチンを保持するための変数。
    private Coroutine currentCoroutine;

    [Header("Probability Settings")]
    [SerializeField] private int healthThreshold = 50;   // 血量の閥値。両キャラクターの血量がこの値未満なら低状態と判断する。
    [SerializeField] private int magicThreshold = 30;    // 魔力の閥値。両キャラクターの魔力がこの値未満なら低状態と判断する。

    [Header("UI & Prefabs")]
    [SerializeField] private GameObject magicPanel;  // 魔法用パネル。水晶収集フェーズ中に表示する。
    [SerializeField] private List<GameObject> playerSkillList; // プレイヤーのスキルアイコンのリスト
    [SerializeField] private List<GameObject> enemySkillList;  // 敵のスキルアイコンのリスト
    [SerializeField] private GameObject okSignPrefab;  // OK サインのプレハブ。準備完了時などに表示する。

    [Header("Crystal Settings")]
    // 生成する水晶の数
    [SerializeField] private int crystalCount = 9;
    [SerializeField] private GameObject attackCrystalPrefab; // 攻撃水晶のプレハブ
    [SerializeField] private GameObject healCrystalPrefab;   // 治療水晶のプレハブ
    [SerializeField] private GameObject manaCrystalPrefab;   // 魔力水晶のプレハブ

    [Header("Hand & Tracking")]
    [SerializeField] private GameObject playerHandPoint;   // プレイヤーの手部のポイント（UI上の操作対象）
    [SerializeField] private GameObject enemyHandPoint;    // 敵の手部のポイント
    [SerializeField] private GameObject handTrackingManager; // 手追跡管理用オブジェクト（手の動きを検出・追跡する）

    [Header("Boundary & Grid Settings")]
    [SerializeField] private float minX = -4f; // グリッドの X 軸最小値（シーン上の座標）
    [SerializeField] private float maxX = 4f;  // グリッドの X 軸最大値
    [SerializeField] private float minY = -3f; // グリッドの Y 軸最小値
    [SerializeField] private float maxY = 3f;  // グリッドの Y 軸最大値
    [SerializeField] private float cellSize = 1f; // グリッドの各セルのサイズ

    [Header("Timing Settings")]
    [SerializeField] private float crystalCreateTime = 0.5f; // 水晶生成アニメーションの再生時間

    [Header("Crystal Weight Settings - Normal")]
    [SerializeField] private float normalAttackWeight = 60f;
    [SerializeField] private float normalHealWeight = 20f;
    [SerializeField] private float normalManaWeight = 20f;

    [Header("Crystal Weight Settings - Low Health Only")]
    [SerializeField] private float lowHealthAttackWeight = 40f;
    [SerializeField] private float lowHealthHealWeight = 40f;
    [SerializeField] private float lowHealthManaWeight = 20f;

    [Header("Crystal Weight Settings - Low Magic Only")]
    [SerializeField] private float lowMagicAttackWeight = 40f;
    [SerializeField] private float lowMagicHealWeight = 20f;
    [SerializeField] private float lowMagicManaWeight = 40f;


    // 内部キャラクター参照（タグ "Player" と "Enemy" で取得）
    private GameObject player;
    private GameObject enemy;

    // 生成された水晶オブジェクトを保持するリスト
    private List<GameObject> crystalList = new List<GameObject>();
    // 生成される水晶の種類（インデックス）を保持するリスト
    private List<int> crystalIndices;

    // 各フェーズで実行するアクション設定。actionList, multiplierList, fighterList で各アクション情報を蓄積する。
    private List<string> actionList = new List<string>();   // 実行するアクション名のリスト
    private List<int> multiplierList = new List<int>();       // アクションの倍率（効果値など）のリスト
    private List<GameObject> fighterList = new List<GameObject>(); // アクションを実行するキャラクターのリスト
    private bool isPlayerActionSet = false; // プレイヤーのアクションが設定されたかどうかのフラグ
    private bool isEnemyActionSet = false;  // 敵のアクションが設定されたかどうかのフラグ

    // 水晶収集フェーズの制御用フラグ
    private bool isCollectPhaseStart = false;      // 外部（手部の衝突など）から設定され、収集フェーズ開始を示す
    private bool isCollectCrystalsPhaseEnd = false;  // 収集フェーズが終了したかどうかの判定フラグ

    // GridManager とレイヤー設定
    private GridManager gridManager; // 水晶配置などのグリッド管理用オブジェクト
    private string interactableLayerName = "Interactable"; // インタラクション対象のレイヤー名
    private int interactableLayer; // 上記レイヤーのレイヤー番号

    #endregion

    #region Singleton

    /// <summary>
    /// BattleLogicManager のシングルトンインスタンス
    /// </summary>
    public static BattleLogicManager Instance { get; private set; }

    #endregion

    #region Unity Events

    private void Awake()
    {
        // シングルトンパターンの実装：既に存在する場合は自身を破棄
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        // GridManager の初期化。グリッド範囲は minX, minY, maxX, maxY で指定された領域、セルサイズ cellSize を使用
        Boundary boundary = new Boundary(new Vector2(minX, minY), new Vector2(maxX, maxY));
        gridManager = new GridManager(boundary, cellSize);

        // 指定したレイヤー名からレイヤー番号を取得
        interactableLayer = LayerMask.NameToLayer(interactableLayerName);
    }

    private void OnEnable()
    {
        // 戦闘開始時の初期化処理を実行
        Initialize();
    }

    private void OnDisable()
    {
        // 戦闘終了時にキャラクターの状態をリセットし、ターン終了処理を実施
        ResetFighters();
        HandleEndTurnPhase();
    }

    #endregion

    #region Initialization

    /// <summary>
    /// 戦闘の初期化処理を実行します。
    /// 各フェーズの初期化、キャラクター（プレイヤー、敵）の取得と初期化、UI の設定を行います。
    /// </summary>
    public void Initialize()
    {
        currentPhase = BattlePhase.Prepare;
        isCollectCrystalsPhaseEnd = false;
        isCollectPhaseStart = false;

        // 手部のインタラクションコントローラーに本 BattleLogicManager インスタンスをセットする
        playerHandPoint.GetComponent<BattleModeHandInteractionController>().BattleLogicManager = this;
        enemyHandPoint.GetComponent<EnemyHandInteractionController>().BattleLogicManager = this;

        // タグからキャラクターを取得する（存在しない場合は警告となる）
        player = GameObject.FindGameObjectWithTag("Player");
        enemy = GameObject.FindGameObjectWithTag("Enemy");

        // 各キャラクターの FighterStats コンポーネントを初期化する
        player?.GetComponent<FighterStats>()?.Initialize();
        enemy?.GetComponent<FighterStats>()?.Initialize();

        // 魔法パネルの表示設定
        if (magicPanel != null)
            magicPanel.SetActive(true);
    }

    #endregion

    #region UpdateBattle

    /// <summary>
    /// 戦闘状態の更新処理を実行します。
    /// 外部の Update() ループから呼び出し、各フェーズごとの処理を開始します。
    /// </summary>
    public void UpdateBattle()
    {
        // どちらかのキャラクターが死亡している場合、更新処理を中断する
        if (GetFighterDeath())
            return;

        // 現在実行中のコルーチンがない場合、フェーズに応じた処理を開始する
        if (currentCoroutine == null)
        {
            switch (currentPhase)
            {
                case BattlePhase.Prepare:
                    currentCoroutine = StartCoroutine(HandlePreparePhase());
                    break;
                case BattlePhase.CollectCrystals:
                    HandleCollectCrystalsPhase();
                    break;
                case BattlePhase.FirstAction:
                    currentCoroutine = StartCoroutine(HandleFirstActionPhase());
                    break;
                case BattlePhase.SecondAction:
                    currentCoroutine = StartCoroutine(HandleSecondActionPhase());
                    break;
                case BattlePhase.EndTurn:
                    HandleEndTurnPhase();
                    break;
            }
        }

        // 外部から水晶収集フェーズ開始の指示が来た場合、フェーズを切り替える
        if (isCollectPhaseStart)
        {
            currentPhase = BattlePhase.CollectCrystals;
            currentCoroutine = null;
        }

        // Prepare および CollectCrystals フェーズ中は、プレイヤーの手部更新と手追跡の処理を実行する
        if (currentPhase == BattlePhase.Prepare || currentPhase == BattlePhase.CollectCrystals)
        {
            playerHandPoint.GetComponent<BattleModeHandInteractionController>()?.ModeUpdate();
            handTrackingManager.GetComponent<HandTracking>()?.HandleHandTrackingUpdate();
        }

        // CollectCrystals フェーズ中は、敵側の手部更新も実行する
        if (currentPhase == BattlePhase.CollectCrystals)
        {
            enemyHandPoint.GetComponent<EnemyHandInteractionController>()?.HandleEnemyHandPointUpdate();
        }

        // 各フレームで、収集フェーズの終了条件をチェックする
        if (!isCollectCrystalsPhaseEnd)
        {
            CheckCollectCrystalsPhaseEnd();
        }
    }

    #endregion

    #region Cleanup

    /// <summary>
    /// 戦闘終了時のリソース解放処理を実行します。
    /// 現在実行中のコルーチンを停止し、生成した水晶をすべて削除します。
    /// </summary>
    public void Cleanup()
    {
        if (currentCoroutine != null)
            StopCoroutine(currentCoroutine);

        foreach (var crystal in crystalList)
            Destroy(crystal);
        crystalList.Clear();
    }

    #endregion

    #region Phase Handlers

    /// <summary>
    /// Prepare フェーズの処理。  
    /// ・水晶を生成し、魔法パネルを表示  
    /// ・敵側の手部を初期化  
    /// ・OK サインを生成して、プレイヤーに操作可能な状態を示す
    /// </summary>
    private IEnumerator HandlePreparePhase()
    {
        // 水晶生成処理：CreateCrystals() で水晶オブジェクトのリストを取得
        crystalList = CreateCrystals();
        magicPanel?.SetActive(true);

        // 敵側手部の初期化（遅延処理等を内部で実施）
        enemyHandPoint.GetComponent<EnemyHandInteractionController>()?.Initialized();

        // OK サインの生成（プレイヤーが準備完了を確認できるように表示）
        Vector3 okSignPosition = Vector3.zero; // OK サインの生成位置は仮設定（必要に応じて変更）
        Quaternion okSignRotation = Quaternion.identity;
        GameObject okSign = Instantiate(okSignPrefab, okSignPosition, okSignRotation);
        okSign.layer = interactableLayer;

        // 水晶生成アニメーションの再生時間分待機する
        yield return new WaitForSeconds(crystalCreateTime);
    }

    /// <summary>
    /// CollectCrystals フェーズの処理。  
    /// このフェーズでは外部入力（手部の衝突など）による水晶収集が行われるため、
    /// 内部処理としては何もしない（currentCoroutine を null に設定）。
    /// </summary>
    private void HandleCollectCrystalsPhase()
    {
        currentCoroutine = null;
    }

    /// <summary>
    /// 水晶収集フェーズが終了しているかチェックします。  
    /// プレイヤーと敵の両方のアクションが設定されている場合、フェーズを FirstAction に切り替えます。
    /// </summary>
    private void CheckCollectCrystalsPhaseEnd()
    {
        if (isPlayerActionSet && isEnemyActionSet)
        {
            // 両キャラクターのアクション設定フラグをリセット
            isPlayerActionSet = false;
            isEnemyActionSet = false;
            isCollectPhaseStart = false;
            isCollectCrystalsPhaseEnd = true;

            // プレイヤーの手部リソースを解放
            playerHandPoint.GetComponent<BattleModeHandInteractionController>()?.CleanupResources();

            // 水晶収集フェーズのフラグリセットと生成済み水晶の削除
            isCollectCrystalsPhaseEnd = false;
            foreach (var crystal in crystalList)
                Destroy(crystal);
            crystalList.Clear();

            // フェーズを FirstAction に切り替え、魔法パネルを非表示にする
            currentPhase = BattlePhase.FirstAction;
            if (magicPanel != null)
                magicPanel.SetActive(false);
            currentCoroutine = null;
        }
    }

    /// <summary>
    /// FirstAction フェーズの処理。  
    /// 最初に実行されるアクションを、対象キャラクター（プレイヤーまたは敵）に対して実行し、
    /// アクション完了後に次フェーズ（SecondAction）に切り替えます。
    /// </summary>
    private IEnumerator HandleFirstActionPhase()
    {
        // リストに必要なデータが十分にあるかチェック
        if (fighterList.Count < 1 || actionList.Count < 1 || multiplierList.Count < 1)
        {
            Debug.LogError("FirstActionPhase: Insufficient data!");
            yield break;
        }

        Debug.Log($"{fighterList[0].name} performs first action!");

        // 対象がプレイヤーの場合、プレイヤースキルアイコンを順次処理する
        if (fighterList[0].name == "Player")
        {
            foreach (GameObject skill in playerSkillList)
            {
                AudioManager.Instance.PlaySE(SESoundData.SE.PlacePiece);
                yield return new WaitForSeconds(SKILL_CLEAR_DELAY);
                // スキルアイコンをクリア（null に設定）
                skill.GetComponent<Image>().sprite = null;
            }
        }
        // 対象が敵の場合、敵側スキルアイコンを処理する
        else if (fighterList[0].name == "Enemy")
        {
            foreach (GameObject skill in enemySkillList)
            {
                AudioManager.Instance.PlaySE(SESoundData.SE.PlacePiece);
                yield return new WaitForSeconds(SKILL_CLEAR_DELAY);
                skill.GetComponent<Image>().sprite = null;
            }
        }

        // 対象キャラクターの FighterAction コンポーネントを用いてアクションを選択実行する
        fighterList[0].GetComponent<FighterAction>().SelectAction(actionList[0], multiplierList[0]);

        // アクションに伴う音声再生処理（PlaySE コルーチン）を実行し待機する
        yield return StartCoroutine(PlaySE(actionList[0], multiplierList[0]));
        yield return new WaitForSecondsRealtime(ACTION_WAIT_TIME);

        // フェーズを SecondAction に切り替え、currentCoroutine をリセットする
        currentPhase = BattlePhase.SecondAction;
        currentCoroutine = null;
    }

    /// <summary>
    /// SecondAction フェーズの処理。  
    /// 2 番目に実行されるアクションを対象キャラクターに実行し、処理完了後に EndTurn フェーズへ移行します。
    /// </summary>
    private IEnumerator HandleSecondActionPhase()
    {
        if (fighterList.Count < 2 || actionList.Count < 2 || multiplierList.Count < 2)
        {
            Debug.LogError("SecondActionPhase: Insufficient data!");
            yield break;
        }

        Debug.Log($"{fighterList[1].name} performs second action!");

        if (fighterList[1].name == "Player")
        {
            foreach (GameObject skill in playerSkillList)
            {
                AudioManager.Instance.PlaySE(SESoundData.SE.PlacePiece);
                yield return new WaitForSeconds(SKILL_CLEAR_DELAY);
                skill.GetComponent<Image>().sprite = null;
            }
        }
        else if (fighterList[1].name == "Enemy")
        {
            foreach (GameObject skill in enemySkillList)
            {
                AudioManager.Instance.PlaySE(SESoundData.SE.PlacePiece);
                yield return new WaitForSeconds(SKILL_CLEAR_DELAY);
                skill.GetComponent<Image>().sprite = null;
            }
        }

        fighterList[1].GetComponent<FighterAction>().SelectAction(actionList[1], multiplierList[1]);
        yield return StartCoroutine(PlaySE(actionList[1], multiplierList[1]));
        yield return new WaitForSecondsRealtime(ACTION_WAIT_TIME);

        // フェーズを EndTurn に切り替え、currentCoroutine をリセットする
        currentPhase = BattlePhase.EndTurn;
        currentCoroutine = null;
    }

    /// <summary>
    /// EndTurn フェーズの処理。  
    /// 各アクションリスト、倍率、キャラクターリストをクリアし、手部のリソースを解放する。
    /// また、生成した水晶を削除し、フェーズを Prepare にリセットする。
    /// </summary>
    private void HandleEndTurnPhase()
    {
        // アクション関連リストのクリア
        actionList.Clear();
        multiplierList.Clear();
        fighterList.Clear();

        // プレイヤー・敵手部のリソース解放
        playerHandPoint.GetComponent<BattleModeHandInteractionController>()?.CleanupResources();
        enemyHandPoint.GetComponent<EnemyHandInteractionController>()?.CleanupResources();

        // 次のターンに備えてフェーズを初期状態に戻す
        currentPhase = BattlePhase.Prepare;
        currentCoroutine = null;
    }

    #endregion

    #region External Call Methods

    /// <summary>
    /// 水晶収集フェーズ開始フラグを取得します。
    /// </summary>
    public bool GetIsCollectPhaseStart() => isCollectPhaseStart;

    /// <summary>
    /// 水晶収集フェーズ開始フラグを設定します。外部から呼ばれて収集フェーズ開始を通知します。
    /// </summary>
    public void SetIsCollectPhaseStart(bool value) => isCollectPhaseStart = value;

    /// <summary>
    /// 指定された水晶オブジェクトをリストから削除します。
    /// </summary>
    public void ReduceCrystalCount(GameObject crystal)
    {
        if (crystalList.Contains(crystal))
            crystalList.Remove(crystal);
    }

    /// <summary>
    /// 現在生成されている水晶オブジェクトのリストを返します。
    /// </summary>
    public List<GameObject> GetCrystalList() => crystalList;

    /// <summary>
    /// プレイヤーのアクション（アクション名と倍率）を設定し、対象キャラクターとしてプレイヤーを登録します。
    /// </summary>
    public void SetPlayerAction(string action, int multiplier)
    {
        actionList.Add(action);
        multiplierList.Add(multiplier);
        fighterList.Add(player);
        isPlayerActionSet = true;
    }

    /// <summary>
    /// 敵のアクション（アクション名と倍率）を設定し、対象キャラクターとして敵を登録します。
    /// </summary>
    public void SetEnemyAction(string action, int multiplier)
    {
        actionList.Add(action);
        multiplierList.Add(multiplier);
        fighterList.Add(enemy);
        isEnemyActionSet = true;
    }

    #endregion

    #region Crystal Generation & Helpers

    /// <summary>
    /// キャラクターの死亡状態をチェックします。  
    /// プレイヤーまたは敵が死亡している場合、true を返します。
    /// </summary>
    private bool GetFighterDeath()
    {
        return (enemy && enemy.GetComponent<FighterStats>().GetDead()) ||
               (player && player.GetComponent<FighterStats>().GetDead());
    }

    /// <summary>
    /// 水晶オブジェクトの生成処理を実行します。  
    /// 各水晶は GridManager によるグリッド上の空き位置に配置され、生成時に拡大アニメーションを再生します。
    /// </summary>
    private List<GameObject> CreateCrystals()
    {
        // 利用可能な水晶プレハブを配列に格納
        GameObject[] crystalPrefabs = { attackCrystalPrefab, healCrystalPrefab, manaCrystalPrefab };
        List<GameObject> newCrystals = new List<GameObject>();

        // 生成する水晶の種類リストを作成
        crystalIndices = CreateCrystalList();

        // グリッドの状態を初期化
        gridManager.ClearGrid();

        // 各水晶の生成処理
        foreach (int index in crystalIndices)
        {
            // 空いているグリッド位置を取得
            Vector3? pos = gridManager.GetAvailableGridPosition();
            if (pos.HasValue)
            {
                // プレハブから水晶オブジェクトを生成
                GameObject crystal = Instantiate(crystalPrefabs[index], pos.Value, Quaternion.identity);
                crystal.transform.localScale = Vector3.zero; // 初期スケールはゼロにしてアニメーションで拡大
                crystal.layer = interactableLayer;
                newCrystals.Add(crystal);

                // グリッド上のその位置を占有状態に設定
                gridManager.OccupyGridPosition(pos.Value);

                // 水晶生成アニメーションを再生（指定時間 crystalCreateTime で拡大）
                StartCoroutine(PlayScaleAnimation(crystal, Vector3.one, crystalCreateTime));
            }
            else
            {
                Debug.LogWarning("No available grid position for crystal.");
            }
        }
        return newCrystals;
    }

    /// <summary>
    /// 指定したオブジェクトのスケールを、指定時間内にリニアに変化させるアニメーションを実行します。
    /// </summary>
    private IEnumerator PlayScaleAnimation(GameObject target, Vector3 targetScale, float duration)
    {
        Vector3 initialScale = target.transform.localScale;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            target.transform.localScale = Vector3.Lerp(initialScale, targetScale, t);
            yield return null;
        }
        target.transform.localScale = targetScale;
    }

    /// <summary>
    /// 水晶の種類リストを生成します。  
    /// プレイヤーと敵の健康状態および魔力状態に応じて、生成する水晶の種類の重み付けを調整します。
    /// </summary>
    private List<int> CreateCrystalList()
    {
        List<int> list = new List<int>();
        bool lowHealth = false;
        bool lowMagic = false;

        // プレイヤーと敵が存在する場合、各キャラクターの FighterStats を取得し状態を評価する
        if (player != null && enemy != null)
        {
            FighterStats playerStats = player.GetComponent<FighterStats>();
            FighterStats enemyStats = enemy.GetComponent<FighterStats>();
            if (playerStats != null && enemyStats != null)
            {
                // 両キャラクターの血量が閥値未満なら低健康状態と判断
                lowHealth = (playerStats.health < healthThreshold && enemyStats.health < healthThreshold);
                // 両キャラクターの魔力が閥値未満なら低魔力状態と判断
                lowMagic = (playerStats.magic < magicThreshold && enemyStats.magic < magicThreshold);
            }
        }

        // crystalCount (または DEFAULT_CRYSTAL_COUNT) 個の水晶を生成するため、各水晶の種類を重み付けにより決定する
        for (int i = 0; i < crystalCount; i++)
        {
            int crystalIndex = GetCrystalBasedOnProbability(lowHealth, lowMagic);
            list.Add(crystalIndex);
        }
        return list;
    }


    /// <summary>
    /// 現在の状態（低状態か否か）に応じた確率で、水晶の種類を決定します。
    /// 通常状態では攻撃水晶の出現確率を高く、低状態では治療・魔力水晶の出現確率を高く設定します。
    /// </summary>
    private int GetCrystalBasedOnProbability(bool lowHealth, bool lowMagic)
    {
        float attackWeight = (lowHealth && lowMagic) ? (lowHealthAttackWeight + lowMagicAttackWeight) / 2f
                              : lowHealth ? lowHealthAttackWeight
                              : lowMagic ? lowMagicAttackWeight
                              : normalAttackWeight;

        float healWeight = (lowHealth && lowMagic) ? (lowHealthHealWeight + lowMagicHealWeight) / 2f
                            : lowHealth ? lowHealthHealWeight
                            : lowMagic ? lowMagicHealWeight
                            : normalHealWeight;

        float manaWeight = (lowHealth && lowMagic) ? (lowHealthManaWeight + lowMagicManaWeight) / 2f
                            : lowHealth ? lowHealthManaWeight
                            : lowMagic ? lowMagicManaWeight
                            : normalManaWeight;

        float totalWeight = attackWeight + healWeight + manaWeight;
        float roll = Random.value * totalWeight;

        if (roll < attackWeight)
            return 0; // AttackCrystal
        else if (roll < attackWeight + healWeight)
            return 1; // HealCrystal
        else
            return 2; // ManaCrystal
    }



    /// <summary>
    /// 指定されたアクションとレベルに応じた音声効果を再生するコルーチンです。
    /// アクション名により再生する音声を選択し、一定時間待機します。
    /// </summary>
    private IEnumerator PlaySE(string action, int level)
    {
        switch (action)
        {
            case "Heal":
                if (level == 1 || level == 2)
                    AudioManager.Instance.PlaySE(SESoundData.SE.Healing_Lv1);
                else
                    AudioManager.Instance.PlaySE(SESoundData.SE.Healing_Lv2);
                yield return new WaitForSeconds(ACTION_AUDIO_DELAY);
                break;
            case "Attack":
                if (level == 1)
                    AudioManager.Instance.PlaySE(SESoundData.SE.Attack_Lv1);
                else if (level == 2)
                    AudioManager.Instance.PlaySE(SESoundData.SE.Attack_Lv2);
                else if (level == 3)
                    AudioManager.Instance.PlaySE(SESoundData.SE.Attack_Lv3);
                yield return new WaitForSeconds(ACTION_AUDIO_DELAY);
                break;
            case "IncreaseMana":
                if (level == 1 || level == 2)
                    AudioManager.Instance.PlaySE(SESoundData.SE.IncreaseMana_Lv1);
                else
                    AudioManager.Instance.PlaySE(SESoundData.SE.IncreaseMana_Lv2);
                yield return new WaitForSeconds(ACTION_AUDIO_DELAY);
                break;
        }
    }

    /// <summary>
    /// 戦闘終了時に、各キャラクターの FighterStats をリセットする。
    /// fighterList を ToList() でコピーしてから処理することで、リストの変更による例外を回避する。
    /// </summary>
    private void ResetFighters()
    {
        foreach (var fighter in fighterList.ToList())
        {
            fighter?.GetComponent<FighterStats>()?.ResetStats();
        }
    }

    #endregion
}


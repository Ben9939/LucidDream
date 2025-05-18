using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// 各種エフェクトの種類を示す列挙体
/// Attack：攻撃エフェクト、Heal：回復エフェクト、IncreaseMana：mp増加エフェクト
/// </summary>
public enum Effect
{
    Attack,
    Heal,
    IncreaseMana,
}

/// <summary>
/// 戦闘中の各キャラクターのステータスを管理するクラス。
/// ・hp（体力）とmp（魔力）の管理、ダメージ受け、回復、mpの更新を行います。
/// ・エフェクトの生成、UIバーの更新、キャラクターのアニメーションや振動・点滅エフェクトも実装しています。
/// </summary>
public class FighterStats : MonoBehaviour, IComparable
{
    // UIバーの参照
    [SerializeField] private GameObject healthFill; // hpバー
    [SerializeField] private GameObject magicFill;  // mpバー

    // キャラクターやシーン管理用のオブジェクト参照
    [SerializeField] private GameObject owner;
    [SerializeField] private GameObject GameControllerObj;
    [SerializeField] Text battleResultText;
    [SerializeField] private GameObject HandTrackingManager;

    // エフェクトプレハブのリスト（Attack, Heal, IncreaseManaの順）
    [SerializeField] private List<GameObject> effectPrefabList;

    [Header("Stats")]
    // キャラクターの基本ステータス（hp, mp, 近接攻撃力, 回復力）
    public float health;
    public float magic;
    public float melee;
    public float healing;

    // 初期値としてのhp, mp（バーの更新用）
    private float startHP;
    private float startMP;

    // 内部状態の管理
    private bool dead = false;
    private Transform healthTransform;
    private Transform magicTransform;
    private float resShowingTime = 3f; // 結果表示時間
    private Vector2 healthScale;
    private Vector2 magicScale;
    private float xNewHealthScale;
    private float xNewMagicScale;

    // エフェクト用のコンポーネント
    private SpriteRenderer spriteRenderer; // キャラクターのスプライト（点滅効果用）
    [SerializeField] private float flashDuration = 0.1f; // 点滅時間
    [SerializeField] private int flashCount = 3;         // 点滅回数
    [SerializeField] private float shakeIntensity = 0.1f;  // 振動の強さ
    [SerializeField] private float shakeDuration = 0.5f;   // 振動の持続時間

    // プレイヤーおよび敵の参照とステータスリスト
    GameObject Player;
    GameObject enemy;
    private List<FighterStats> fighterStats;

    [Header("ActiveScenarioState")]
    [SerializeField] private ScenarioStateSO activeScenarioState;

    /// <summary>
    /// Awakeで初期設定を行います。
    /// ・UIバーのTransformとスケールの取得
    /// ・初期hp, mpの保存
    /// ・スプライトレンダラーの取得
    /// </summary>
    void Awake()
    {
        // UIバーの参照と初期スケールの保存
        healthTransform = healthFill.GetComponent<RectTransform>();
        healthScale = healthFill.transform.localScale;
        magicTransform = magicFill.GetComponent<RectTransform>();
        magicScale = magicFill.transform.localScale;

        // 初期hpとmpの設定（シーン開始時の値を保持）
        startHP = health;
        startMP = magic;

        // エフェクト用のスプライトレンダラー取得
        spriteRenderer = owner.GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// プレイヤーと敵のFighterStatsを初期化し、リストに格納します。
    /// これにより、後のターン管理やステータス比較に利用します。
    /// </summary>
    public void Initialize()
    {
        fighterStats = new List<FighterStats>();

        // プレイヤーのステータスを取得してリストに追加
        Player = GameObject.FindGameObjectWithTag("Player");
        FighterStats currentFighterStats = Player.GetComponent<FighterStats>();
        fighterStats.Add(currentFighterStats);

        // 敵のステータスを取得してリストに追加
        enemy = GameObject.FindGameObjectWithTag("Enemy");
        FighterStats currentEnemyStats = enemy.GetComponent<FighterStats>();
        fighterStats.Add(currentEnemyStats);
    }

    /// <summary>
    /// ダメージを受けた際の処理を行うコルーチン。
    /// ・hpを減少させ、攻撃エフェクトを生成
    /// ・スプライトの点滅や振動効果を再生
    /// ・hpが0以下になった場合、勝敗の結果を表示しシナリオを進行させる
    /// ・hpバーのスケールを更新してUIに反映
    /// </summary>
    /// <param name="damage">受けるダメージ量</param>
    public IEnumerator ReceiveDamage(float damage)
    {
        // ダメージ分hpを減少
        health -= damage;

        // 攻撃エフェクトの生成（effectPrefabListのAttackエフェクトを利用）
        if (effectPrefabList[(int)Effect.Attack] != null)
        {
            GameObject effect = Instantiate(effectPrefabList[(int)Effect.Attack], owner.transform.position, Quaternion.identity);
            Destroy(effect, 0.4f);
        }

        // 点滅と振動のエフェクトを再生
        if (spriteRenderer != null)
        {
            StartCoroutine(FlashSprite());
            StartCoroutine(ShakeObject());
        }

        // hpが0以下の場合、キャラクターの死亡処理と勝敗判定を実施
        if (health <= 0)
        {
            // hpバーを0に設定
            xNewHealthScale = 0;
            healthFill.transform.localScale = new Vector2(xNewHealthScale, healthScale.y);

            if (owner.name == "Enemy")
            {
                battleResultText.text = "You Win";
                dead = true;
                Debug.Log("Enemy defeated. You Win.");
                AudioManager.Instance.StopBGM();
                AudioManager.Instance.PlaySE(SESoundData.SE.BattleResult_Win);
                activeScenarioState.Active();
                yield return new WaitForSeconds(resShowingTime);

            }
            else if (owner.name == "Player")
            {
                battleResultText.text = "You Lose";
                dead = true;
                Debug.Log("Player defeated. You Lose.");
                AudioManager.Instance.StopBGM();
                AudioManager.Instance.PlaySE(SESoundData.SE.BattleResult_Lose);
                yield return new WaitForSeconds(resShowingTime);
            }

            // リソース解放やシナリオ進行処理を実行
            HandTrackingManager.GetComponent<UDPReceive>().ReleaseResources();
            battleResultText.text = "";
            GameStateManager.Instance.SwitchState(GameState.FreeRoam);
        }
        // hpが残っている場合、ダメージアニメーションを再生しUIバーを更新
        else if (damage > 0)
        {
            owner.GetComponent<Animator>().Play("Damage");
            xNewHealthScale = healthScale.x * (health / startHP);
            healthFill.transform.localScale = new Vector2(xNewHealthScale, healthScale.y);
        }
    }

    /// <summary>
    /// mpを増加させる処理を行います。
    /// ・IncreaseManaエフェクトを生成し、点滅エフェクトを実行
    /// ・mpバーを更新して、mpの増加をUIに反映します。
    /// </summary>
    /// <param name="amount">mp増加量</param>
    public void UpdateMagic(float amount)
    {
        if (effectPrefabList[(int)Effect.IncreaseMana] != null)
        {
            GameObject effect = Instantiate(effectPrefabList[(int)Effect.IncreaseMana], owner.transform.position, Quaternion.identity);
            Destroy(effect, 0.4f);
        }
        if (spriteRenderer != null)
        {
            StartCoroutine(FlashSprite());
        }
        UpdateMagicFill(amount);
    }

    /// <summary>
    /// hpを回復させる処理を行います。
    /// ・Healエフェクトを生成し、点滅エフェクトを実行
    /// ・hpバーの更新処理（UpdateHpFill）を呼び出して、回復をUIに反映します。
    /// </summary>
    /// <param name="amount">回復量</param>
    public void UpdateHealth(float amount)
    {
        if (effectPrefabList[(int)Effect.Heal] != null)
        {
            GameObject effect = Instantiate(effectPrefabList[(int)Effect.Heal], owner.transform.position, Quaternion.identity);
            Destroy(effect, 0.4f);
        }
        if (spriteRenderer != null)
        {
            StartCoroutine(FlashSprite());
        }
        UpdateHealthFill(amount);
    }

    /// <summary>
    /// hpバーの更新を行うメソッド。
    /// hpが初期値未満の場合、回復量を加算し、Clampで0からstartHPまでに収めた後、バーのスケールを更新します。
    /// </summary>
    /// <param name="amount">加算する回復量</param>
    public void UpdateHealthFill(float amount)
    {
        if (health < startHP)
        {
            health += amount;
            health = Mathf.Clamp(health, 0f, startHP);
            xNewHealthScale = healthScale.x * (health / startHP);
            healthFill.transform.localScale = new Vector2(xNewHealthScale, healthScale.y);
        }
    }

    /// <summary>
    /// mpバーの更新を行うメソッド。
    /// mpの変化量を反映し、0からstartMPの範囲にClampした上で、mpバーのスケールを更新します。
    /// </summary>
    /// <param name="change">mpに対する増減値</param>
    public void UpdateMagicFill(float change)
    {
        float newMp = magic + change;
        if (newMp >= 0 && newMp <= startMP)
        {
            magic = newMp;
        }
        else if (newMp < 0)
        {
            magic = 0;
        }
        else if (newMp > startMP)
        {
            magic = startMP;
        }

        xNewMagicScale = magicScale.x * (magic / startMP);
        magicFill.transform.localScale = new Vector2(xNewMagicScale, magicScale.y);
    }

    /// <summary>
    /// キャラクターの現在のhpとmpを返すメソッド。
    /// タプル形式で (hp, mp) を返します。
    /// </summary>
    /// <returns>現在のhpとmp</returns>
    public (float health, float magic) GetStats()
    {
        return (health, magic);
    }

    /// <summary>
    /// キャラクターが死亡状態かどうかを返すメソッド。
    /// </summary>
    /// <returns>死亡している場合はtrue、そうでなければfalse</returns>
    public bool GetDead()
    {
        return dead;
    }

    /// <summary>
    /// プレイヤーと敵のステータスを初期状態にリセットします。
    /// ・hp, mpを初期値に戻し、UIバーのスケールも更新します。
    /// </summary>
    public void ResetStats()
    {
        // hpとmpを初期値にリセット
        health = 100;
        magic = 100;

        // UIバーを初期スケールに戻す
        xNewHealthScale = healthScale.x;
        healthFill.transform.localScale = new Vector2(xNewHealthScale, healthScale.y);

        xNewMagicScale = magicScale.x;
        magicFill.transform.localScale = new Vector2(xNewMagicScale, magicScale.y);

        dead = false;
    }

    /// <summary>
    /// IComparableインターフェースの実装（現状は固定値0を返す）。
    /// 今後、ターン順等の比較処理を実装する際に利用可能。
    /// </summary>
    /// <param name="otherStats">比較対象のFighterStats</param>
    /// <returns>比較結果（現在は0固定）</returns>
    public int CompareTo(object otherStats)
    {
        return 0;
    }

    /// <summary>
    /// キャラクターのスプライトを一定時間点滅させるコルーチン。
    /// 点滅回数と点滅時間はflashCountとflashDurationで制御します。
    /// </summary>
    private IEnumerator FlashSprite()
    {
        Color originalColor = spriteRenderer.color;
        for (int i = 0; i < flashCount; i++)
        {
            // 透明度を50%に下げる
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.5f);
            yield return new WaitForSeconds(flashDuration);
            // 元の色に戻す
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(flashDuration);
        }
    }

    /// <summary>
    /// キャラクターの位置を短時間ランダムに振動させるコルーチン。
    /// 振動の強さと継続時間はshakeIntensityとshakeDurationで制御されます。
    /// </summary>
    private IEnumerator ShakeObject()
    {
        Vector3 originalPosition = transform.localPosition;
        float elapsedTime = 0f;

        while (elapsedTime < shakeDuration)
        {
            // X, Y方向にランダムなオフセットを追加
            float offsetX = UnityEngine.Random.Range(-shakeIntensity, shakeIntensity);
            float offsetY = UnityEngine.Random.Range(-shakeIntensity, shakeIntensity);
            transform.localPosition = new Vector3(originalPosition.x + offsetX, originalPosition.y + offsetY, originalPosition.z);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        // 元の位置に戻す
        transform.localPosition = originalPosition;
    }
}

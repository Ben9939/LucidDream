using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// クレジット画面のスクロール処理を管理するクラス。
/// ControllerBaseを継承し、クレジット生成とスクロールを実施する。
/// </summary>
public class CreditScrolleController : ControllerBase
{
    // クレジットデータの参照
    [Header("Credit Data")]
    [SerializeField] private CreditDataSO creditData;

    // UIプレハブの参照（段落タイトルと各クレジット項目用）
    [Header("UI Prefabs")]
    [SerializeField] private GameObject titleTextPrefab;   // 段落タイトル表示用
    [SerializeField] private GameObject creditItemPrefab;    // クレジット項目表示用

    // クレジット項目を配置するUIコンテナ
    [Header("UI Container")]
    [SerializeField] private RectTransform creditsContainer;

    // スクロール設定
    [Header("Scroll Settings")]
    [SerializeField] private float scrollSpeed = 20f;   // スクロール速度
    [SerializeField] private float startDelay = 1f;       // スクロール開始前の遅延時間

    /// <summary>
    /// ゲーム状態開始時の処理。
    /// クレジットUIの生成とスクロールの開始を行う。
    /// </summary>
    protected override void Initialize()
    {
        GenerateCredits();
        StartCoroutine(ScrollCredits());
        AudioManager.Instance.PlayBGM(BGMSoundData.BGM.Ending);
    }

    /// <summary>
    /// ゲーム状態更新時の処理（今回は使用しない）。
    /// </summary>
    protected override void OnUpdate() { }

    /// <summary>
    /// ゲーム状態終了時の処理（今回は使用しない）。
    /// </summary>
    protected override void Cleanup() { }

    /// <summary>
    /// クレジットUIを動的に生成する。
    /// 各CreditSectionごとに段落タイトルと、そのセクションに属するCreditItemを生成する。
    /// </summary>
    private void GenerateCredits()
    {
        if (creditData == null || creditsContainer == null)
        {
            Debug.LogWarning("CreditData or CreditsContainer is null");
            return;
        }

        // コンテナ内の既存の子オブジェクトを全て削除
        foreach (Transform child in creditsContainer)
        {
            Destroy(child.gameObject);
        }

        // 各セクションごとに処理を実施
        foreach (CreditSection section in creditData.sections)
        {
            // セクションタイトルがある場合、タイトルUIを生成
            if (!string.IsNullOrEmpty(section.sectionTitle) && titleTextPrefab != null)
            {
                GameObject titleGO = Instantiate(titleTextPrefab, creditsContainer);
                // TextまたはTMP_Textコンポーネントを探してテキスト設定
                Text titleText = titleGO.GetComponent<Text>();
                if (titleText != null)
                {
                    titleText.text = section.sectionTitle;
                }
               
            }

            // セクション内の各クレジット項目を生成
            if (section.creditItems != null && creditItemPrefab != null)
            {
                foreach (CreditItem item in section.creditItems)
                {
                    GameObject itemGO = Instantiate(creditItemPrefab, creditsContainer);

                    // "CreditText"の子オブジェクトを探し、テキストを設定
                    //Transform textTransform = itemGO.transform.Find("CreditText");
                    //if (textTransform != null)
                    //{
                        Text itemText = itemGO.GetComponent<Text>();
                        if (itemText != null)
                        {
                            itemText.text = item.creditText;
                        }
                        
                    //}

                    //// "Portrait"の子オブジェクトを探し、画像を設定
                    //Transform portraitTransform = itemGO.transform.Find("Portrait");
                    //if (portraitTransform != null)
                    //{
                    //    Image portraitImage = portraitTransform.GetComponent<Image>();
                    //    if (portraitImage != null && item.characterPortrait != null)
                    //    {
                    //        portraitImage.sprite = item.characterPortrait;
                    //        portraitImage.gameObject.SetActive(true);
                    //    }
                    //    else if (portraitImage != null)
                    //    {
                    //        portraitImage.gameObject.SetActive(false);
                    //    }
                    //}
                }
            }
        }
    }

    /// <summary>
    /// クレジットが上方向へスクロールする協程。
    /// スクロールが終了すると、メインタイトルへ状態遷移する。
    /// </summary>
    private IEnumerator ScrollCredits()
    {
        // 指定された遅延時間後にスクロール開始
        yield return new WaitForSeconds(startDelay);

        // creditsContainerを画面下部に配置し、画面外からスクロール開始
        creditsContainer.anchoredPosition = new Vector2(creditsContainer.anchoredPosition.x, -Screen.height);

        // 最後のクレジット項目が画面上部に到達するための目標位置を算出（コンテナの高さを目標とする）
        float targetY = creditsContainer.rect.height;

        // クレジットが目標位置に到達するまでスクロールを継続
        while (creditsContainer.anchoredPosition.y < targetY)
        {
            creditsContainer.anchoredPosition += new Vector2(0, scrollSpeed * Time.deltaTime);
            yield return null;
        }

        // スクロール終了のデバッグログ出力
        Debug.Log("Credits finished scrolling");

        // メインタイトルへ状態遷移
        GameStateManager.Instance.SwitchState(GameState.MainTitle);
        AudioManager.Instance.StopBGM();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 物件対話用の UI を管理するクラス
/// （立ち絵を使用しないシンプルな対話用 UI）
///</ summary >
public class EventDialogUI : MonoBehaviour, IDialogUI
{
    [Header("Dialog Box")]
    [SerializeField] private GameObject objectDialogBox;
    [SerializeField] private Text objectDialogText;

    [Header("Options Panel")]
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private List<Text> optionTexts;

    [Header("CG Image")]
    [SerializeField] private Image CG;
    [Header("Fade Settings")]
    [SerializeField] private float fadeStep = 0.05f;
    [SerializeField] private float fadeDelay = 0.05f;

    // 動的に生成した選択肢テキストのリスト
    private List<Text> optionTextUI = new List<Text>();

    /// <summary>
    /// CG のアニメーション状態（外部参照用）
    /// </summary>
    public bool IsCGAnimating { get; private set; } = false;

    /// <summary>
    /// UI を初期化し、対話ボックスを表示、テキストをクリアし、選択肢を非表示にする
    /// </summary>
    public void InitUI()
    {
        objectDialogBox.SetActive(true);
        objectDialogText.text = "";
        HideOptions();
    }

    /// <summary>
    /// スピーカー表示は物件対話では不要なため実装しない
    /// </summary>
    public void ShowSpeaker(SpeakerName speaker) { }

    /// <summary>
    /// スピーカー非表示は物件対話では不要なため実装しない
    /// </summary>
    public void HideSpeaker(SpeakerName speaker) { }

    /// <summary>
    /// 講話アニメーションは物件対話では不要なため実装しない
    /// </summary>
    public void SetSpeakerTalking(SpeakerName speaker, bool isTalking) { }

    /// <summary>
    /// スピーカー名の表示は必要に応じて実装
    /// </summary>
    public void SetSpeakerName(SpeakerName speaker) { }

    /// <summary>
    /// 対話テキストを追加表示する
    /// </summary>
    public void ShowLineText(string text)
    {
        objectDialogText.text += text;
    }

    /// <summary>
    /// CG をフェードインで表示する
    /// </summary>
    public void ShowCg(Sprite newSprite)
    {
        if (CG != null && newSprite != null)
        {
            StartCoroutine(FadeInCG(newSprite));
        }
    }

    /// <summary>
    /// CG をフェードアウトで非表示にする
    /// </summary>
    public void HideCg()
    {
        if (CG != null && CG.sprite != null)
        {
            StartCoroutine(FadeOutCG());
        }
    }

    /// <summary>
    /// CG のフェードイン処理（透明から不透明へ）
    /// </summary>
    public IEnumerator FadeInCG(Sprite newSprite)
    {
        if (newSprite == null)
            yield break;

        IsCGAnimating = true;
        CG.sprite = newSprite;

        float alpha = 0f;
        while (alpha < 1f)
        {
            alpha += fadeStep;
            Color color = CG.color;
            color.a = alpha;
            CG.color = color;
            yield return new WaitForSeconds(fadeDelay);
        }
        IsCGAnimating = false;
    }

    /// <summary>
    /// CG のフェードアウト処理（不透明から透明へ）
    /// </summary>
    public IEnumerator FadeOutCG()
    {
        IsCGAnimating = true;
        float alpha = 1f;
        while (alpha > 0f)
        {
            alpha -= fadeStep;
            Color color = CG.color;
            color.a = alpha;
            CG.color = color;
            yield return new WaitForSeconds(fadeDelay);
        }
        IsCGAnimating = false;
    }

    /// <summary>
    /// 対話テキストをクリアする
    /// </summary>
    public void ClearLineText()
    {
        objectDialogText.text = "";
    }

    /// <summary>
    /// 分岐選択肢を動的生成して表示する
    /// </summary>
    public void ShowOptions(List<string> options)
    {
        optionsPanel.SetActive(true);
        foreach (Transform child in optionsPanel.transform)
        {
            Destroy(child.gameObject);
        }
        optionTextUI.Clear();

        RectTransform panelRect = optionsPanel.GetComponent<RectTransform>();
        float panelWidth = panelRect.rect.width;
        float spacing = 10f;
        float optionHeight = 60f;
        float totalHeight = options.Count * (optionHeight + spacing);
        panelRect.sizeDelta = new Vector2(panelWidth, Mathf.Min(totalHeight, 400));

        VerticalLayoutGroup layoutGroup = optionsPanel.GetComponent<VerticalLayoutGroup>();
        if (layoutGroup == null)
        {
            layoutGroup = optionsPanel.AddComponent<VerticalLayoutGroup>();
        }
        layoutGroup.childAlignment = TextAnchor.MiddleCenter;
        layoutGroup.spacing = spacing;
        layoutGroup.padding = new RectOffset(10, 10, 10, 10);

        for (int i = 0; i < options.Count; i++)
        {
            GameObject newOption = new GameObject($"Option_{i}", typeof(RectTransform), typeof(Text));
            newOption.transform.SetParent(optionsPanel.transform, false);

            RectTransform rt = newOption.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(panelWidth - 20, optionHeight);

            Text optionText = newOption.GetComponent<Text>();
            optionText.font = objectDialogText.font;
            optionText.fontSize = 30;
            optionText.alignment = TextAnchor.MiddleCenter;
            optionText.color = Color.white;
            optionText.text = options[i];

            optionTextUI.Add(optionText);
        }

        int initialSelectedIndex = 0;
        UpdateOptionText(initialSelectedIndex);
    }

    /// <summary>
    /// 選択中の選択肢に "> " を付加して更新する
    /// </summary>
    public void UpdateOptionText(int selectedOptionIndex)
    {
        for (int i = 0; i < optionTextUI.Count; i++)
        {
            string orig = optionTextUI[i].text;
            if (orig.StartsWith("> "))
            {
                orig = orig.Substring(2);
            }
            optionTextUI[i].text = (i == selectedOptionIndex) ? "> " + orig : orig;
        }
    }

    /// <summary>
    /// 選択肢パネルを非表示にし、生成した選択肢を削除する
    /// </summary>
    public void HideOptions()
    {
        optionsPanel.SetActive(false);
        foreach (Transform child in optionsPanel.transform)
        {
            Destroy(child.gameObject);
        }
        optionTextUI.Clear();
    }

    /// <summary>
    /// 対話終了時の処理（対話ボックスを閉じ、選択肢パネルを非表示にする）
    /// </summary>
    public void OnDialogEnd()
    {
        objectDialogBox.SetActive(false);
        HideOptions();
    }
}

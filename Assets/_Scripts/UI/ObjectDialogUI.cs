using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 物件対話用の UI を管理するクラス
/// </summary>
public class ObjectDialogUI : MonoBehaviour, IDialogUI
{
    [SerializeField] private GameObject objectDialogBox;
    [SerializeField] private Text objectDialogText;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private List<Text> optionTexts;
    [SerializeField] private Image CG;

    // 動的に生成した選択肢テキストを保持するリスト
    private List<Text> optionTextUI = new List<Text>();

    /// <summary>
    /// UI を初期化する
    /// </summary>
    public void InitUI()
    {
        objectDialogBox.SetActive(true);
        objectDialogText.text = "";
        HideOptions();
    }

    /// <summary>
    /// スピーカーを表示する（物件対話では実装不要の場合がある）
    /// </summary>
    public void ShowSpeaker(SpeakerName speaker)
    {
        // 物件対話の場合、キャラクター表示が不要な場合があります
    }

    /// <summary>
    /// スピーカーを非表示にする（物件対話では実装不要の場合がある）
    /// </summary>
    public void HideSpeaker(SpeakerName speaker)
    {
        // 同上
    }

    /// <summary>
    /// スピーカーの講話状態を設定する（物件対話では実装不要の場合がある）
    /// </summary>
    public void SetSpeakerTalking(SpeakerName speaker, bool isTalking)
    {
        // 物件対話では講話アニメーションを使用しない場合があります
    }

    /// <summary>
    /// スピーカー名を設定する
    /// </summary>
    public void SetSpeakerName(SpeakerName speaker)
    {
        // 必要に応じて実装
    }

    /// <summary>
    /// 対話テキストを表示する
    /// </summary>
    public void ShowLineText(string text)
    {
        objectDialogText.text += text;
    }

    /// <summary>
    /// CG（背景画像）を表示する
    /// </summary>
    public void ShowCg(Sprite newSprite)
    {
        if (CG != null && newSprite != null)
        {
            StartCoroutine(FadeInCG(newSprite));
        }
    }

    /// <summary>
    /// CG を非表示にする
    /// </summary>
    public void HideCg()
    {
        if (CG != null && CG.sprite != null)
        {
            StartCoroutine(FadeOutCG());
        }
    }

    /// <summary>
    /// CG のフェードインアニメーション
    /// </summary>
    public IEnumerator FadeInCG(Sprite newSprite)
    {
        if (newSprite == null)
            yield break;

        CG.sprite = newSprite;

        float alpha = 0f;
        while (alpha < 1f)
        {
            alpha += 0.05f;
            Color color = CG.color;
            color.a = alpha;
            CG.color = color;
            yield return new WaitForSeconds(0.05f);
        }
    }

    /// <summary>
    /// CG のフェードアウトアニメーション
    /// </summary>
    public IEnumerator FadeOutCG()
    {
        float alpha = 1f;
        while (alpha > 0f)
        {
            alpha -= 0.05f;
            Color color = CG.color;
            color.a = alpha;
            CG.color = color;
            yield return new WaitForSeconds(0.05f);
        }
    }

    /// <summary>
    /// 対話テキストをクリアする
    /// </summary>
    public void ClearLineText()
    {
        objectDialogText.text = "";
    }

    /// <summary>
    /// 選択肢を表示する
    /// </summary>
    public void ShowOptions(List<string> options)
    {
        optionsPanel.SetActive(true);
        // 既存の選択肢を削除する
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

        // VerticalLayoutGroup を利用して選択肢を配置する
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
            optionText.font = objectDialogText.font; // 対話テキストと同じフォントを使用
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
    /// 選択肢テキストを更新する（選択中の項目前に "> " を追加する）
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
    /// 選択肢を非表示にする
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
    /// 対話終了時の処理
    /// </summary>
    public void OnDialogEnd()
    {
        objectDialogBox.SetActive(false);
        HideOptions();
    }
}

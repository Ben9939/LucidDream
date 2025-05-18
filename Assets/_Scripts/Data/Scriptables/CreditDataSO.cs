using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// クレジット項目を表すクラスです。
/// 名前や役職、キャラクター画像などの情報を管理します。
/// </summary>
[System.Serializable]
public class CreditItem
{
    [Tooltip("表示するテキスト（例：名前や役職）")]
    public string creditText;

    [Tooltip("キャラクターの画像（任意）")]
    public Sprite characterPortrait;
}

/// <summary>
/// クレジットのセクションを表すクラスです。
/// セクションタイトルと、このセクションに含まれる全ての項目のリストを管理します。
/// </summary>
[System.Serializable]
public class CreditSection
{
    [Tooltip("セクションのタイトル（例：ゲームプログラミング）")]
    public string sectionTitle;

    [Tooltip("このセクションに含まれる項目（名前と役職）")]
    public List<CreditItem> creditItems;
}

/// <summary>
/// クレジットデータを管理するScriptableObjectです。
/// 複数のセクションを保持し、クレジット表示に利用されます。
/// </summary>
[CreateAssetMenu(fileName = "CreditData", menuName = "Credits/CreditDataSO")]
public class CreditDataSO : ScriptableObject
{
    public List<CreditSection> sections;
}

//using System.Collections.Generic;
//using UnityEngine;

//[System.Serializable]
//public class CreditItem
//{
//    [Tooltip("顯示的文字（例如姓名或職位）")]
//    public string creditText;
//    [Tooltip("角色圖像（可選）")]
//    public Sprite characterPortrait;
//}

//[System.Serializable]
//public class CreditSection
//{
//    [Tooltip("段落標題，例如：『遊戲編程』")]
//    public string sectionTitle;
//    [Tooltip("該段落下的所有項目（姓名和角色圖）")]
//    public List<CreditItem> creditItems;
//}

//[CreateAssetMenu(fileName = "CreditData", menuName = "Credits/CreditDataSO")]
//public class CreditDataSO : ScriptableObject
//{
//    public List<CreditSection> sections;
//}

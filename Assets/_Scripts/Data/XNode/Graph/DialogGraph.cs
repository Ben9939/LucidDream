using System.Collections.Generic;
using UnityEngine;
using XNode;

public enum Character { Player, Npc }
public enum Event { None, Battle, GetItem, ReadQrCode, SwitchScene, ShowCG, IntroEnd, GetBag, Puzzle }
public enum dialogEffect { None }


[System.Serializable]
public class DialogData
{
    public List<NpcDialog> dialogs;

    [System.Serializable]
    public class NpcDialog
    {
        public List<Line> lines;
    }

    [System.Serializable]
    public class Line
    {
        public string line;
        public string character;
        public string dialogEvent;
        public string item;
        public string CG;
        public List<string> options;
        public List<int> nextDialogs;
        public bool scenarioTrigger;
        public string effect;
    }
}


/// <summary>
/// ダイアロググラフです。<br/>
/// 対話シーンの現在ノードと BGM 設定を保持します。
/// </summary>
[CreateAssetMenu(fileName = "DialogGraph", menuName = "Graph/DialogGraph")]
public class DialogGraph : NodeGraph
{
    /// <summary>
    /// 現在の対話ノード（進行中の対話のトラッキング用）
    /// </summary>
    public Dialog_BaseNode currentNode;
    /// <summary>
    /// 対話中に再生する BGM の種類
    /// </summary>
    public BGMSoundData.BGM BGM = BGMSoundData.BGM.None;

    public void ResetData()
    {
        currentNode = null;
    }
}

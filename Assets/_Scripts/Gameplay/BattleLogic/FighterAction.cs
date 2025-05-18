using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FighterAction : MonoBehaviour
{
    [SerializeField] GameObject target;
    [SerializeField] GameObject owner;

    public void SelectAction(string Action, int multiplier)
    {
        if (Action.CompareTo("Heal") == 0)
        {
            //SkillText.text = "HPかいふくりょく + " + multiplier;
            owner.GetComponent<ActionScript>().Healing(multiplier);
        }
        else if (Action.CompareTo("Attack") == 0)
        {
            //SkillText.text = "こうげきくりょく + " + multiplier;
            owner.GetComponent<ActionScript>().Attack(target, multiplier);
        }
        else if (Action.CompareTo("IncreaseMana") == 0)
        {
            //SkillText.text = "MPかいふくりょく + " + multiplier;
            owner.GetComponent<ActionScript>().IncreaseMana(multiplier);
        }
    }
}

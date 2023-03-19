using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleTip_DeleteThis_FasterIsBetter : MonoBehaviour
{
    void Start()
    {
        if (ScensVar.LevelId is 20 or 13 or 6)
        {
            Text text = GetComponentInChildren<Text>();
            switch (ScensVar.LevelId)
            {
                case 6:
                    text.text = "Boss ability: replaces a random number of cards in your hand with random ones!";
                    break;
                case 13:
                    text.text = "Boss ability: reduces your wall's max health to its current health!";
                    break;
                case 20:
                    text.text = "Boss ability: burns up to 1 income of a random resource!";
                    break;
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
}

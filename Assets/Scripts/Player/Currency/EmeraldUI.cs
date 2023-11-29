using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EmeraldUI : Singleton<EmeraldUI>
{
    public Text text;

    public void UpdatePlayerEmeraldUI()
    {
        text.text = PlayerFSM.instance.curEmeraldNumber.ToString();
    }
}

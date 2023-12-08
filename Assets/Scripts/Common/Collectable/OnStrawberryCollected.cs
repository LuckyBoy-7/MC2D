using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnStrawberryCollected : MonoBehaviour
{
    /// <summary>
    /// 在动画结束后调用
    /// </summary>
    public void OnCollected()
    {
        var player = PlayerFSM.instance;
        player.strawberryNumber += 1;
        StrawberryUI.instance.OnCollected();
        Destroy(gameObject);
    }
}

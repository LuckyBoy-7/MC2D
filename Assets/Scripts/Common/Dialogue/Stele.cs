using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 原理就是，第一行为Header，一开始用空的Text占位，然后真正的从第一段话开始向上出现，然后显示接下来的话
/// </summary>
public class Stele : MonoBehaviour
{
    public List<string> contents;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            SteleUI.instance.ShowContents(contents);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            SteleUI.instance.HideContents();
    }
}
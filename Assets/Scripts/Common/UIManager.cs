using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public KeyCode inventoryKey = KeyCode.E;
    public GameObject inventory;

    private void Update()
    {
        var manager = GameManager.instance;
        if (Input.GetKeyDown(inventoryKey))
        {
            if (inventory.activeSelf)
            {
                manager.state = GameStateType.Play;
                inventory.SetActive(false);
            }
            else
            {
                if (manager.state == GameStateType.PausePlayer || manager.state == GameStateType.PauseAll)
                    return;
                manager.state = GameStateType.PausePlayer;
                inventory.SetActive(true);
            }
        }
    }
}
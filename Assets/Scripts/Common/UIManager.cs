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
        if (manager.state == GameStateType.Pause || manager.state == GameStateType.Dialogue)
            return;
        if (Input.GetKeyDown(inventoryKey))
        {
            if (inventory.activeSelf)
            {
                manager.state = GameStateType.Play;
            }
            else
            {
                manager.state = GameStateType.Inventory;
            }

            inventory.SetActive(!inventory.activeSelf);
        }
    }
}
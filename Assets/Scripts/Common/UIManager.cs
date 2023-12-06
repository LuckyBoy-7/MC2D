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
        if (Input.GetKeyDown(inventoryKey))
        {
            inventory.SetActive(!inventory.activeSelf);
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Test : MonoBehaviour
{
    public GameObject test;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
            Instantiate(test, PlayerFSM.instance.transform.position, quaternion.identity);
    }
}

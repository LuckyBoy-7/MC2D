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

    private void Start()
    {
        StartCoroutine(Temp());
    }

    private IEnumerator Temp()
    {
        Debug.Log(1);
        yield return Temp2();
        Debug.Log(4);
    }
    
    private IEnumerator Temp2()
    {
        Debug.Log(2);
        yield return 1;
        Debug.Log(3);
    }
}

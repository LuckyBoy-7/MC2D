using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class EmeraldPile : MonoBehaviour
{
    public EmeraldEmitter emeraldEmitter;
    public int spawnEmeraldCount;
    public int spawnEmeraldNumberEachTime;

    public void Looted()
    {
        Instantiate(emeraldEmitter, transform.position, Quaternion.identity).num = spawnEmeraldNumberEachTime;
        if (--spawnEmeraldCount <= 0)
            Destroy(gameObject);
    }
}
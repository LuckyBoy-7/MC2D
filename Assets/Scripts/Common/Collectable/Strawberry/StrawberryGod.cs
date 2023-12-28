using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class StrawberryGod : MonoBehaviour
{
    public int receivedStrawberryNumber;
    public float rewardTimeGap;
    private float elapse;
    private bool isInArea;

    public EmeraldEmitter emeraldEmitter;
    public float emitForce;
    public int rewardEmeraldNumber;
    public int rewardEmeraldNumberNoise;

    private void Update()
    {
        elapse += Time.deltaTime;
        if (elapse >= rewardTimeGap)
        {
            elapse -= rewardTimeGap;
            if (receivedStrawberryNumber < PlayerFSM.instance.strawberryNumber)
            {
                receivedStrawberryNumber += 1;
                var emeraldNumber = rewardEmeraldNumber + Random.Range(0, rewardEmeraldNumberNoise + 1);
                var emitter= Instantiate(emeraldEmitter, transform.position, Quaternion.identity);
                emitter.num = emeraldNumber;
                emitter.speed = emitForce;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.GetComponent<PlayerTriggerBox>())
            return;
        isInArea = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.GetComponent<PlayerTriggerBox>())
            return;
        isInArea = false;
        elapse = 0;
    }
}
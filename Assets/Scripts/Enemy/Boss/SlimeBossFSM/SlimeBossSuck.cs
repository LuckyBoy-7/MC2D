using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeBossSuck : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        var slime = other.GetComponent<SlimeFSM>();
        var parent = transform.parent.GetComponent<SlimeBossFSM>();
        if (slime && slime.canBeSucked && parent.healthPoint != parent.maxHealthPoint)
        {
            parent.Heal(slime.healthPoint);
            Destroy(slime.gameObject);
        }
    }
}
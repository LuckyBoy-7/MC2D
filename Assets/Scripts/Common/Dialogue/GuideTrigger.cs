using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuideTrigger : MonoBehaviour
{
    public AbilityType abilityType;
    private bool hasBeenShown;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (hasBeenShown)
                return;
            hasBeenShown = true;
            GuideUI.instance.Show(abilityType);
        }
    }
}

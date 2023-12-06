using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Villager : MonoBehaviour
{
    public Dialogue_SO test;

    void Start()
    {
        DialogueManager.instance.ShowDialogue(test);
    }
}
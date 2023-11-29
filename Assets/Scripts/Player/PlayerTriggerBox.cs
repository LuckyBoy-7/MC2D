using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTriggerBox : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerFSM.instance.UpdateTriggerEnter2D(other);
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerFSM.instance.UpdateTriggerExit2D(other);
    }
    
    private void OnTriggerStay2D(Collider2D other)
    {
        PlayerFSM.instance.UpdateTriggerStay2D(other);
    }
}
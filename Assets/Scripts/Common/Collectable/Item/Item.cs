using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Item : MonoBehaviour
{
    public AbilityType abilityType;
    public Transform hintPoint;
    public string hintContent = "拾取";

    public float defaultPushedForce;
    private Rigidbody2D rigidbody;

    private bool isInArea;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
    }

    public void Pushed(Vector2 dir)
    {
        rigidbody.AddForce(dir * defaultPushedForce, ForceMode2D.Impulse);
    }

    private void Update()
    {
        if (Input.GetKeyDown(PlayerFSM.instance.upKey))
        {
            HintUI.instance.FadeOut();
            PlayerFSM.instance.UnlockAbility(abilityType);
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.GetComponent<PlayerTriggerBox>())
            return;
        isInArea = true;
        HintUI.instance.SetFixedPoint(hintPoint);
        HintUI.instance.ChangeText(hintContent);
        HintUI.instance.FadeIn();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.GetComponent<PlayerTriggerBox>())
            return;
        isInArea = false;
        HintUI.instance.FadeOut();
    }
}
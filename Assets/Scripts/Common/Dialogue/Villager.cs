using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class Villager : Singleton<Villager>
{
    public string hintContent = "交谈";
    public Transform hintTransform;
    public bool isPlayerInArea;

    public Dialogue_SO test;

    public bool isFirstSee = true;
    public Dialogue_SO firstSeeDialogue;  // 初次见面
    public Dialogue_SO[] emptyDialogue; // 当无话可说时的一些闲聊
    public Dialogue_SO dashDialogue; // 当刚获取冲刺时
    public Dialogue_SO releaseArrowDialogue; // 当刚获取复仇之箭时
    public Dialogue_SO dropDialogue; // 当刚获取下砸
    public Dialogue_SO roarDialogue; // 当刚获取上吼
    public Dialogue_SO superDashDialogue; // 当刚获取超冲
    public Dialogue_SO doubleJumpDialogue; // 当刚获取二段跳
    public Dialogue_SO wallSlideDialogue; // 当刚获取蹬墙跳
    private Dictionary<AbilityType, Dialogue_SO> dialogues;
    private Dictionary<AbilityType, bool> dialogueUnlockedStateDic = new();

    public void UnlockDialogue(AbilityType stateType)
    {
        dialogueUnlockedStateDic[stateType] = true;
    }


    void Start()
    {
        dialogues = new Dictionary<AbilityType, Dialogue_SO>()
        {
            { AbilityType.Dash, dashDialogue },
            { AbilityType.ReleaseArrow, releaseArrowDialogue },
            { AbilityType.Drop, dropDialogue },
            { AbilityType.Roar, roarDialogue },
            { AbilityType.SuperDash, superDashDialogue },
            { AbilityType.DoubleJump, doubleJumpDialogue },
            { AbilityType.WallSlide, wallSlideDialogue },
        };
        // DialogueManager.instance.ShowDialogue(test);
    }

    private void Update()
    {
        if (isPlayerInArea)
        {
            if (Input.GetKeyDown(PlayerFSM.instance.upKey) && GameManager.instance.state == GameStateType.Play &&
                PlayerFSM.instance.isOnGround)
            {
                Chat();
            }
        }
    }

    private void Chat()
    {
        if (isFirstSee)
        {
            isFirstSee = false;
            DialogueUI.instance.ShowDialogues(firstSeeDialogue);
            return;
        }
            
        // DialogueUI.instance.ShowDialogue(test);
        bool found = false;
        foreach (var (stateType, unlockedState) in dialogueUnlockedStateDic)
        {
            if (unlockedState == false)
                continue;
            found = true;
            DialogueUI.instance.ShowDialogues(dialogues[stateType]);
            break;
        }

        if (!found)
        {
            DialogueUI.instance.ShowDialogues(emptyDialogue[Random.Range(0, emptyDialogue.Length)]);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;
        HintUI.instance.ChangeText(hintContent);
        HintUI.instance.FadeIn();
        HintUI.instance.SetFixedPoint(hintTransform);
        isPlayerInArea = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;
        isPlayerInArea = false;
        HintUI.instance.FadeOut();
    }
}
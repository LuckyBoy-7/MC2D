using System.Collections.Generic;
using UnityEngine;

public class Smithy : Singleton<Smithy>
{
    public string hintContent = "交谈";
    public Transform hintTransform;
    public bool isPlayerInArea;

    public bool isFirstSee = true;
    public Dialogue_SO firstSeeDialogue; // 初次见面

    public List<int> emeraldCosts;  // 4把等级的剑对应升级3次
    private int idx;
    public Dialogue_SO noMoneyDialogue; // 钱不够 
    public Dialogue_SO yesDialogue; // 买
    public Dialogue_SO noDialogue; // 不买
    public Dialogue_SO finishedDialogue; // 不买
    public Dialogue_SO afterUpgradeDialogue; // 更新完装备后发出的赞叹

    public int curCost => emeraldCosts[idx];

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
        if (isFirstSee)  // 初见
        {
            isFirstSee = false;
            DialogueUI.instance.onDialogueOver += ChoiceUI.instance.Show;
            DialogueUI.instance.ShowDialogues(firstSeeDialogue);
        }
        else if (idx == emeraldCosts.Count)  // 毕业
        {
            DialogueUI.instance.ShowDialogues(finishedDialogue);
        }
        else  // 贩卖
        {
            ChoiceUI.instance.Show();
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

    public void OnChooseYes()
    {
        // Debug.Log("Yes");
        if (PlayerFSM.instance.curEmeraldNumber >= emeraldCosts[idx]) // 钱够
        {
            PlayerAttack.instance.UpgradeSword();

            DialogueUI.instance.ShowDialogues(yesDialogue, afterUpgradeDialogue);
            PlayerFSM.instance.curEmeraldNumber -= emeraldCosts[idx++];
            EmeraldUI.instance.UpdatePlayerEmeraldUI();
        }
        else
        {
            DialogueUI.instance.ShowDialogues(noMoneyDialogue);
        }
    }

    public void OnChooseNo()
    {
        // Debug.Log("No");
        DialogueUI.instance.ShowDialogues(noDialogue);
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueManager : Singleton<DialogueManager>
{
    public DialogueUI dialogueUI;  // 如果单例直接写UI上的话，一开始如果为关闭状态则拿不到instance

    public void ShowDialogue(Dialogue_SO dialogue_SO)
    {
        dialogueUI.ShowDialogue(dialogue_SO);
    }
}
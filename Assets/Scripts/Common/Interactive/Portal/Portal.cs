using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Portal : MonoBehaviour
{
    private static Dictionary<string, Vector3> portalToPos = new();

    public string name;
    public Transform hintTransform;
    public string unLockedHintContent = "进入";
    public string lockedHintContent = "点燃";
    public static string currentPortal;

    public float unlockDuration;
    public bool hasUnlocked; // 是否被点燃
    private bool canUse; // 被点燃后还要等待地狱门传送片生成才能使用

    public SpriteRenderer[] portalClips; // 做淡出的
    private bool isPlayerInArea;

    private void Update()
    {
        PlayerFSM player = PlayerFSM.instance;
        if (isPlayerInArea && player.isOnGround && Input.GetKeyDown(player.upKey))
        {
            if (hasUnlocked)
            {
                if (canUse && GameManager.instance.state == GameStateType.Play)
                    PortalUI.instance.Show(); // 打开传送地图
            }
            else // 点燃地狱门（激活）
            {
                hasUnlocked = true;
                RegisterCurrentPortal();
                foreach (var spriteRenderer in portalClips)
                    spriteRenderer.DOFade(1, unlockDuration).onComplete += () => canUse = true;
                HintUI.instance.ChangeText(unLockedHintContent, unlockDuration);
            }
        }
    }

    private void RegisterCurrentPortal()
    {
        portalToPos[name] = transform.position;
        PortalUI.instance.RegisterUnlockedPortal(name);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;
        HintUI.instance.ChangeText(hasUnlocked ? unLockedHintContent : lockedHintContent);
        HintUI.instance.FadeIn();
        HintUI.instance.SetFixedPos(hintTransform.position);
        isPlayerInArea = true;
        currentPortal = name;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;
        isPlayerInArea = false;
        HintUI.instance.FadeOut();
    }

    public static void TeleportTo(string name)
    {
        PlayerFSM.instance.transform.position = portalToPos[name];
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class Portal : MonoBehaviour
{
    private static Dictionary<string, Vector3> portalToPos = new();
    public string name;
    public Transform hintTransform;
    public CanvasGroup hintUI;
    public Text hintText;
    public string unLockedHintContent = "进入";
    public string lockedHintContent = "点燃";
    public float UIFadeDuration;
    public static string currentPortal;

    public bool hasUnlocked;
    public SpriteRenderer[] portalClips;
    public float unlockDuration;
    private bool isShowingHintUI;
    private bool canUse;
    private bool isPlayerInArea;

    private void Start()
    {
        hintUI = PortalUI.instance.hintUI;
        hintText = PortalUI.instance.hintText;
    }


    private void Update()
    {
        if (isShowingHintUI)
        {
            Debug.Log(hintTransform.position);
            hintUI.transform.position = Camera.main.WorldToScreenPoint(hintTransform.position);
        }

        if (isPlayerInArea)
        {
            PlayerFSM player = PlayerFSM.instance;
            if (player.isOnGround && Input.GetKeyDown(player.upKey))
            {
                if (hasUnlocked)
                {
                    if (canUse && GameManager.instance.state == GameStateType.Play)
                    {
                        GameManager.instance.state = GameStateType.PausePlayer;
                        PortalUI.instance.gameObject.SetActive(true);
                    }
                }
                else
                {
                    hasUnlocked = true;
                    RegisterPortal();
                    foreach (var spriteRenderer in portalClips)
                        spriteRenderer.DOFade(1, unlockDuration).onComplete += () => canUse = true;
                    hintText.DOFade(0, unlockDuration / 2).onComplete += () =>
                    {
                        hintText.text = unLockedHintContent;
                        hintText.DOFade(1, unlockDuration / 2);
                    };
                }
            }
        }
    }

    private void RegisterPortal()
    {
        portalToPos[name] = transform.position;
        PortalUI.instance.RegisterUnlockedPortal(name);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (hintUI.alpha != 0) // 除非原来正在淡出然后player又进来了
                hintUI.alpha = 0;
            hintText.text = hasUnlocked ? unLockedHintContent : lockedHintContent;
            DOTween.Kill("HintUIFade");
            hintUI.GetComponent<CanvasGroup>().DOFade(1, UIFadeDuration).SetId("HintUIFade");
            isShowingHintUI = true;
            isPlayerInArea = true;
            currentPortal = name;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInArea = false;
            DOTween.Kill("HintUIFade");
            hintUI.DOFade(0, UIFadeDuration).SetId("HintUIFade").onKill += () => isShowingHintUI = false; 
        }
    }

    public static void TeleportTo(string name)
    {
        PlayerFSM.instance.transform.position = portalToPos[name];
    }
}
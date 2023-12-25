using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bed : MonoBehaviour
{
    private static Dictionary<string, Vector3> portalToPos = new();

    public Transform hintTransform;
    public string hintContent = "休息";

    private bool isPlayerInArea;

    private void Update()
    {
        PlayerFSM player = PlayerFSM.instance;
        if (isPlayerInArea && player.isOnGround && Input.GetKeyDown(player.upKey))
        {
            player.Recover();
            player.SetSpawnPoint(transform);
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
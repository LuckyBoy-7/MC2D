using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;

public class Guide : MonoBehaviour
{
    public string firstVerb;
    public string secondVerb;
    public InputType inputType;
    private bool hasBeenShown;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (hasBeenShown)
                return;
            hasBeenShown = true;
            var player = PlayerFSM.instance;
            if (inputType == InputType.Move)
                GuideUI.instance.ShowContents(firstVerb, secondVerb);
            else if (inputType == InputType.Attack)
                GuideUI.instance.ShowContents(firstVerb, secondVerb, player.attackKey);
            else if (inputType == InputType.Jump)
                GuideUI.instance.ShowContents(firstVerb, secondVerb, player.jumpKey);
            else if (inputType == InputType.Dash)
                GuideUI.instance.ShowContents(firstVerb, secondVerb, player.dashKey);
            else if (inputType == InputType.Heal)
                GuideUI.instance.ShowContents(firstVerb, secondVerb, player.spellKey);
            else if (inputType == InputType.Arrow)
                GuideUI.instance.ShowContents(firstVerb, secondVerb, player.spellKey);
            else if (inputType == InputType.Drop)
                GuideUI.instance.ShowContents(firstVerb, secondVerb, player.downKey, player.spellKey);
            else if (inputType == InputType.Roar)
                GuideUI.instance.ShowContents(firstVerb, secondVerb, player.upKey, player.spellKey);
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Serialization;

public class GuideManager : Singleton<GuideManager>
{
    public void Show(AbilityType abilityType)
    {
        var player = PlayerFSM.instance;
        if (abilityType == AbilityType.Move)
            GuideUI.instance.ShowContents("按住","移动");
        else if (abilityType == AbilityType.Attack)
            GuideUI.instance.ShowContents("点击", "攻击", player.attackKey);
        else if (abilityType == AbilityType.Jump)
            GuideUI.instance.ShowContents("按住", "跳跃", player.jumpKey);
        else if (abilityType == AbilityType.Dash)
            GuideUI.instance.ShowContents("点击", "冲刺", player.dashKey);
        else if (abilityType == AbilityType.Heal)
            GuideUI.instance.ShowContents("按住", "治愈", player.spellKey);
        else if (abilityType == AbilityType.ReleaseArrow)
            GuideUI.instance.ShowContents("点击", "放箭", player.spellKey);
        else if (abilityType == AbilityType.Drop)
            GuideUI.instance.ShowContents("按住", "下砸", player.downKey, player.spellKey);
        else if (abilityType == AbilityType.Roar)
            GuideUI.instance.ShowContents("按住", "上吼", player.upKey, player.spellKey);
        else if (abilityType == AbilityType.SuperDash)
            GuideUI.instance.ShowContents("按住", "超冲", player.superDashKey);
        else if (abilityType == AbilityType.DoubleJump)
            GuideUI.instance.ShowContents("按住", "二段跳", player.jumpKey);
        else if (abilityType == AbilityType.WallSlide)
            GuideUI.instance.ShowContents("蹬墙跳Get");
        
    }
}
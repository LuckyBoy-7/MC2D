using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Enums
{
}


public enum AbilityType
{
    Move,
    Jump,
    Dash,
    Attack,
    Heal,
    ReleaseArrow,
    Drop,
    Roar,
    SuperDash,
    DoubleJump,
    WallSlide,
    Bag
}


public enum SwordType
{
    Wooden,
    Stone,
    Iron,
    Diamond,
}

public enum GameStateType
{
    // play是正常游玩的情况，pause是暂停的情况（啥都是禁止的，有些画面会慢放），dialogue是对话的情况（有动画，但是不能动），Inventory（有动画，不能动）
    Play,
    PauseAll,
    PausePlayer
}

public enum EnemyEggType
{
    Zombie,
    Hoglin,
    Spider,
    Slime,
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Enums
{
}


public enum InputType
{
    Move,
    Jump,
    Dash,
    Attack,
    Heal,
    Arrow,
    Drop,
    Roar,
    SuperDash
}

public enum InventoryType
{
    Dash,
    Sword,
    Heal,
    Arrow,
    Drop,
    Roar,
    WallSlide,
    DoubleJump,
    SuperDash
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
    Pause,
    Dialogue,
    Inventory
}


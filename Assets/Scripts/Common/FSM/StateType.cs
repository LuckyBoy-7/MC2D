using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StateType
{
    // Common
    Idle,
    Run,
    Attack,
    Hurt,

    // Player
    Jump,
    WallJump,
    WallSlide,
    Dash,
    ReleaseArrow,
    DoubleJump,
    Fall,
    // Enemy
    Chase,
    Teleport
}
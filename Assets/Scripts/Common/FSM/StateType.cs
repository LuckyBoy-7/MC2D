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
    Spell,
    Drop,
    Roar,
    Recover,
    SuperDash,
    // Enemy
    Chase,
    Teleport,
    Alert,
    Question,
    Patrol,
    Move,
    Wait,
    Guard,
    // Boss
    Wander,
    Crush,
    Vomit,
    Throw,
    ThrowOne,
    WideExplosion,
    // Him
    GroundCrush,
    AirCrush,
    Lash,
    StepBack,
    GroundSpike,
    ShootSpike,
    BurstSpike,
    SingExplode
}
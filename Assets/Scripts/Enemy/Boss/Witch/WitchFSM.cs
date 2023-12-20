using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WitchFSM : EnemyFSM
{
    public List<Transform> wanderPoses;
    public float wanderSpeed;
    [Header("Reset")] private Vector3 origPos;

    protected override void Start()
    {
        origPos = transform.position;
        base.Start();
        states[StateType.Wander] = new WitchWander(this); // idle
        TransitionState(StateType.Wander);
    }

    public void Reset()
    {
        transform.position = origPos;
        Heal(9999);
    }
}

public class WitchWander : IState
{
    private WitchFSM m;
    private Vector3 targetPos;
    private Vector3 prePos;

    public WitchWander(WitchFSM m)
    {
        this.m = m;
    }

    public void OnEnter()
    {
        RollTargetPos();
    }

    public void OnUpdate()
    {
        if (hasReachedTargetPos)
        {
            RollTargetPos();
        }
    }

    public void OnFixedUpdate()
    {
        m.transform.position =
            Vector3.MoveTowards(m.transform.position, targetPos, m.wanderSpeed * Time.fixedDeltaTime);
    }

    public void OnExit()
    {
    }

    private void RollTargetPos()
    {
        prePos = m.transform.position;
        targetPos = m.wanderPoses[Random.Range(0, m.wanderPoses.Count)].position;
        UpdateFacingDirection();
    }

    private bool hasReachedTargetPos => (targetPos - m.transform.position).magnitude < 1e-3;

    private void UpdateFacingDirection()
    {
        var sign = Mathf.Sign(targetPos.x - prePos.x);
        var scale = m.transform.localScale;
        m.transform.localScale = new Vector3(Mathf.Abs(scale.x) * sign, scale.y, scale.z);
    }
}
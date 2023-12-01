using DG.Tweening;
using UnityEditor;
using UnityEngine;

public class SilvefishFSM : GroundEnemyFSM
{
    [Header("Move")] // 最无脑的生物
    public float moveSpeed;

    public int moveDir;

    public float rotateTime;
    public float turnStateCheckDelay;
    // public List<Vector2> dirLst = new (){ Vector2.right ,Vector2.down,Vector2.left,Vector2.up};

    protected override void Start()
    {
        base.Start();
        healthPoint = maxHealthPoint;
        states[StateType.Move] = new SilverfishMove(this);
        TransitionState(StateType.Move);
    }

    private void Update()
    {
        currentState.OnUpdate();
    }

    private void FixedUpdate()
    {
        currentState.OnFixedUpdate();
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        // Cliff Raycast check
        var position = transform.position;
        var scale = transform.localScale;
        var box = hitBoxCollider;
        var (width, height) = (box.bounds.max.x - box.bounds.min.x, box.bounds.max.y - box.bounds.min.y);
        var (w, h) = (width / 2, height / 2);
        var transformDown = Quaternion.Euler(0, 0, -90) * transform.right;

        // cliff
        Gizmos.DrawLine(position, position + transformDown * cliffCheckDownRaycastDist);

        // Hit Box
        Handles.color = Color.red;
        Transform center = transform;
        Matrix4x4 oldMat = Handles.matrix;
        //获取目标旋转矩阵
        Matrix4x4 rotationMat = center.localToWorldMatrix;
        //设置当前为旋转矩阵
        Handles.matrix = rotationMat;
        //这里的center是相对目标中心而言，因为旋转cube与目标位置相同所以是zero
        Handles.DrawWireCube(center: hitBoxCollider.offset, size: hitBoxCollider.size);
        //重置当前矩阵
        Handles.matrix = oldMat;
    }

    public bool isOverMiddleCliff
    {
        get
        {
            var box = hitBoxCollider;
            var position = transform.position;
            var (width, height) = (box.bounds.max.x - box.bounds.min.x, box.bounds.max.y - box.bounds.min.y);
            var (w, h) = (width / 2, height / 2);

            var transformDown = Quaternion.Euler(0, 0, -90) * transform.right;
            bool overMiddleCliff = !Physics2D.Linecast(position,
                position + transformDown * cliffCheckDownRaycastDist,
                groundLayer);
            return overMiddleCliff;
        }
    }
}

public class SilverfishMove : IState
{
    private SilvefishFSM m;
    private float elapse;
    private bool isRest;
    private bool isTurning;

    public SilverfishMove(SilvefishFSM m)
    {
        this.m = m;
    }

    public void OnEnter()
    {
        Move();
    }

    public void OnUpdate()
    {
        if (isTurning)
        {
            elapse += Time.deltaTime;
            if (elapse > m.turnStateCheckDelay + m.rotateTime)
            {
                isTurning = false;
                elapse = 0;
            }
        }

        if (m.isOverMiddleCliff && !isTurning)
        {
            Stop();
            isTurning = true;
            Rotate();
        }
    }

    public void OnFixedUpdate()
    {
    }

    public void OnExit()
    {
    }

    private void Stop() => m.rigidbody.velocity = Vector2.zero;

    private void Rotate() =>
        m.transform.DORotate(m.transform.eulerAngles + new Vector3(0, 0, -m.moveDir * 90), m.rotateTime).onComplete +=
            Move;

    private void Move() => m.rigidbody.velocity = m.transform.right * (m.moveDir * m.moveSpeed);
}
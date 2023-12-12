using System;
using UnityEngine;

public class LaserButton : MonoBehaviour
{
    public float arc;
    public LineRenderer lineRenderer;
    private LineRenderer curLineRenderer;
    public Transform pivot;
    private bool isShooting;
    private bool hasCausedDamage;
    public float gizmosRadius;

    private void Start()
    {
        arc %= 360;
    }

    public void Shoot()
    {
        isShooting = true;
        var position = pivot.position;
        var hit = Physics2D.Raycast(position,
            Quaternion.Euler(0, 0, arc) * Vector2.right,
            Single.PositiveInfinity,
            1 << LayerMask.NameToLayer("Platform"));

        curLineRenderer = Instantiate(lineRenderer);
        curLineRenderer.SetPosition(0, position);
        curLineRenderer.SetPosition(1, hit.point);
    }

    public void LaserFade()
    {
        isShooting = false;
        Destroy(curLineRenderer.gameObject);
    }

    private void Update()
    {
        if (isShooting)
        {
            var position = pivot.position;
            var hit = Physics2D.Raycast(position,
                Quaternion.Euler(0, 0, arc) * Vector2.right,
                Single.PositiveInfinity,
                1 << LayerMask.NameToLayer("Player"));
            if (hit.collider != null) // 射到player了
            {
                if (!hasCausedDamage)
                {
                    hasCausedDamage = true;
                    PlayerFSM.instance.TryTakeDamage(1, position);
                }
            }
        }
        else
        {
            hasCausedDamage = false;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        var position = pivot.position;
        var dir = Quaternion.Euler(0, 0, arc) * Vector2.right;
        Gizmos.DrawLine(position, position + dir * gizmosRadius);
    }
}
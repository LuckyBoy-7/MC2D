using UnityEngine;

public class MovingSpike : MonoBehaviour
{
    public float delayTime;
    private float delayElapse;
    public Transform spike;
    public float movingSpeed;
    public bool enableResting;
    public float restTime;
    private float elapse;
    public Transform[] routes;
    private int idx;
    public Color routeMarkColor = Color.red;
    public float routeMarkRadius = 0.3f;

    private void OnDrawGizmos()
    {
        if (routes == null)
            return;
        Gizmos.color = routeMarkColor;
        foreach (var route in routes)
        {
            Gizmos.DrawWireSphere(route.position, routeMarkRadius);
        }
    }

    private void Update()
    {
        delayElapse += Time.deltaTime;
        if (delayElapse < delayTime)
            return;
        
        if ((spike.transform.position - routes[idx].position).magnitude < 1e-3) // 到了
        {
            if (enableResting)
            {
                elapse += Time.deltaTime;
                if (elapse > restTime)
                {
                    elapse = 0;
                    idx = (idx + 1) % routes.Length;
                }
            }
            else
                idx = (idx + 1) % routes.Length;
        }
        else
        {
            spike.position = Vector3.MoveTowards(spike.position, routes[idx].position, movingSpeed * Time.deltaTime);
        }
    }

}
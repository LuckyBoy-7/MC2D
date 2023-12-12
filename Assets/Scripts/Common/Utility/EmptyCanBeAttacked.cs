using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmptyCanBeAttacked : MonoBehaviour, ICanBeAttacked
{
    public bool canBeDestroyed;
    public void Attacked(Collider2D other = default)
    {
        if (canBeDestroyed)
            Destroy(gameObject);
    }
}
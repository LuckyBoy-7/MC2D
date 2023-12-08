using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDestroyAfterAnimation : MonoBehaviour
{
    public void DestroyGameObject()
    {
        Destroy(gameObject);
    }
}

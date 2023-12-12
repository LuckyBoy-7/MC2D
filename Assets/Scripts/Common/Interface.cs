using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interface
{
}


public interface ICanBeAttacked
{
    public void Attacked(Collider2D other = default);
}
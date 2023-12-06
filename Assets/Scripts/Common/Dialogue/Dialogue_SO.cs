using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue_SO")]
public class Dialogue_SO : ScriptableObject
{
    [TextArea] public List<string> contents;
}
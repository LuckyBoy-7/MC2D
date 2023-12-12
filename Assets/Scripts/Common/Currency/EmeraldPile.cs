using UnityEngine;

public class EmeraldPile : MonoBehaviour, ICanBeAttacked
{
    public EmeraldEmitter emeraldEmitter;
    public int spawnEmeraldCount;
    public int spawnEmeraldNumberEachTime;

    public void Attacked(Collider2D other = default)
    {
        Instantiate(emeraldEmitter, transform.position, Quaternion.identity).num = spawnEmeraldNumberEachTime;
        if (--spawnEmeraldCount <= 0)
            Destroy(gameObject);
    }
}
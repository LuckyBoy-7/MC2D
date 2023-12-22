using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct EnemyEggInfo
{
    public EnemyEggType type;
    public EnemyFSM enemy;
    public Sprite sprite;
}


public class EnemyEgg : MonoBehaviour
{
    public List<EnemyEggInfo> enemyEggInfos;
    private EnemyEggType type;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rigidbody;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rigidbody = GetComponent<Rigidbody2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Platform"))
        {
            Instantiate(enemyEggInfos.Find(info => info.type == type).enemy, transform.position, Quaternion.identity,
                transform.parent).canBeLooted = false;  // 怪物蛋里的怪没有钱
            Destroy(gameObject);
        }
    }

    public void Init(EnemyEggType type)
    {
        this.type = type;
        spriteRenderer.sprite = enemyEggInfos.Find(info => info.type == type).sprite;
    }

    public void Pushed(Vector2 force)
    {
        rigidbody.AddForce(force, ForceMode2D.Impulse);
    }
}
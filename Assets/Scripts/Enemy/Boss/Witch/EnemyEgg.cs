using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SpritePrefabPair
{
    public Sprite sprite;
    public GameObject enemy;
}


public class EnemyEgg : MonoBehaviour
{
    public List<SpritePrefabPair> spriteToEnemyPrefabs;
    private Sprite sprite;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Platform"))
        {
            Instantiate(spriteToEnemyPrefabs.Find(pair => pair.sprite == sprite).enemy, transform.position, Quaternion.identity, transform.parent);
            Destroy(gameObject);
        }
    }

    public void Init(Sprite sprite)
    {
        this.sprite = sprite;
        spriteRenderer.sprite = sprite;
    }
}
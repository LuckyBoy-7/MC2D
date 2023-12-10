using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using Random = UnityEngine.Random;

public class LifeFruitPlant : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    public Sprite[] lifeFruitPlants;
    public GameObject lifeFruitPrefab;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = lifeFruitPlants[Random.Range(0, lifeFruitPlants.Length)]; // 给生命果植物随机一张图片
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerAttack"))
        {
            Instantiate(lifeFruitPrefab, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
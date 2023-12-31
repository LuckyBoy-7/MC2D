using UnityEngine;

public class DripStone : MonoBehaviour
{
    private Rigidbody2D rigidbody;
    private BoxCollider2D box;
    private bool isOnGround;
    public AudioClip dripSfxSound;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        box = GetComponent<BoxCollider2D>();
    }

    public void Drip()
    {
        rigidbody.bodyType = RigidbodyType2D.Dynamic;
        AudioManager.instance.Play(dripSfxSound);
    }

    private void Update()
    {
        if (box.IsTouchingLayers(1 << LayerMask.NameToLayer("Platform")))
        {
            rigidbody.velocity = Vector2.zero;
            rigidbody.bodyType = RigidbodyType2D.Kinematic;
            isOnGround = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isOnGround)
        {
            PlayerFSM.instance.TryTakeDamage(1, transform.position);
        }
    }
}

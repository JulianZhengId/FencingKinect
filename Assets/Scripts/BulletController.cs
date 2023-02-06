using UnityEngine;

public class BulletController : MonoBehaviour
{
    public float speed = 5f;
    private Rigidbody2D rb;
    public Vector3 bulletDirection;
    public float timer;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= 5f)
        {
            Destroy(gameObject);
        }
        rb.velocity = (bulletDirection * speed);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.transform.name == "Shield")
        {
            AudioManager.playShieldHit();
            Destroy(this.gameObject);
            return;
        }

        if (other.transform.root.name == "P1" || other.transform.root.name == "P2")
        {
            other.transform.root.GetComponent<HealthManager>().TakeDamage();
            Destroy(this.gameObject);
        }
    }
}

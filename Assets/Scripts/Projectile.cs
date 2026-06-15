using UnityEngine;

public class Projectile : MonoBehaviour
{
    public Enemy target;
    public float damage;
    public bool isSplash;
    public float splashRadius = 1.2f;
    public float speed = 9f;

    void Update()
    {
        if (target == null || target.gameObject == null)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = Vector3.MoveTowards(
            transform.position, target.transform.position, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target.transform.position) < 0.12f)
            Impact();
    }

    void Impact()
    {
        if (isSplash)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, splashRadius);
            foreach (var h in hits)
            {
                var e = h.GetComponent<Enemy>();
                if (e != null) e.TakeDamage(damage);
            }
        }
        else
        {
            if (target != null && target.gameObject != null)
                target.TakeDamage(damage);
        }
        Destroy(gameObject);
    }
}

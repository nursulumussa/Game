using UnityEngine;

// CircleCast sweeps the entire step, avoiding tunnelling at low phone frame rates.
public class EnemyProjectile2D : MonoBehaviour
{
    public float radius = 0.10f;
    Vector2 direction;
    float speed, expires, screenMargin;
    int damage;
    LayerMask mask;
    Camera gameCamera;
    bool launched;
    readonly RaycastHit2D[] hits = new RaycastHit2D[8];
    public void Launch(Vector2 heading, float velocity, float lifetime, int amount,
        LayerMask collisionMask, Camera camera, float margin)
    {
        direction = heading.normalized; speed = velocity; expires = Time.time + lifetime;
        damage = amount; mask = collisionMask; gameCamera = camera; screenMargin = margin; launched = true;
    }
    void Update()
    {
        if (!launched) return;
        if (Time.time >= expires || gameCamera == null) { Destroy(gameObject); return; }
        Vector3 v = gameCamera.WorldToViewportPoint(transform.position);
        if (v.z <= 0f || v.x <= screenMargin || v.x >= 1f - screenMargin || v.y <= screenMargin || v.y >= 1f - screenMargin)
        { Destroy(gameObject); return; }
        float step = speed * Time.deltaTime;
        ContactFilter2D filter = new ContactFilter2D(); filter.SetLayerMask(mask); filter.useTriggers = true;
        int count = Physics2D.CircleCast(transform.position, radius, direction, filter, hits, step);
        if (count > 0)
        {
            PlayerHealth target = hits[0].collider.GetComponentInParent<PlayerHealth>();
            if (target != null)
            {
                Vector3 playerView = gameCamera.WorldToViewportPoint(target.transform.position);
                if (playerView.z > 0f && playerView.x > screenMargin && playerView.x < 1f - screenMargin
                    && playerView.y > screenMargin && playerView.y < 1f - screenMargin)
                    target.TakeDamage(damage);
            }
            Destroy(gameObject); return;
        }
        transform.position += (Vector3)(direction * step);
    }
}

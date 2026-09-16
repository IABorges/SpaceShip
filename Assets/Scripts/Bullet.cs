using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 14f;

    float rightBound;

    void Start()
    {
        Camera cam = Camera.main;
        rightBound = cam.orthographicSize * cam.aspect + 1f;
    }

    void Update()
    {
        transform.position += Vector3.right * speed * GameManager.PlayerDeltaTime;
        if (transform.position.x > rightBound) Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out EnemyController enemy))
        {
            enemy.Kill();
            Destroy(gameObject);
        }
    }
}

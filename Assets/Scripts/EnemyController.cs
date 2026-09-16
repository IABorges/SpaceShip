using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public float speed = 3f;

    float leftBound;

    void Start()
    {
        Camera cam = Camera.main;
        leftBound = -cam.orthographicSize * cam.aspect - 1f;
    }

    // Time.deltaTime respeita o tempo lento, entao o inimigo desacelera junto.
    void Update()
    {
        transform.position += Vector3.left * speed * Time.deltaTime;
        if (transform.position.x < leftBound) Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController>() != null)
        {
            GameManager.Instance?.OnPlayerHit();
            Destroy(gameObject);
        }
    }

    public void Kill()
    {
        GameManager.Instance?.OnEnemyKilled();
        Destroy(gameObject);
    }
}

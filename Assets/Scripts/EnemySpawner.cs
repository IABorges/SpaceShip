using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public float minSpawnDelay = 0.3f;
    public float maxSpawnDelay = 0.8f;

    float halfWidth;
    float halfHeight;
    float timer;
    float nextSpawnTime;

    void Start()
    {
        Camera cam = Camera.main;
        halfHeight = cam.orthographicSize;
        halfWidth = halfHeight * cam.aspect;
        ScheduleNext();
    }

    // Time.deltaTime respeita o tempo lento: no efeito, surgem menos inimigos.
    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= nextSpawnTime)
        {
            Spawn();
            timer = 0f;
            ScheduleNext();
        }
    }

    void ScheduleNext()
    {
        nextSpawnTime = Random.Range(minSpawnDelay, maxSpawnDelay);
    }

    void Spawn()
    {
        if (enemyPrefab == null) return;
        float y = Random.Range(-halfHeight + 1f, halfHeight - 1f);
        Instantiate(enemyPrefab, new Vector3(halfWidth + 1f, y, 0f), Quaternion.identity);
    }
}

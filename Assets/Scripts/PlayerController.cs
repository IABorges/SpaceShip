using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SpriteRenderer))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 6f;
    public float fireRate = 0.2f;
    public GameObject bulletPrefab;
    public Transform firePoint;

    float nextFireTime;
    Vector2 halfView;
    Vector2 halfShip;

    void Start()
    {
        Camera cam = Camera.main;
        halfView = new Vector2(cam.orthographicSize * cam.aspect, cam.orthographicSize);
        halfShip = GetComponent<SpriteRenderer>().bounds.extents;
    }

    void Update()
    {
        if (GameManager.IsOver) return;

        var kb = Keyboard.current;
        if (kb == null) return;

        Vector2 move = Vector2.zero;
        if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) move.x -= 1;
        if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) move.x += 1;
        if (kb.wKey.isPressed || kb.upArrowKey.isPressed) move.y += 1;
        if (kb.sKey.isPressed || kb.downArrowKey.isPressed) move.y -= 1;

        Vector3 pos = transform.position + (Vector3)(move.normalized * moveSpeed * GameManager.PlayerDeltaTime);
        pos.x = Mathf.Clamp(pos.x, -halfView.x + halfShip.x, halfView.x - halfShip.x);
        pos.y = Mathf.Clamp(pos.y, -halfView.y + halfShip.y, halfView.y - halfShip.y);
        transform.position = pos;

        if (kb.spaceKey.isPressed && Time.unscaledTime >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.unscaledTime + fireRate;
        }
    }

    void Shoot()
    {
        if (bulletPrefab == null) return;
        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
        // O sprite do tiro aponta para cima; gira para seguir para a direita.
        Instantiate(bulletPrefab, spawnPos, Quaternion.Euler(0f, 0f, -90f));
    }
}

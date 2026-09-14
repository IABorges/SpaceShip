using UnityEngine;

public class Parallax : MonoBehaviour
{
    private float lenght;
    public float parallaxEffect;

    void Start()
    {
        lenght = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    void Update()
    {
        transform.position += Vector3.left * Time.deltaTime * parallaxEffect;
        if (transform.position.x < -lenght)
        {
            transform.position += Vector3.right * lenght * 2f;
        }
    }
}

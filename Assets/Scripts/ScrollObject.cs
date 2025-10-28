using UnityEngine;

public class ScrollObject : MonoBehaviour
{
    public bool destroyWhenInvisible = true;
    public float speed = 30f;

    void Update()
    {
        float moveSpeed = GameManager.Instance != null ? GameManager.Instance.currentSpeed : speed;
        transform.Translate(Vector3.left * moveSpeed * Time.deltaTime, Space.World);

        if (destroyWhenInvisible && transform.position.x < -60f)
            Destroy(gameObject);
    }
}

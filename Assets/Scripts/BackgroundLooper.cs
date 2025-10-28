using UnityEngine;

public class BackgroundLooper : MonoBehaviour
{
    public float backgroundWidth = 40f; // ancho X del fondo
    public float parallaxFactor = 0.5f; // 0.5 = se mueve la mitad de la velocidad del ground
    private Vector3 initialPos;

    void Start()
    {
        initialPos = transform.position;
    }

    void Update()
    {
        float speed = (GameManager.Instance != null ? GameManager.Instance.currentSpeed : 0f) * parallaxFactor;
        transform.Translate(Vector3.left * speed * Time.deltaTime, Space.World);

        if (transform.position.x < initialPos.x - backgroundWidth)
            transform.position = new Vector3(initialPos.x + backgroundWidth, initialPos.y, initialPos.z);
    }
}

using UnityEngine;
using System.Collections;

public class AfterimageTrail : MonoBehaviour
{
    [Header("Configuración del rastro")]
    public GameObject ghostPrefab; // Asigna tu PlayerGhost aquí
    public float spawnInterval = 0.1f;
    public float lifetime = 0.6f;
    public Vector3 offset = Vector3.zero;

    private bool spawning = true;

    void Start()
    {
        StartCoroutine(SpawnTrail());
    }

    IEnumerator SpawnTrail()
    {
        while (spawning)
        {
            GameObject ghost = Instantiate(ghostPrefab, transform.position + offset, transform.rotation);
            Destroy(ghost, lifetime);
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    public void StopTrail() => spawning = false;
}
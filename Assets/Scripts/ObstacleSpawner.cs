using UnityEngine;
using System.Collections;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Prefabs de obstáculos")]
    public GameObject[] obstaclePrefabs;

    [Header("Posiciones posibles")]
    public float spawnX = 30f;

    private bool spawning = true;

    void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (spawning)
        {
            SpawnObstacle();
            yield return new WaitForSeconds(GameManager.Instance.currentSpawnInterval);
        }
    }

    void SpawnObstacle()
    {
        int index = Random.Range(0, obstaclePrefabs.Length);
        GameObject prefab = obstaclePrefabs[index];

        // ✅ Tomar la altura y rotación EXACTA del prefab original
        Vector3 prefabLocalPos = prefab.transform.localPosition;
        Quaternion prefabRot = prefab.transform.rotation;

        // Mantener su misma Y del prefab, pero con X en la posición de spawn
        Vector3 spawnPos = new Vector3(spawnX, prefabLocalPos.y, prefabLocalPos.z);

        GameObject obstacle = Instantiate(prefab, spawnPos, prefabRot);

        // Asignar movimiento
        var scroll = obstacle.GetComponent<ScrollObject>();
        if (scroll == null) scroll = obstacle.AddComponent<ScrollObject>();
        scroll.speed = GameManager.Instance.currentSpeed;
    }
}
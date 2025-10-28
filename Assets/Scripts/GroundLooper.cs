using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class GroundLooper : MonoBehaviour
{
    public GameObject groundPrefab;   // prefab tile (pivot en base)
    public int tiles = 2;             // cuantos tiles usar para loop (2 es suficiente)
    public float groundWidth = 20f;   // ancho del tile en unidades (ajústalo desde inspector)
    private GameObject[] spawned;
    private Vector3 startPos;

    void Start()
    {
        if (groundPrefab == null)
        {
            Debug.LogError("GroundLooper: groundPrefab no asignado.");
            enabled = false;
            return;
        }

        spawned = new GameObject[tiles];
        startPos = transform.position;

        // Crear tiles en fila
        float x = startPos.x;
        for (int i = 0; i < tiles; i++)
        {
            spawned[i] = Instantiate(groundPrefab, new Vector3(x, startPos.y, startPos.z), Quaternion.identity, transform);
            // aseguramos que tengan ScrollObject (opcional; ScrollObject usará GameManager)
            if (spawned[i].GetComponent<ScrollObject>() == null)
                spawned[i].AddComponent<ScrollObject>();
            x += groundWidth;
        }
    }

    void Update()
    {
        // Si un tile sale por la izquierda, reposicionarlo a la derecha del último tile
        for (int i = 0; i < tiles; i++)
        {
            GameObject g = spawned[i];
            if (g == null) continue;

            if (g.transform.position.x < startPos.x - groundWidth)
            {
                // encontrar la máxima X entre tiles actuales
                float maxX = float.MinValue;
                for (int j = 0; j < tiles; j++)
                    if (spawned[j] != null && spawned[j].transform.position.x > maxX)
                        maxX = spawned[j].transform.position.x;

                g.transform.position = new Vector3(maxX + groundWidth, g.transform.position.y, g.transform.position.z);
            }
        }
    }
}

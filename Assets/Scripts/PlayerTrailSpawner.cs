using UnityEngine;

public class PlayerTrailSpawner : MonoBehaviour
{
    [Header("Referencia del prefab del rastro")]
    public GameObject trailPrefab;

    [Header("Configuración del rastro")]
    public float spawnRate = 0.05f;   // Tiempo entre cada ghost
    public float trailLifetime = 0.5f; // Cuánto tarda en desaparecer cada uno

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnRate)
        {
            SpawnTrail();
            timer = 0f;
        }
    }

    void SpawnTrail()
    {
        // Instancia un ghost en la posición y rotación actual
        GameObject ghost = Instantiate(trailPrefab, transform.position, transform.rotation);

        // Copia la escala
        ghost.transform.localScale = transform.localScale;

        // Si el jugador tiene un SkinnedMeshRenderer (por ejemplo, modelo 3D animado)
        // copiamos la pose actual al ghost
        SkinnedMeshRenderer skinned = GetComponentInChildren<SkinnedMeshRenderer>();
        if (skinned != null)
        {
            Mesh mesh = new Mesh();
            skinned.BakeMesh(mesh); // Captura la pose actual del jugador
            MeshFilter mf = ghost.GetComponent<MeshFilter>();
            if (mf == null)
                mf = ghost.AddComponent<MeshFilter>();
            mf.mesh = mesh;
        }

        // Asegura que tenga un MeshRenderer con el material del trail
        MeshRenderer renderer = ghost.GetComponent<MeshRenderer>();
        if (renderer == null)
            renderer = ghost.AddComponent<MeshRenderer>();

        // Configura el material del ghost (el shader de fade)
        // Si tu prefab ya lo tiene asignado, puedes omitir esta parte
        // renderer.material = trailMaterial;

        // Destruye el ghost después del tiempo de vida
        Destroy(ghost, trailLifetime);
    }
}
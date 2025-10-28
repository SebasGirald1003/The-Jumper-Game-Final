using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement; // Necesario para reiniciar escenas

public class GameManager : MonoBehaviour
{
    [Header("Velocidad y dificultad")]
    public float baseSpeed = 6f;
    public float speedIncrease = 0.5f;
    public float maxSpeed = 20f;

    public float baseSpawnInterval = 2f;
    public float minSpawnInterval = 0.7f;
    public float difficultyIncreaseInterval = 10f;

    [Header("Puntaje")]
    public TextMeshProUGUI scoreText;       // Texto del puntaje actual
    public TextMeshProUGUI bestScoreText;   // Texto del récord
    private float scoreTime = 0f;           // Contador de tiempo
    private bool isPlaying = false;         // Control del estado del juego
    private float bestScore = 0f;           // Récord guardado

    [Header("Game Over Settings")]
    public float restartDelay = 2.5f;       // Tiempo antes de reiniciar el juego automáticamente

    [HideInInspector] public float currentSpeed;
    [HideInInspector] public float currentSpawnInterval;
    private float timeSinceDifficultyUp = 0f;

    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        currentSpeed = baseSpeed;
        currentSpawnInterval = baseSpawnInterval;

        // Cargar el récord guardado desde PlayerPrefs
        bestScore = PlayerPrefs.GetFloat("BestScore", 0f);
        UpdateBestScoreText();


    }

    private void Update()
    {
        if (isPlaying)
        {
            // 🕓 Aumentar puntaje con el tiempo
            scoreTime += Time.deltaTime;
            UpdateScoreText();

            // 💪 Aumentar dificultad cada cierto tiempo
            timeSinceDifficultyUp += Time.deltaTime;
            if (timeSinceDifficultyUp >= difficultyIncreaseInterval)
            {
                IncreaseDifficulty();
                timeSinceDifficultyUp = 0f;
            }
        }
    }

    // -------- CONTROL DEL JUEGO --------
    public void StartGame()
    {
        scoreTime = 0f;
        isPlaying = true;
        UpdateScoreText();
    }

    public void GameOver()
    {
        isPlaying = false;

        // 🎯 Verificar y guardar récord
        if (scoreTime > bestScore)
        {
            bestScore = scoreTime;
            PlayerPrefs.SetFloat("BestScore", bestScore);
            PlayerPrefs.Save();
        }

        UpdateBestScoreText();

        // 🔄 Reiniciar juego automáticamente
        Invoke(nameof(RestartGame), restartDelay);
    }

    private void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // -------- UI --------
    private void UpdateScoreText()
    {
        if (scoreText != null)
            scoreText.text = $"Time: {scoreTime:F2}";
    }

    private void UpdateBestScoreText()
    {
        if (bestScoreText != null)
            bestScoreText.text = $"Record: {bestScore:F2}";
    }

    // -------- DIFICULTAD --------
    private void IncreaseDifficulty()
    {
        currentSpeed = Mathf.Min(currentSpeed + speedIncrease, maxSpeed);
        currentSpawnInterval = Mathf.Max(currentSpawnInterval - 0.1f, minSpawnInterval);
    }

    public void ResetBestScore()
    {
    bestScore = 0f;
    PlayerPrefs.SetFloat("BestScore", 0f);
    PlayerPrefs.Save();
    UpdateBestScoreText();
    }

}
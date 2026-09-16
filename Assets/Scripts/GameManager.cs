using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    enum State { Playing, Won, Lost }

    public static GameManager Instance { get; private set; }

    // Delta time da nave e dos tiros: ignora o tempo lento, para o jogador ter vantagem.
    public static float PlayerDeltaTime => Instance != null && Instance.state != State.Playing ? 0f : Time.unscaledDeltaTime;
    public static bool IsOver => Instance != null && Instance.state != State.Playing;

    [Header("Regras")]
    public int startingLives = 3;
    public int pointsPerEnemy = 10;
    public int winScore = 300;

    [Header("Tempo lento (tecla E)")]
    public int startingSlowMoCharges = 1;
    public int slowMoScoreStep = 50;      // a cada N pontos o jogador ganha 1 uso
    public float slowMoDuration = 5f;
    [Range(0.1f, 1f)] public float slowMoScale = 0.4f;

    [Header("HUD")]
    public Text scoreText;
    public Text livesText;
    public Text slowMoText;
    public GameObject slowMoOverlay;

    [Header("Telas de fim de jogo")]
    public GameObject endPanel;
    public Text endTitleText;
    public Text endScoreText;

    static readonly Color SlowMoActiveColor = new Color(0.5f, 0.85f, 1f);
    static readonly Color SlowMoReadyColor = new Color(1f, 1f, 1f, 0.9f);
    static readonly Color SlowMoEmptyColor = new Color(1f, 1f, 1f, 0.4f);

    State state = State.Playing;
    int score;
    int lives;
    int slowMoCharges;
    int slowMoTier;
    bool slowMoActive;
    float slowMoEndTime;
    float baseFixedDeltaTime;

    void Awake()
    {
        Instance = this;
        baseFixedDeltaTime = Time.fixedDeltaTime;
        Time.timeScale = 1f;
        lives = startingLives;
        slowMoCharges = startingSlowMoCharges;
        if (endPanel != null) endPanel.SetActive(false);
        if (slowMoOverlay != null) slowMoOverlay.SetActive(false);
        if (slowMoText != null) slowMoText.gameObject.SetActive(true);
        UpdateHud();
        UpdateSlowMoText();
    }

    void OnDestroy()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = baseFixedDeltaTime;
        if (Instance == this) Instance = null;
    }

    void Update()
    {
        var kb = Keyboard.current;

        if (state != State.Playing)
        {
            if (kb != null && kb.rKey.wasPressedThisFrame) Restart();
            return;
        }

        if (slowMoActive && Time.unscaledTime >= slowMoEndTime) EndSlowMo();

        if (kb != null && kb.eKey.wasPressedThisFrame && !slowMoActive && slowMoCharges > 0)
        {
            slowMoCharges--;
            StartSlowMo();
        }

        UpdateSlowMoText();
    }

    public void AddScore(int amount)
    {
        if (state != State.Playing) return;
        score += amount;
        UpdateHud();

        if (score >= winScore)
        {
            EndGame(State.Won);
            return;
        }

        // Cada multiplo do passo alcancado da 1 uso do tempo lento.
        int tier = score / slowMoScoreStep;
        if (tier > slowMoTier)
        {
            slowMoCharges += tier - slowMoTier;
            slowMoTier = tier;
        }
    }

    public void OnEnemyKilled()
    {
        AddScore(pointsPerEnemy);
    }

    public void OnPlayerHit()
    {
        if (state != State.Playing) return;
        lives--;
        UpdateHud();
        if (lives <= 0) EndGame(State.Lost);
    }

    void StartSlowMo()
    {
        slowMoActive = true;
        slowMoEndTime = Time.unscaledTime + slowMoDuration;
        SetTimeScale(slowMoScale);
        if (slowMoOverlay != null) slowMoOverlay.SetActive(true);
    }

    void EndSlowMo()
    {
        slowMoActive = false;
        SetTimeScale(1f);
        if (slowMoOverlay != null) slowMoOverlay.SetActive(false);
    }

    // Mantem o passo da fisica proporcional, para os triggers continuarem sendo
    // avaliados na mesma frequencia em tempo real durante o tempo lento.
    void SetTimeScale(float scale)
    {
        Time.timeScale = scale;
        Time.fixedDeltaTime = baseFixedDeltaTime * scale;
    }

    void EndGame(State result)
    {
        state = result;
        slowMoActive = false;
        if (slowMoOverlay != null) slowMoOverlay.SetActive(false);
        if (slowMoText != null) slowMoText.gameObject.SetActive(false);
        Time.fixedDeltaTime = baseFixedDeltaTime;
        Time.timeScale = 0f;

        if (endPanel != null) endPanel.SetActive(true);
        if (endTitleText != null)
        {
            bool won = result == State.Won;
            endTitleText.text = won ? "VITORIA!" : "DERROTA";
            endTitleText.color = won ? new Color(0.4f, 1f, 0.5f) : new Color(1f, 0.35f, 0.35f);
        }
        if (endScoreText != null)
            endScoreText.text = "Pontuacao: " + score + "\n\nPressione R para jogar novamente";
    }

    void Restart()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = baseFixedDeltaTime;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void UpdateHud()
    {
        if (scoreText != null) scoreText.text = "Pontos: " + score + " / " + winScore;
        if (livesText != null) livesText.text = "Vidas: " + lives;
    }

    void UpdateSlowMoText()
    {
        if (slowMoText == null) return;

        if (slowMoActive)
        {
            slowMoText.text = "TEMPO LENTO  " + (slowMoEndTime - Time.unscaledTime).ToString("0.0") + "s";
            slowMoText.color = SlowMoActiveColor;
        }
        else
        {
            slowMoText.text = "Tempo lento [E]: " + slowMoCharges;
            slowMoText.color = slowMoCharges > 0 ? SlowMoReadyColor : SlowMoEmptyColor;
        }
    }
}

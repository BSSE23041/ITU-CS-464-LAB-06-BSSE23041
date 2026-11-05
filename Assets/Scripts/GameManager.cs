using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

// Lecture 06's GameManager + Lecture 07's lives, Respawn and Lose.
// One per level (make it a prefab; it keeps its NUMBERS, so reassign the texts,
// panels, player and respawn point in each scene).
//
// The four ways a level can be won or lost, all decided HERE:
//   Collect all   : hasGoal = false, win in AddScore when score >= targetScore
//   Reach the goal: hasGoal = true,  win in ReachGoal() if score >= targetScore
//   Timer         : timeLimit > 0, Lose("Time's up") when it hits 0
//   Survive       : lives; Lose when the last one goes
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Objective")]
    [SerializeField] int targetScore = 6;
    [SerializeField] bool hasGoal = false;
    [Tooltip("0 = no timer")]
    [SerializeField] float timeLimit = 0f;
    [Tooltip("Off: running out of time LOSES (race). On: lasting until it hits 0 WINS (survive).")]
    [SerializeField] bool timerWins = false;

    [Header("Lives")]
    [SerializeField] int lives = 3;
    [SerializeField] float respawnDelay = 0.6f;
    [SerializeField] float spawnProtection = 1.5f;

    [Header("Scene references (reassign in every level)")]
    [SerializeField] Health player;
    [SerializeField] Transform respawnPoint;
    [SerializeField] TMP_Text scoreText;
    [SerializeField] TMP_Text livesText;
    [SerializeField] TMP_Text timerText;        // optional
    [SerializeField] TMP_Text messageText;      // optional: "Collect 2 more coins!"
    [SerializeField] TMP_Text loseReasonText;   // optional, on the lose panel
    [SerializeField] GameObject winPanel, losePanel;
    [SerializeField] GameObject nextLevelButton; // optional, hidden on the last level

    int score;
    float timeLeft;
    int shownSecond = -1;
    bool ended, respawning;
    float messageUntil;

    void Awake() => Instance = this;

    void Start()
    {
        timeLeft = timeLimit;
        winPanel.SetActive(false);
        losePanel.SetActive(false);
        if (messageText) messageText.text = "";
        if (timerText) timerText.gameObject.SetActive(timeLimit > 0f);
        UpdateUI();
    }

    void Update()
    {
        if (ended) return;

        if (timeLimit > 0f)
        {
            timeLeft -= Time.deltaTime;
            int sec = Mathf.CeilToInt(Mathf.Max(timeLeft, 0f));
            if (sec != shownSecond && timerText)            // set a TMP_Text only when it changes
            {
                shownSecond = sec;
                timerText.text = $"Time: {sec}";
            }
            if (timeLeft <= 0f)
            {
                if (timerWins) Win();            // survived the whole timer
                else Lose("Time's up!");
            }
        }

        if (messageText && messageUntil > 0f && Time.time > messageUntil)
        {
            messageText.text = "";
            messageUntil = 0f;
        }
    }

    // ---------------- score and winning ----------------
    public void AddScore(int amount)
    {
        if (ended) return;
        score += amount;
        UpdateUI();
        if (!hasGoal && score >= targetScore) Win();
    }

    public void ReachGoal()
    {
        if (ended) return;
        if (score >= targetScore) Win();
        else ShowMessage($"Collect {targetScore - score} more coin(s) first!");
    }

    public void SetCheckpoint(Transform point) => respawnPoint = point;

    public void UpdateUI()
    {
        scoreText.text = $"Coins: {score} / {targetScore}";
        livesText.text = $"Lives: {lives}   Health: {Mathf.Max(player.Current, 0)} / {player.Max}"
                         + (player.shielded ? "   [Shield]" : "");
    }

    void ShowMessage(string text)
    {
        if (!messageText) return;
        messageText.text = text;
        messageUntil = Time.time + 2f;
    }

    // ---------------- losing a life ----------------
    // Called by Health (hearts gone) and by KillZone (fell off).
    public void PlayerDied()
    {
        if (ended || respawning) return;
        lives--;
        UpdateUI();
        if (lives <= 0) Lose("Out of lives");
        else StartCoroutine(Respawn());
    }

    IEnumerator Respawn()
    {
        respawning = true;
        // A beat first: a respawn inside the hit lets the queued knockback slide you away.
        yield return new WaitForSeconds(respawnDelay);

        Rigidbody body = player.GetComponent<Rigidbody>();
        body.position = respawnPoint.position;
        player.GetComponent<PlayerController>().ResetMotion();   // new life starts still
        player.Restore(spawnProtection);
        respawning = false;
    }

    // ---------------- the ends ----------------
    void Win()
    {
        if (ended) return;
        ended = true;
        winPanel.SetActive(true);
        if (nextLevelButton)
            nextLevelButton.SetActive(SceneManager.GetActiveScene().buildIndex + 1
                                      < SceneManager.sceneCountInBuildSettings);
        Time.timeScale = 0f;
    }

    void Lose(string reason)
    {
        if (ended) return;
        ended = true;
        if (loseReasonText) loseReasonText.text = reason;
        losePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    // ---------------- buttons ----------------
    public void Restart()
    {
        Time.timeScale = 1f;                      // it survives a scene load, so reset it FIRST
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void NextLevel()
    {
        Time.timeScale = 1f;
        int next = SceneManager.GetActiveScene().buildIndex + 1;
        if (next < SceneManager.sceneCountInBuildSettings) SceneManager.LoadScene(next);
    }
}

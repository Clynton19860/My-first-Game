using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages scoring system, high scores, and score-related events.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    [Header("Score Settings")]
    [SerializeField] private int currentScore = 0;
    [SerializeField] private int highScore = 0;
    [SerializeField] private int scorePerKill = 100;
    [SerializeField] private int scorePerHeadshot = 50;
    [SerializeField] private int comboMultiplier = 1;
    
    [Header("Combo System")]
    [SerializeField] private float comboTimeWindow = 3f;
    [SerializeField] private int maxComboMultiplier = 5;
    
    // Events
    public System.Action<int> OnScoreChanged;
    public System.Action<int> OnHighScoreChanged;
    public System.Action<int> OnComboChanged;
    
    // Properties
    public int CurrentScore => currentScore;
    public int HighScore => highScore;
    public int ComboMultiplier => comboMultiplier;
    
    // Private variables
    private float lastKillTime;
    private List<ScoreEvent> recentKills = new List<ScoreEvent>();
    
    private void Start()
    {
        LoadHighScore();
    }
    
    private void Update()
    {
        UpdateComboSystem();
    }
    
    /// <summary>
    /// Add score for killing an enemy
    /// </summary>
    public void AddKillScore(Enemy enemy, bool isHeadshot = false)
    {
        int baseScore = scorePerKill;
        
        if (isHeadshot)
        {
            baseScore += scorePerHeadshot;
        }
        
        int totalScore = baseScore * comboMultiplier;
        AddScore(totalScore);
        
        // Record kill for combo system
        recentKills.Add(new ScoreEvent(Time.time, totalScore));
        
        // Update combo
        UpdateComboMultiplier();
        
        Debug.Log($"Killed enemy! Score: +{totalScore} (Combo: x{comboMultiplier})");
    }
    
    /// <summary>
    /// Add score for other actions
    /// </summary>
    public void AddScore(int points)
    {
        currentScore += points;
        OnScoreChanged?.Invoke(currentScore);
        
        // Check for new high score
        if (currentScore > highScore)
        {
            highScore = currentScore;
            SaveHighScore();
            OnHighScoreChanged?.Invoke(highScore);
        }
    }
    
    /// <summary>
    /// Update combo multiplier based on recent kills
    /// </summary>
    private void UpdateComboMultiplier()
    {
        // Count kills within time window
        int killsInWindow = 0;
        float currentTime = Time.time;
        
        for (int i = recentKills.Count - 1; i >= 0; i--)
        {
            if (currentTime - recentKills[i].time <= comboTimeWindow)
            {
                killsInWindow++;
            }
            else
            {
                // Remove old kills
                recentKills.RemoveAt(i);
            }
        }
        
        // Calculate new combo multiplier
        int newCombo = Mathf.Min(killsInWindow, maxComboMultiplier);
        
        if (newCombo != comboMultiplier)
        {
            comboMultiplier = newCombo;
            OnComboChanged?.Invoke(comboMultiplier);
        }
    }
    
    /// <summary>
    /// Update combo system
    /// </summary>
    private void UpdateComboSystem()
    {
        // Remove old kills outside time window
        float currentTime = Time.time;
        recentKills.RemoveAll(kill => currentTime - kill.time > comboTimeWindow);
        
        // Update combo if needed
        UpdateComboMultiplier();
    }
    
    /// <summary>
    /// Reset score for new game
    /// </summary>
    public void ResetScore()
    {
        currentScore = 0;
        comboMultiplier = 1;
        recentKills.Clear();
        OnScoreChanged?.Invoke(currentScore);
        OnComboChanged?.Invoke(comboMultiplier);
    }
    
    /// <summary>
    /// Save high score to PlayerPrefs
    /// </summary>
    private void SaveHighScore()
    {
        PlayerPrefs.SetInt("HighScore", highScore);
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// Load high score from PlayerPrefs
    /// </summary>
    private void LoadHighScore()
    {
        highScore = PlayerPrefs.GetInt("HighScore", 0);
    }
    
    /// <summary>
    /// Get score as formatted string
    /// </summary>
    public string GetScoreText()
    {
        return currentScore.ToString("D6");
    }
    
    /// <summary>
    /// Get high score as formatted string
    /// </summary>
    public string GetHighScoreText()
    {
        return highScore.ToString("D6");
    }
    
    /// <summary>
    /// Get combo text
    /// </summary>
    public string GetComboText()
    {
        if (comboMultiplier > 1)
        {
            return $"COMBO x{comboMultiplier}";
        }
        return "";
    }
}

/// <summary>
/// Represents a score event for combo tracking
/// </summary>
[System.Serializable]
public struct ScoreEvent
{
    public float time;
    public int score;
    
    public ScoreEvent(float time, int score)
    {
        this.time = time;
        this.score = score;
    }
} 
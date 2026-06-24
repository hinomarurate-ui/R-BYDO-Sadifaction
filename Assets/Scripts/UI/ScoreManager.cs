using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class ScoreManager : MonoBehaviour
{
    [FormerlySerializedAs("Score")]
    [SerializeField] int score;
    [FormerlySerializedAs("ScoreText")]
    [SerializeField] TextMeshProUGUI scoreText;
    [FormerlySerializedAs("ViewScore")]
    [SerializeField] int displayedScore;
    [SerializeField] int displayStepPerFrame = 1;

    void Awake()
    {
        RefreshScoreText();
    }

    void Update()
    {
        if(displayedScore != score)
        {
            int step = Mathf.Max(1, displayStepPerFrame);
            displayedScore = Mathf.RoundToInt(Mathf.MoveTowards(displayedScore, score, step));
            RefreshScoreText();
        }
    }

    public void AddScore(int amount)
    {
        if(amount <= 0)
        {
            return;
        }

        score = Mathf.Max(0, score + amount);
    }

    void RefreshScoreText()
    {
        if(scoreText != null)
        {
            scoreText.text = displayedScore.ToString("00000000");
        }
    }
}

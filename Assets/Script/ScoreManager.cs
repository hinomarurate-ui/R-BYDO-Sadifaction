using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 実装意図: 敵撃破時に加算されたスコアを保持し、表示は 1 フレームずつ増える演出にする。
public class ScoreManager : MonoBehaviour
{
    [SerializeField] int Score;
    [SerializeField] TextMeshProUGUI ScoreText;
    [SerializeField] int ViewScore;

    // Update is called once per frame
    void Update()
    {
        if(ViewScore < Score)
        {
            ViewScore += 1;
        }

        if(ScoreText != null)
        {
            ScoreText.text = ViewScore.ToString("00000000");
        }
    }

    public void AddScore(int plusScore)
    {
        // 実装意図: EnemyHealth からは加算だけを呼ばせ、表示形式や増加演出は ScoreManager に閉じ込める。
        Score += plusScore;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

        ScoreText.text = ViewScore.ToString("00000000") ;
    }

    public void AddScore(int plusScore)
    {
        Score += plusScore;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 実装意図: 経過時間を分秒で表示するだけの軽い UI component として、ゲーム進行ロジックから分離する。
public class Timer : MonoBehaviour
{
    private float sec;
    private float min;
    [SerializeField] TextMeshProUGUI dateTimeText;

    // Update is called once per frame
    void Update()
    {
        sec += Time.deltaTime;
        if(sec >= 60f)
        {

            sec -= 60f;

            min++;
        }

        if(dateTimeText != null)
        {
            dateTimeText.text = min.ToString("00") + ":" + ((int)sec).ToString("00");
        }
    }
}

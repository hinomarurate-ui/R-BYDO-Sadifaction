using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Timer : MonoBehaviour
{
    private float sec;
    private float min;
    [SerializeField] TextMeshProUGUI dateTimeText;

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

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

    // Update is called once per frame
    void Update()
    {
        sec += Time.deltaTime;
        if(sec > 60){

            sec = 0;

            min++;
        }

        dateTimeText.text = min.ToString("00") + ":" + ((int)sec).ToString("00");
    }
}

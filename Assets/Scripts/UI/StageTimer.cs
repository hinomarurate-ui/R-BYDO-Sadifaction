using TMPro;
using UnityEngine;

public class StageTimer : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI dateTimeText;

    float elapsedSeconds;

    void Update()
    {
        elapsedSeconds += Time.deltaTime;
        RefreshText();
    }

    void RefreshText()
    {
        if(dateTimeText == null)
        {
            return;
        }

        int totalSeconds = Mathf.FloorToInt(elapsedSeconds);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        dateTimeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}

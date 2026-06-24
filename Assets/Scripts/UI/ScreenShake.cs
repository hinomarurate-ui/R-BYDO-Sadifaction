using UnityEngine;

[DefaultExecutionOrder(100000)]
public class ScreenShake : MonoBehaviour
{
    float amplitude;
    float duration;
    float remainingTime;
    Vector3 currentOffset;

    public static void Shake(float power, float time)
    {
        if(power <= 0f || time <= 0f)
        {
            return;
        }

        Camera mainCamera = Camera.main;
        if(mainCamera == null)
        {
            return;
        }

        ScreenShake shaker = mainCamera.GetComponent<ScreenShake>();
        if(shaker == null)
        {
            shaker = mainCamera.gameObject.AddComponent<ScreenShake>();
        }

        shaker.StartShake(power, time);
    }

    void LateUpdate()
    {
        RemoveCurrentOffset();

        if(remainingTime <= 0f)
        {
            amplitude = 0f;
            duration = 0f;
            return;
        }

        float rate = duration > 0f ? remainingTime / duration : 0f;
        Vector2 random = Random.insideUnitCircle * amplitude * rate;
        currentOffset = new Vector3(random.x, random.y, 0f);
        transform.localPosition += currentOffset;
        remainingTime -= Time.deltaTime;
    }

    void OnDisable()
    {
        RemoveCurrentOffset();
    }

    void StartShake(float power, float time)
    {
        amplitude = Mathf.Max(amplitude, power);
        duration = Mathf.Max(duration, time);
        remainingTime = Mathf.Max(remainingTime, time);
    }

    void RemoveCurrentOffset()
    {
        if(currentOffset == Vector3.zero)
        {
            return;
        }

        transform.localPosition -= currentOffset;
        currentOffset = Vector3.zero;
    }
}

using UnityEngine;

[DefaultExecutionOrder(100000)]
public class ShakeScreen : MonoBehaviour
{
    float shakePower;
    float shakeTime;
    float shakeTimer;
    Vector3 currentOffset;
    // Start is called before the first frame update
    public static void Shake(float power, float time)
    {
        if(power <= 0f || time <= 0f) return;

        Camera mainCamera = Camera.main;
        if(mainCamera == null) return;

        ShakeScreen shaker = mainCamera.GetComponent<ShakeScreen>();
        if(shaker == null)
        {
            shaker = mainCamera.gameObject.AddComponent<ShakeScreen>();
        }

        shaker.StartShake(power, time);
        
    }

    // Update is called once per frame
    void StartShake(float power,float time)
    {
        shakePower = Mathf.Max(shakePower, power);
        shakeTime = Mathf.Max(shakeTime, time);
        shakeTimer = Mathf.Max(shakeTimer, time);
    }

    void LateUpdate()
    {
        transform.localPosition -= currentOffset;
        currentOffset = Vector3.zero;

        if(shakeTimer <= 0f)
        {
            shakePower = 0f;
            shakeTime = 0f;
            return;
        }

        float rate = shakeTime > 0f ? shakeTimer / shakeTime : 0f;
        Vector2 random = Random.insideUnitCircle * shakePower * rate;
        currentOffset = new Vector3(random.x, random.y, 0f);
        transform.localPosition += currentOffset;

        shakeTimer -= Time.deltaTime;
        
    }
}

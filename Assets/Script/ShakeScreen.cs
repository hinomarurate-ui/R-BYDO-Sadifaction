using UnityEngine;

[DefaultExecutionOrder(100000)]
// 実装意図: 撃破などのヒット演出から static 呼び出しできる、Camera 用の一時的な画面揺れ処理にする。
public class ShakeScreen : MonoBehaviour
{
    float shakePower;
    float shakeTime;
    float shakeTimer;
    Vector3 currentOffset;
    // Start is called before the first frame update
    public static void Shake(float power, float time)
    {
        // 実装意図: MainCamera に component が無ければその場で足し、呼び出し側に事前設定を要求しない。
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
        // 実装意図: 前フレームの揺れを戻してから新しい offset を足し、カメラ位置のドリフトを防ぐ。
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

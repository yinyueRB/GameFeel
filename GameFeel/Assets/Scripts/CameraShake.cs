using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance; // 单例模式，方便其他脚本直接调用
    private Vector3 originalPos;

    void Awake()
    {
        Instance = this;
        originalPos = transform.localPosition;
    }

    // 外部调用的震动接口：传入震动持续时间和震动幅度
    public void Shake(float duration, float magnitude)
    {
        StartCoroutine(ShakeCoroutine(duration, magnitude));
    }

    IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(x, y, originalPos.z);
            
            // 注意这里用 unscaledDeltaTime，这样即使触发了“顿帧(时间减缓)”，摄像机依然会全速震动！
            elapsed += Time.unscaledDeltaTime; 
            yield return null;
        }

        transform.localPosition = originalPos; // 震完归位
    }
}
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;
    private Vector3 originalPos;

    [Header("闪屏设置")]
    public Image flashPanel; // 拖入红色FlashPanel的Image组件
    public float flashDuration = 0.2f;
    public Color flashColor = Color.red; // 闪屏颜色，可调整

    void Awake()
    {
        Instance = this;
        originalPos = transform.localPosition;

        if (flashPanel != null)
        {
            // 初始化面板为透明
            Color c = flashColor;
            c.a = 0f;
            flashPanel.color = c;
            flashPanel.gameObject.SetActive(false);
        }
    }

    // 震动接口
    public void Shake(float duration, float magnitude)
    {
        StartCoroutine(ShakeCoroutine(duration, magnitude));
    }

    // 死亡时调用闪屏
    public void DeathFlash()
    {
        if (flashPanel != null)
        {
            StartCoroutine(DeathFlashCoroutine());
        }
    }

    // 死亡震动+闪屏
    public void DeathEffect(float shakeDuration = 0.3f, float shakeMagnitude = 0.2f)
    {
        StartCoroutine(ShakeCoroutine(shakeDuration, shakeMagnitude));
        DeathFlash();
    }

    IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(x, y, originalPos.z);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        transform.localPosition = originalPos;
    }

    IEnumerator DeathFlashCoroutine()
    {
        if (flashPanel == null) yield break;

        flashPanel.gameObject.SetActive(true);

        // 淡入
        float timer = 0f;
        while (timer < flashDuration * 0.3f)
        {
            timer += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(0f, 0.6f, timer / (flashDuration * 0.3f));
            Color c = flashColor;
            c.a = alpha;
            flashPanel.color = c;
            yield return null;
        }

        // 淡出
        timer = 0f;
        while (timer < flashDuration * 0.7f)
        {
            timer += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(0.6f, 0f, timer / (flashDuration * 0.7f));
            Color c = flashColor;
            c.a = alpha;
            flashPanel.color = c;
            yield return null;
        }

        // 恢复透明
        Color finalColor = flashColor;
        finalColor.a = 0f;
        flashPanel.color = finalColor;
        flashPanel.gameObject.SetActive(false);
    }
}
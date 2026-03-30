using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class VictoryBackgroundUI : MonoBehaviour
{
    [Header("胜利图片")]
    public Image p1WinImage;
    public Image p2WinImage;

    [Header("效果参数")]
    public float delayBeforeShow = 2f;   // 延迟时间
    public float fadeDuration = 1f;      // 淡入时间

    private bool hasTriggered = false;

    void Start()
    {
        HideAllInstant();
    }

    void Update()
    {
        if (GameManager.Instance == null) return;

        int p1Wins = GameManager.Instance.p1Wins;
        int p2Wins = GameManager.Instance.p2Wins;
        int winsNeeded = GameManager.Instance.winsNeeded;

        // ✅ 触发胜利（只触发一次）
        if (!hasTriggered && (p1Wins >= winsNeeded || p2Wins >= winsNeeded))
        {
            hasTriggered = true;

            if (p1Wins > p2Wins)
                StartCoroutine(ShowWithDelay(p1WinImage));
            else
                StartCoroutine(ShowWithDelay(p2WinImage));
        }

        // ✅ 重新开始时重置
        if (hasTriggered && p1Wins == 0 && p2Wins == 0)
        {
            StopAllCoroutines();
            HideAllInstant();
            hasTriggered = false;
        }
    }

    IEnumerator ShowWithDelay(Image img)
    {
        yield return new WaitForSeconds(delayBeforeShow);

        if (img == null) yield break;

        img.gameObject.SetActive(true);

        // 从透明开始
        Color c = img.color;
        c.a = 0f;
        img.color = c;

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = t / fadeDuration;

            c.a = alpha;
            img.color = c;

            yield return null;
        }

        // 保证最终完全不透明
        c.a = 1f;
        img.color = c;
    }

    void HideAllInstant()
    {
        if (p1WinImage != null)
        {
            p1WinImage.gameObject.SetActive(false);
            SetAlpha(p1WinImage, 0f);
        }

        if (p2WinImage != null)
        {
            p2WinImage.gameObject.SetActive(false);
            SetAlpha(p2WinImage, 0f);
        }
    }

    void SetAlpha(Image img, float a)
    {
        Color c = img.color;
        c.a = a;
        img.color = c;
    }
}
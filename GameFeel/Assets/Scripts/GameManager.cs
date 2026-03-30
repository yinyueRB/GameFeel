using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game Feel - 爽感设置")]
    public float hitStopDuration = 0.15f;
    public float shakeDuration = 0.2f;
    public float shakeMagnitude = 0.3f;

    [Header("Players")]
    public PlayerController p1;
    public PlayerController p2;

    [Header("Match Info")]
    public int p1Wins = 0;
    public int p2Wins = 0;
    public int winsNeeded = 3;
    private bool matchIsOver = false;

    [Header("战斗 UI")]
    public TextMeshProUGUI countdownText;
    public Image fightImage;
    public Image killImage;
    public Image p1MissImage;
    public Image p2MissImage;

    [Header("Audio Clips")]
    public AudioClip tickClip;
    public AudioClip fightClip;
    public AudioClip killClip;

    [Header("结算 UI (左右分屏)")]
    public GameObject settlementPanel;
    public Image p1ResultImage;
    public Image p2ResultImage;
    public Sprite p1WinnerSprite;
    public Sprite p1LoserSprite;
    public Sprite p2WinnerSprite;
    public Sprite p2LoserSprite;
    public Image scoreTitleImage;  // 确保这是正确的引用
    public TextMeshProUGUI finalScoreText;

    [Header("MISS动画参数")]
    [SerializeField] private float missMoveDistance = 200f;
    [SerializeField] private float missTotalDuration = 0.8f;
    [SerializeField] private Vector3 p1MissStartPos;
    [SerializeField] private Vector3 p2MissStartPos;

    [Header("浮动动画参数")]
    [SerializeField] private float floatAmplitude = 8f;       // 稍微减少浮动幅度
    [SerializeField] private float floatSpeed = 1.8f;         // 稍微调慢浮动速度
    [SerializeField] private float scaleAmplitude = 0.05f;    // 大幅减少缩放幅度，更加轻微
    [SerializeField] private float scaleSpeed = 1.2f;         // 调慢缩放速度
    [SerializeField] private float startDelay = 0.3f;         // 减少开始延迟

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (p1MissImage != null) p1MissStartPos = p1MissImage.transform.localPosition;
        if (p2MissImage != null) p2MissStartPos = p2MissImage.transform.localPosition;

        StartCoroutine(RoundStartRoutine());
    }

    void Update()
    {
        if (matchIsOver)
        {
            if (Input.GetKeyDown(KeyCode.Space)) RestartFullMatch();
            else if (Input.GetKeyDown(KeyCode.Escape)) ReturnToTitle();
        }
    }

    IEnumerator RoundStartRoutine()
    {
        p1.canAct = false;
        p2.canAct = false;

        countdownText.gameObject.SetActive(true);
        if (fightImage != null) fightImage.gameObject.SetActive(false);
        killImage.gameObject.SetActive(false);
        p1MissImage.gameObject.SetActive(false);
        p2MissImage.gameObject.SetActive(false);
        settlementPanel.SetActive(false);

        for (int i = 3; i > 0; i--)
        {
            countdownText.text = i.ToString();
            if (tickClip != null && AudioManager.Instance != null) AudioManager.Instance.PlaySFX(tickClip);
            StartCoroutine(PunchScale(countdownText.transform, 1.5f, 0.2f));
            yield return new WaitForSeconds(1f);
        }

        countdownText.gameObject.SetActive(false);
        if (fightImage != null)
        {
            fightImage.gameObject.SetActive(true);
            StartCoroutine(PunchScale(fightImage.transform, 2f, 0.3f));
        }

        if (fightClip != null && AudioManager.Instance != null) AudioManager.Instance.PlaySFX(fightClip, 1.2f);

        p1.canAct = true;
        p2.canAct = true;

        yield return new WaitForSeconds(0.5f);
        if (fightImage != null) fightImage.gameObject.SetActive(false);
    }

    void ResetRound()
    {
        p1.ResetPlayer();
        p2.ResetPlayer();
        StartCoroutine(RoundStartRoutine());
    }

    public void OnPlayerKilled(PlayerController loser)
    {
        if (matchIsOver) return;

        StartCoroutine(HitStopRoutine());
        if (CameraShake.Instance != null)
            CameraShake.Instance.Shake(shakeDuration, shakeMagnitude);

        if (killClip != null && AudioManager.Instance != null) AudioManager.Instance.PlaySFX(killClip);

        ShowKillUI();

        if (loser == p1) p2Wins++;
        else p1Wins++;

        if (p1Wins >= winsNeeded || p2Wins >= winsNeeded)
        {
            Invoke("ShowSettlementScreen", 2.5f);
        }
        else
        {
            Invoke("ResetRound", 3f);
        }
    }

    void ShowSettlementScreen()
    {
        matchIsOver = true;
        p1.canAct = false;
        p2.canAct = false;
        settlementPanel.SetActive(true);

        finalScoreText.text = $"{p1Wins} : {p2Wins}";

        if (p1Wins > p2Wins)
        {
            p1ResultImage.sprite = p1WinnerSprite;
            p2ResultImage.sprite = p2LoserSprite;
        }
        else
        {
            p1ResultImage.sprite = p1LoserSprite;
            p2ResultImage.sprite = p2WinnerSprite;
        }

        // 启动所有UI元素的浮动动画
        StartAllSettlementAnimations();
    }

    void StartAllSettlementAnimations()
    {
        // 确保所有需要动画的元素都启动动画
        if (scoreTitleImage != null)
        {
            StartCoroutine(FloatAndScaleAnimation(scoreTitleImage.transform));
            Debug.Log("Score Title Image 动画已启动");
        }
        else
        {
            Debug.LogWarning("Score Title Image 未分配！请检查Inspector中的引用");
        }

        if (finalScoreText != null)
        {
            StartCoroutine(FloatAndScaleAnimation(finalScoreText.transform));
            Debug.Log("Final Score Text 动画已启动");
        }

        if (p1ResultImage != null)
        {
            StartCoroutine(FloatAndScaleAnimation(p1ResultImage.transform));
            Debug.Log("P1 Result Image 动画已启动");
        }

        if (p2ResultImage != null)
        {
            StartCoroutine(FloatAndScaleAnimation(p2ResultImage.transform));
            Debug.Log("P2 Result Image 动画已启动");
        }
    }

    IEnumerator PunchScale(Transform target, float punchSize, float duration)
    {
        Vector3 originalScale = Vector3.one;
        target.localScale = originalScale * punchSize;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            target.localScale = Vector3.Lerp(originalScale * punchSize, originalScale, elapsed / duration);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        target.localScale = originalScale;
    }

    IEnumerator FloatAndScaleAnimation(Transform target)
    {
        if (target == null)
        {
            Debug.LogWarning("浮动动画目标为空！");
            yield break;
        }

        // 记录初始位置和缩放
        Vector3 originalPosition = target.localPosition;
        Vector3 originalScale = target.localScale;

        // 记录初始旋转，确保动画不影响原本的旋转
        Quaternion originalRotation = target.rotation;
        Vector3 originalEulerAngles = target.eulerAngles;

        // 添加随机相位偏移，让不同元素浮动不同步
        float timeOffset = Random.Range(0f, 2f * Mathf.PI);
        float scaleTimeOffset = Random.Range(0f, 2f * Mathf.PI);

        Debug.Log($"启动浮动动画: {target.name}, 原始位置: {originalPosition}, 原始缩放: {originalScale}");

        // 延迟开始
        yield return new WaitForSeconds(startDelay);

        // 淡入效果
        if (target.GetComponent<Image>() != null)
        {
            Image image = target.GetComponent<Image>();
            Color originalColor = image.color;
            Color transparentColor = originalColor;
            transparentColor.a = 0f;

            float fadeDuration = 0.5f;
            float fadeElapsed = 0f;

            while (fadeElapsed < fadeDuration)
            {
                fadeElapsed += Time.deltaTime;
                float progress = fadeElapsed / fadeDuration;
                float alpha = Mathf.Lerp(0f, 1f, progress);
                image.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                yield return null;
            }
        }
        else if (target.GetComponent<TextMeshProUGUI>() != null)
        {
            TextMeshProUGUI text = target.GetComponent<TextMeshProUGUI>();
            Color originalColor = text.color;
            Color transparentColor = originalColor;
            transparentColor.a = 0f;

            float fadeDuration = 0.5f;
            float fadeElapsed = 0f;

            while (fadeElapsed < fadeDuration)
            {
                fadeElapsed += Time.deltaTime;
                float progress = fadeElapsed / fadeDuration;
                float alpha = Mathf.Lerp(0f, 1f, progress);
                text.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                yield return null;
            }
        }

        // 开始浮动循环
        while (settlementPanel != null && settlementPanel.activeSelf)
        {
            float time = Time.time + timeOffset;
            float scaleTime = Time.time + scaleTimeOffset;

            // 垂直浮动（幅度更小，更轻柔）
            float floatValue = Mathf.Sin(time * floatSpeed) * floatAmplitude;

            // 使用localPosition确保正确的坐标系
            target.localPosition = originalPosition + Vector3.up * floatValue;

            // 缩放动画（幅度更小，更轻微）
            float scaleValue = 1f + Mathf.Sin(scaleTime * scaleSpeed) * scaleAmplitude;
            target.localScale = originalScale * scaleValue;

            // 轻微旋转（幅度更小）
            float rotation = Mathf.Sin(time * floatSpeed * 0.3f) * 0.3f;
            target.rotation = originalRotation * Quaternion.Euler(0f, 0f, rotation);

            yield return null;
        }

        // 恢复原状
        if (target != null)
        {
            target.localPosition = originalPosition;
            target.localScale = originalScale;
            target.rotation = originalRotation;
            target.eulerAngles = originalEulerAngles;
        }
    }

    void ShowKillUI()
    {
        killImage.gameObject.SetActive(true);

        // 确保初始alpha正常
        Color c = killImage.color;
        c.a = 1f;
        killImage.color = c;

        StartCoroutine(KillScaleAnimation());
    }

    IEnumerator KillScaleAnimation()
    {
        killImage.transform.localScale = Vector3.zero;
        killImage.gameObject.SetActive(true);

        float phase1Duration = 0.15f;
        float elapsed = 0f;

        while (elapsed < phase1Duration)
        {
            float progress = elapsed / phase1Duration;
            float scaleValue = Mathf.Lerp(0f, 2.2f, Mathf.Pow(progress, 0.5f));
            killImage.transform.localScale = Vector3.one * scaleValue;

            float rotation = Mathf.Lerp(0f, 15f, progress);
            killImage.transform.rotation = Quaternion.Euler(0f, 0f, rotation);

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        elapsed = 0f;
        float phase2Duration = 0.1f;
        float startScale = 2.2f;
        float targetScale = 0.6f;

        while (elapsed < phase2Duration)
        {
            float progress = elapsed / phase2Duration;
            float overshoot = Mathf.Sin(progress * Mathf.PI) * 0.2f;
            float scaleValue = Mathf.Lerp(startScale, targetScale, progress) - overshoot;
            killImage.transform.localScale = Vector3.one * scaleValue;

            float rotation = Mathf.Lerp(15f, -5f, progress);
            killImage.transform.rotation = Quaternion.Euler(0f, 0f, rotation);

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        elapsed = 0f;
        float phase3Duration = 0.1f;
        startScale = 0.6f;
        targetScale = 1.5f;

        while (elapsed < phase3Duration)
        {
            float progress = elapsed / phase3Duration;
            float scaleValue = Mathf.Lerp(startScale, targetScale, progress) + Mathf.Sin(progress * Mathf.PI) * 0.3f;
            killImage.transform.localScale = Vector3.one * scaleValue;

            float rotation = Mathf.Lerp(-5f, 3f, progress);
            killImage.transform.rotation = Quaternion.Euler(0f, 0f, rotation);

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        elapsed = 0f;
        float phase4Duration = 0.3f;
        startScale = 1.5f;
        float finalScale = 1f;

        while (elapsed < phase4Duration)
        {
            float progress = elapsed / phase4Duration;

            float damping = Mathf.Exp(-progress * 3f);
            float frequency = 10f;
            float oscillation = Mathf.Sin(progress * frequency * Mathf.PI) * damping * 0.3f;

            float baseScale = Mathf.Lerp(startScale, finalScale, progress);
            float scaleValue = baseScale + oscillation;
            killImage.transform.localScale = Vector3.one * scaleValue;

            float rotation = Mathf.Sin(progress * frequency * Mathf.PI * 0.5f) * damping * 2f;
            killImage.transform.rotation = Quaternion.Euler(0f, 0f, rotation);

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        // 最终归位
        killImage.transform.localScale = Vector3.one;
        killImage.transform.rotation = Quaternion.identity;

        // 停顿
        yield return new WaitForSeconds(0.3f);

        // 淡出
        yield return StartCoroutine(FadeOutKillImage());
    }

    IEnumerator FadeOutKillImage()
    {
        float fadeDuration = 0.5f;

        Color c = killImage.color;
        float startAlpha = c.a;

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, 0f, t / fadeDuration);

            c.a = alpha;
            killImage.color = c;

            // 轻微缩小
            float scale = Mathf.Lerp(1f, 0.8f, t / fadeDuration);
            killImage.transform.localScale = Vector3.one * scale;

            yield return null;
        }

        c.a = 0f;
        killImage.color = c;

        killImage.gameObject.SetActive(false);

        // 重置
        c.a = 1f;
        killImage.color = c;
        killImage.transform.localScale = Vector3.one;
    }

    public void OnPlayerMiss(PlayerController dodger)
    {
        if (dodger == p1)
            StartCoroutine(MissAnimation(p1MissImage, p1MissStartPos));
        else
            StartCoroutine(MissAnimation(p2MissImage, p2MissStartPos));
    }

    IEnumerator MissAnimation(Image missImage, Vector3 originalPosition)
    {
        missImage.gameObject.SetActive(true);
        missImage.transform.localPosition = originalPosition;

        Color originalColor = missImage.color;
        originalColor.a = 1f;
        missImage.color = originalColor;

        float elapsed = 0f;
        float totalDuration = missTotalDuration;

        while (elapsed < totalDuration)
        {
            float progress = elapsed / totalDuration;

            float easeValue = 1f - Mathf.Pow(1f - progress, 2);
            float moveX = Mathf.Lerp(0f, missMoveDistance, easeValue);
            missImage.transform.localPosition = originalPosition + Vector3.right * moveX;

            float alpha = progress < 0.1f ? Mathf.Lerp(0f, 1f, progress * 10f)
                        : progress < 0.6f ? 1f
                        : 1f - ((progress - 0.6f) / 0.4f);

            missImage.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);

            elapsed += Time.deltaTime;
            yield return null;
        }

        missImage.gameObject.SetActive(false);
        missImage.transform.localPosition = originalPosition;
        missImage.color = originalColor;
    }

    void RestartFullMatch()
    {
        matchIsOver = false;
        p1Wins = 0;
        p2Wins = 0;
        ResetRound();
    }

    void ReturnToTitle()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Title");
    }

    IEnumerator HitStopRoutine()
    {
        Time.timeScale = 0.05f;
        yield return new WaitForSecondsRealtime(hitStopDuration);
        Time.timeScale = 1f;
    }
}
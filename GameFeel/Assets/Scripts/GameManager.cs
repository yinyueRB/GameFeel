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
    // 【修改】五局三胜，意味着谁先拿到 3 分谁就赢了！
    public int winsNeeded = 3; 
    private bool matchIsOver = false; 

    [Header("战斗 UI")]
    public TextMeshProUGUI countdownText; // 依然用来显示 3, 2, 1
    public Image fightImage;     // 【新增】美术画的 FIGHT 图片
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
    
    // 【修改】将P1和P2的胜负图片完全分开
    public Sprite p1WinnerSprite;   
    public Sprite p1LoserSprite;    
    public Sprite p2WinnerSprite;   
    public Sprite p2LoserSprite;    

    public Image scoreTitleImage;   // 【新增】其实代码里不用它，但可以留个槽位方便管理
    public TextMeshProUGUI finalScoreText;  

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
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

    // ==========================================
    // 流程控制：倒计时变成图片切换
    // ==========================================
    IEnumerator RoundStartRoutine()
    {
        p1.canAct = false;
        p2.canAct = false;
        
        countdownText.gameObject.SetActive(true);
        if (fightImage != null) fightImage.gameObject.SetActive(false); // 确保一开始隐藏FIGHT图片
        killImage.gameObject.SetActive(false);
        p1MissImage.gameObject.SetActive(false);
        p2MissImage.gameObject.SetActive(false);
        settlementPanel.SetActive(false); 

        // 3, 2, 1 倒计时（使用文字）
        for (int i = 3; i > 0; i--)
        {
            countdownText.text = i.ToString();
            if(tickClip != null && AudioManager.Instance != null) AudioManager.Instance.PlaySFX(tickClip); 
            StartCoroutine(PunchScale(countdownText.transform, 1.5f, 0.2f));
            yield return new WaitForSeconds(1f);
        }

        // 【修改】倒计时结束，隐藏文字，弹出 FIGHT 图片！
        countdownText.gameObject.SetActive(false); 
        if (fightImage != null)
        {
            fightImage.gameObject.SetActive(true);
            StartCoroutine(PunchScale(fightImage.transform, 2f, 0.3f));
        }

        if(fightClip != null && AudioManager.Instance != null) AudioManager.Instance.PlaySFX(fightClip, 1.2f); 

        p1.canAct = true;
        p2.canAct = true;

        yield return new WaitForSeconds(0.5f);
        if (fightImage != null) fightImage.gameObject.SetActive(false); // 半秒后隐藏 FIGHT 图片
    }

    void ResetRound()
    {
        p1.ResetPlayer();
        p2.ResetPlayer();
        StartCoroutine(RoundStartRoutine());
    }

    // ==========================================
    // 核心判定与延迟结算（解决重叠问题）
    // ==========================================
    public void OnPlayerKilled(PlayerController loser)
    {
        if (matchIsOver) return; 

        StartCoroutine(HitStopRoutine());
        if (CameraShake.Instance != null) 
            CameraShake.Instance.Shake(shakeDuration, shakeMagnitude);

        if(killClip != null && AudioManager.Instance != null) AudioManager.Instance.PlaySFX(killClip);

        ShowKillUI();

        if (loser == p1) p2Wins++;
        else p1Wins++;

        if (p1Wins >= winsNeeded || p2Wins >= winsNeeded)
        {
            // 【修改核心】：KILL 动画持续 2 秒。
            // 之前是 1.5 秒弹结算，导致重叠。现在改成 2.5 秒！
            // 这样能让玩家清楚看到 KILL 消失，停顿 0.5 秒后，结算面板才重磅砸下！
            Invoke("ShowSettlementScreen", 2.5f); 
        }
        else
        {
            // 如果还没分出最终胜负，下一回合也稍微等久一点点
            Invoke("ResetRound", 3f); 
        }
    }

    void ShowSettlementScreen()
    {
        matchIsOver = true;
        p1.canAct = false;
        p2.canAct = false;
        settlementPanel.SetActive(true);

        // 【修改】因为有了SCORE图片，文本只需要单纯显示数字即可
        finalScoreText.text = $"{p1Wins} : {p2Wins}";

        // 【修改】分别使用 P1 和 P2 自己专属的美术字图！
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

        StartCoroutine(PunchScale(p1ResultImage.transform, 1.2f, 0.3f));
        StartCoroutine(PunchScale(p2ResultImage.transform, 1.2f, 0.3f));
    }

    // ==========================================
    // 工具库
    // ==========================================
    // 重新开启全新的比赛
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

    public void OnPlayerMiss(PlayerController dodger)
    {
        if (dodger == p1) StartCoroutine(FloatAndFade(p1MissImage));
        else StartCoroutine(FloatAndFade(p2MissImage));
    }

    void ShowKillUI()
    {
        killImage.gameObject.SetActive(true);
        killImage.transform.localScale = Vector3.one * 5f; 
        StartCoroutine(PunchScale(killImage.transform, 1f, 0.1f));
        // KILL UI 存在 2 秒钟
        Invoke("HideKillUI", 2f);
    }

    void HideKillUI() { killImage.gameObject.SetActive(false); }

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

    IEnumerator FloatAndFade(Image uiImage) 
    {
        uiImage.gameObject.SetActive(true);
        Vector3 startPos = uiImage.transform.position;
        float elapsed = 0f;
        float duration = 0.5f;

        while (elapsed < duration)
        {
            uiImage.transform.position = startPos + Vector3.up * (elapsed * 100f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        uiImage.gameObject.SetActive(false);
        uiImage.transform.position = startPos; 
    }

    IEnumerator HitStopRoutine()
    {
        Time.timeScale = 0.05f; 
        yield return new WaitForSecondsRealtime(hitStopDuration); 
        Time.timeScale = 1f;    
    }
}
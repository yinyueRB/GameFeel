using System.Collections;
using UnityEngine;
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
    public int winsNeeded = 2; // 三局两胜
    private bool matchIsOver = false; // 标记整场比赛是否结束

    [Header("战斗 UI (TextMeshPro)")]
    public TextMeshProUGUI countdownText; 
    public TextMeshProUGUI killText;      
    public TextMeshProUGUI p1MissText;    
    public TextMeshProUGUI p2MissText;    

    [Header("Audio Clips (声音文件)")]
    public AudioClip tickClip;   // 倒计时的声音
    public AudioClip fightClip;  // 开始Fight的声音
    public AudioClip killClip;   // 击杀音效
    
    [Header("结算 UI (左右分屏)")]
    public GameObject settlementPanel;      // 整个结算界面父物体
    public TextMeshProUGUI p1ResultText;    // 左半屏字 (WINNER/LOSER)
    public TextMeshProUGUI p2ResultText;    // 右半屏字 (WINNER/LOSER)
    public TextMeshProUGUI finalScoreText;  // 中央比分字 (2 : 0)
    
    [Header("跳转场景")]
    public string titleSceneName = "Title";

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
        // 如果比赛结束了
        if (matchIsOver)
        {
            // 按下空格键，重新开始当前场景（再来一局）
            if (Input.GetKeyDown(KeyCode.Space))
            {
                RestartFullMatch();
            }
            // 按下 ESC 键，返回标题画面
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                ReturnToTitle();
            }
        }
    }

    // ==========================================
    // 流程控制
    // ==========================================
    IEnumerator RoundStartRoutine()
    {
        p1.canAct = false;
        p2.canAct = false;
        
        countdownText.gameObject.SetActive(true);
        killText.gameObject.SetActive(false);
        p1MissText.gameObject.SetActive(false);
        p2MissText.gameObject.SetActive(false);
        settlementPanel.SetActive(false); // 确保结算界面关闭
        
        for (int i = 3; i > 0; i--)
        {
            countdownText.text = i.ToString();
            if(tickClip != null) AudioManager.Instance.PlaySFX(tickClip);
            StartCoroutine(PunchScale(countdownText.transform, 1.5f, 0.2f));
            yield return new WaitForSeconds(1f);
        }

        countdownText.text = "FIGHT!";
        if(fightClip != null) AudioManager.Instance.PlaySFX(fightClip, 1.5f);
        StartCoroutine(PunchScale(countdownText.transform, 2f, 0.3f));

        p1.canAct = true;
        p2.canAct = true;

        yield return new WaitForSeconds(0.5f);
        countdownText.gameObject.SetActive(false); 
    }

    void ResetRound()
    {
        p1.ResetPlayer();
        p2.ResetPlayer();
        StartCoroutine(RoundStartRoutine());
    }

    // ==========================================
    // 核心判定与结算
    // ==========================================
    public void OnPlayerKilled(PlayerController loser)
    {
        if (matchIsOver) return; // 防止鞭尸导致重复计分
        
        if(killClip != null) AudioManager.Instance.PlaySFX(killClip);

        StartCoroutine(HitStopRoutine());
        if (CameraShake.Instance != null) 
            CameraShake.Instance.Shake(shakeDuration, shakeMagnitude);

        ShowKillUI();

        // 计分
        if (loser == p1) p2Wins++;
        else p1Wins++;

        // 检查是否有人赢得了整个系列赛
        if (p1Wins >= winsNeeded || p2Wins >= winsNeeded)
        {
            Invoke("ShowSettlementScreen", 1.5f); // 留出一点时间欣赏最后一击的击杀画面，1.5秒后弹结算
        }
        else
        {
            // 还没分出最终胜负，直接进入下一回合
            Invoke("ResetRound", 2.5f); 
        }
    }

    // 呼出最终结算界面
    void ShowSettlementScreen()
    {
        matchIsOver = true;
        
        // 锁住玩家
        p1.canAct = false;
        p2.canAct = false;

        // 打开界面
        settlementPanel.SetActive(true);

        // 更新比分
        finalScoreText.text = $"SCORE\n{p1Wins} : {p2Wins}";

        // 左右半屏展示酷炫的复古判定
        if (p1Wins > p2Wins)
        {
            p1ResultText.text = "<color=yellow>WINNER!</color>";
            p2ResultText.text = "<color=grey>LOSER</color>";
        }
        else
        {
            p1ResultText.text = "<color=grey>LOSER</color>";
            p2ResultText.text = "<color=yellow>WINNER!</color>";
        }

        // 结算画面做个猛烈的震动/跳动增强复古冲击感
        StartCoroutine(PunchScale(p1ResultText.transform, 1.2f, 0.3f));
        StartCoroutine(PunchScale(p2ResultText.transform, 1.2f, 0.3f));
    }

    // 重新开启全新的三局两胜比赛
    void RestartFullMatch()
    {
        matchIsOver = false;
        p1Wins = 0;
        p2Wins = 0;
        ResetRound();
    }

    public void OnPlayerMiss(PlayerController dodger)
    {
        if (dodger == p1) StartCoroutine(FloatAndFade(p1MissText));
        else StartCoroutine(FloatAndFade(p2MissText));
    }

    // ==========================================
    // 动画工具库
    // ==========================================
    void ShowKillUI()
    {
        killText.gameObject.SetActive(true);
        killText.transform.localScale = Vector3.one * 5f; 
        StartCoroutine(PunchScale(killText.transform, 1f, 0.1f));
        Invoke("HideKillUI", 2f);
    }

    void HideKillUI() { killText.gameObject.SetActive(false); }

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

    IEnumerator FloatAndFade(TextMeshProUGUI uiText) // 【改动】参数类型改为 TMP
    {
        uiText.gameObject.SetActive(true);
        uiText.text = "MISS";
        Vector3 startPos = uiText.transform.position;
        
        float elapsed = 0f;
        float duration = 0.5f;

        while (elapsed < duration)
        {
            uiText.transform.position = startPos + Vector3.up * (elapsed * 100f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        uiText.gameObject.SetActive(false);
        uiText.transform.position = startPos; 
    }

    IEnumerator HitStopRoutine()
    {
        Time.timeScale = 0.05f; 
        yield return new WaitForSecondsRealtime(hitStopDuration); 
        Time.timeScale = 1f;    
    }
    
    void ReturnToTitle()
    {
        // 恢复时间（防止如果在顿帧期间按了返回，导致下一个场景时间依然是停止的）
        Time.timeScale = 1f; 
        
        // 假设你的标题场景名字叫 "TitleScene"
        // 你也可以用场景的 Index 数字，比如 SceneManager.LoadScene(0);
        SceneManager.LoadScene(titleSceneName); 
    }
}
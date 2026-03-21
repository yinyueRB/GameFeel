using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game Feel - 爽感设置")]
    public float hitStopDuration = 0.15f; // 击杀时的“时停”时间（顿帧）
    public float shakeDuration = 0.2f;    // 屏幕震动时间
    public float shakeMagnitude = 0.3f;   // 屏幕震动幅度

    [Header("Players")]
    public PlayerController p1;
    public PlayerController p2;

    [Header("Match Info")]
    public int p1Wins = 0;
    public int p2Wins = 0;
    public int winsNeeded = 2; // 三局两胜

    void Awake()
    {
        Instance = this;
    }

    // 当有玩家死亡时，PlayerController 会调用这个函数告诉裁判
    public void OnPlayerKilled(PlayerController loser)
    {
        // 1. 触发 Game Feel 终极爽感：顿帧 + 屏幕震动
        StartCoroutine(HitStopRoutine());
        if (CameraShake.Instance != null) 
        {
            CameraShake.Instance.Shake(shakeDuration, shakeMagnitude);
        }

        // 2. 计分逻辑
        if (loser == p1) 
        {
            p2Wins++;
            Debug.Log($"<color=orange>回合结束！P2获胜！ 当前比分 P1 [{p1Wins}] : [{p2Wins}] P2</color>");
        }
        else 
        {
            p1Wins++;
            Debug.Log($"<color=orange>回合结束！P1获胜！ 当前比分 P1 [{p1Wins}] : [{p2Wins}] P2</color>");
        }

        // 3. 检查是否有一方获得了总冠军
        if (p1Wins >= winsNeeded || p2Wins >= winsNeeded)
        {
            string winnerName = p1Wins >= winsNeeded ? "Player 1" : "Player 2";
            Debug.Log($"<color=yellow>=== 游戏结束！{winnerName} 获得了最终胜利！ ===</color>");
            // (以后可以在这里呼出结算UI面板)
        }
        else
        {
            // 还没分出最终胜负，2秒后重置开启下一回合
            Invoke("ResetRound", 2f); 
        }
    }

    // 顿帧核心逻辑：通过把时间缩放(TimeScale)降到极低，制造强烈打击感
    IEnumerator HitStopRoutine()
    {
        Time.timeScale = 0.05f; // 几乎时间停止
        // WaitForSecondsRealtime 保证不受 Time.timeScale 影响，真实等待 0.15 秒
        yield return new WaitForSecondsRealtime(hitStopDuration); 
        Time.timeScale = 1f;    // 恢复正常时间
    }

    // 重置回合
    void ResetRound()
    {
        p1.ResetPlayer();
        p2.ResetPlayer();
        Debug.Log("--- 新回合开始！Fight！ ---");
    }
}
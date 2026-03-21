using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // === 游戏设计参数 ===
    [Header("Game Control")]
    public bool canAct = false; // 控制玩家能否行动的开关
    
    [Header("Frame Data (时间/秒)")]
    public float shootStartup = 0.3f;
    public float shootRecovery = 0.5f;
    public float fakeStartup = 0.4f;
    public float fakeRecovery = 0.3f;
    public float dodgeInvincible = 0.3f;
    public float dodgeRecovery = 0.4f;

    [Header("Game Feel")]
    public float inputBufferTime = 0.2f; 
    // [新增] 完美闪避的判定窗口（秒）。
    // 如果在被击中前 0.15 秒内按下闪避，就是完美闪避！
    public float perfectDodgeWindow = 0.15f; 

    [Header("Input Settings")]
    public KeyCode shootKey;
    public KeyCode fakeKey;
    public KeyCode dodgeKey;

    // [新增] 互相引用，为了知道对手是谁
    [Header("Combat")]
    public PlayerController opponent;

    public enum PlayerState { Idle, Startup, Active, Recovery, Dead }
    public enum ActionType { None, Shoot, Fake, Dodge }

    [Header("Current Status")]
    public PlayerState currentState = PlayerState.Idle;
    public ActionType currentAction = ActionType.None;

    private float stateTimer = 0f;
    private ActionType bufferedAction = ActionType.None;
    private float lastInputTime = -10f;
    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        UpdateVisuals(); 

        // 如果自己死了，或者对手死了（回合结束），就不再接收输入和处理状态
        if (!canAct || currentState == PlayerState.Dead || (opponent != null && opponent.currentState == PlayerState.Dead)) return;

        HandleInput();
        ProcessState();
    }

    void HandleInput()
    {
        ActionType inputAction = ActionType.None;

        if (Input.GetKeyDown(shootKey)) inputAction = ActionType.Shoot;
        else if (Input.GetKeyDown(fakeKey)) inputAction = ActionType.Fake;
        else if (Input.GetKeyDown(dodgeKey)) inputAction = ActionType.Dodge;

        if (inputAction != ActionType.None)
        {
            bufferedAction = inputAction;
            lastInputTime = Time.time;
        }
    }

    void ProcessState()
    {
        if (currentState == PlayerState.Idle)
        {
            if (bufferedAction != ActionType.None && (Time.time - lastInputTime <= inputBufferTime))
            {
                StartAction(bufferedAction);
            }
            else if (Time.time - lastInputTime > inputBufferTime)
            {
                bufferedAction = ActionType.None; 
            }
        }
        else
        {
            stateTimer -= Time.deltaTime;

            if (stateTimer <= 0)
            {
                AdvanceState();
            }
        }
    }

    void StartAction(ActionType action)
    {
        currentAction = action;
        bufferedAction = ActionType.None; 

        switch (action)
        {
            case ActionType.Shoot:
                currentState = PlayerState.Startup;
                stateTimer = shootStartup;
                break;
            case ActionType.Fake:
                currentState = PlayerState.Startup;
                stateTimer = fakeStartup;
                break;
            case ActionType.Dodge:
                currentState = PlayerState.Active; 
                stateTimer = dodgeInvincible;
                break;
        }
    }

    void AdvanceState()
    {
        if (currentState == PlayerState.Startup)
        {
            currentState = PlayerState.Active;
            
            if (currentAction == ActionType.Shoot)
            {
                stateTimer = 0.1f; 
                
                // [新增] 开火瞬间，攻击对手！
                if (opponent != null)
                {
                    opponent.ReceiveAttack();
                }
            }
            else if (currentAction == ActionType.Fake)
            {
                stateTimer = 0.1f; 
            }
        }
        else if (currentState == PlayerState.Active)
        {
            currentState = PlayerState.Recovery;
            
            if (currentAction == ActionType.Shoot) stateTimer = shootRecovery;
            else if (currentAction == ActionType.Fake) stateTimer = fakeRecovery;
            else if (currentAction == ActionType.Dodge) stateTimer = dodgeRecovery;
        }
        else if (currentState == PlayerState.Recovery)
        {
            currentState = PlayerState.Idle;
            currentAction = ActionType.None;
        }
    }

    // ==========================================
    // 战斗判定核心逻辑
    // ==========================================
    public void ReceiveAttack()
    {
        // 如果已经被击杀，直接忽略
        if (currentState == PlayerState.Dead) return;

        // 1. 检查是否正在闪避无敌帧中
        if (currentState == PlayerState.Active && currentAction == ActionType.Dodge)
        {
            // 计算已经闪避了多久：(总无敌时间 - 剩余时间)
            float timeInDodge = dodgeInvincible - stateTimer; 

            // 如果刚按下闪避不久（在完美闪避窗口内）
            if (timeInDodge <= perfectDodgeWindow)
            {
                PerfectDodge();
            }
            else
            {
                // 超出完美闪避窗口，但依然在无敌帧内，算作普通闪避
                Debug.Log(gameObject.name + " 闪避成功 (Miss!)");
                if (GameManager.Instance != null) GameManager.Instance.OnPlayerMiss(this);
            }
        }
        // 2. 如果不在无敌帧，被击中！
        else
        {
            Die();
        }
    }

    void PerfectDodge()
    {
        Debug.Log("<color=green>" + gameObject.name + " 完美闪避！(Perfect Dodge!)</color>");
        
        if (GameManager.Instance != null) GameManager.Instance.OnPlayerMiss(this);
        
        // 完美闪避的巨大收益：直接强制取消接下来的所有后摇，回到Idle，可以立刻拔枪反杀对手！
        currentState = PlayerState.Idle;
        currentAction = ActionType.None;
        stateTimer = 0f;
        bufferedAction = ActionType.None; // 清除可能残留的错误缓冲

        // （后续可以在这里加入屏幕闪光、时停顿帧等极具冲击力的 Game Feel 特效）
    }

    void Die()
    {
        Debug.Log("<color=red>" + gameObject.name + " 被击杀了！(Kill!)</color>");
        currentState = PlayerState.Dead;
        currentAction = ActionType.None;

        // 报告裁判有人死了！
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerKilled(this);
        }
    }

    void UpdateVisuals()
    {
        if (currentState == PlayerState.Idle) sr.color = Color.white;
        else if (currentState == PlayerState.Startup) 
        {
            sr.color = currentAction == ActionType.Shoot ? Color.yellow : new Color(1f, 0.8f, 0f); 
        }
        else if (currentState == PlayerState.Active)
        {
            if (currentAction == ActionType.Shoot) sr.color = Color.red; 
            else if (currentAction == ActionType.Fake) sr.color = Color.white; 
            else if (currentAction == ActionType.Dodge) sr.color = Color.blue; 
        }
        else if (currentState == PlayerState.Recovery)
        {
            sr.color = Color.gray; 
        }
        // 死亡变成黑色
        else if (currentState == PlayerState.Dead)
        {
            sr.color = Color.black;
        }
    }
    
    // 用于下一回合重置角色状态
    public void ResetPlayer()
    {
        currentState = PlayerState.Idle;
        currentAction = ActionType.None;
        stateTimer = 0f;
        bufferedAction = ActionType.None;
        canAct = false; 
    }
}
# Brackey-GameJam-2025

# 玩家角色功能（未完更新中）

## 概述
用一个状态机系统管理 NPC 的行为状态。支持枚举状态管理、黑板数据共享和状态转移。

## 系统架构

### 文件结构
```
// 角色基本操作
BirdController/
├── PlayerController.cs              # 角色处理键盘输入，WASD，tab切换，空格释放技能
├── PlayerStateManager.cs            # 角色状态管理/主要用于事件管理 - Normal Hurt Dead


// 角色健康系统
BirdController/
├── BirdHealthManager.cs             # 角色血量 - 上限和当前
├── HealthUIHearts.cs                # 角色血量UI显示

// 游戏系统管理
GameManager/
├── RestartCurrentLevel.cs           # 重载场景（异步）

Unity结构
Bird
├── BirdDash
├── BirdPupu
├── BirdBoom

```

## 一些重要的方法

1. PlayerController.cs - 绑定在Bird父结构
角色切换 - 因为只有3个角色，采取暴力方法
    // 三个角色
    public GameObject characterA;
    public GameObject characterB;
    public GameObject characterC;
    
    // 三个能力脚本，拖入对应技能的脚本
    public MonoBehaviour abilityA;
    public MonoBehaviour abilityB;
    public MonoBehaviour abilityC;

   private void ShowCharacter(int index) // 0=A, 1=B, 2=C

   在角色切换时会发布事件：OnCharacterSwitchedHealth?.Invoke(hm); 让UI刷新血条显示

2.PlayerStateManager.cs
    // 角色状态枚举
    public enum PlayerState
    {
        Normal,
        Hurt,
        Dead
    }

    // 状态切换语法：
    ChangeState(PlayerState.Hurt);

    // 后续可以在这里添加死亡动画和音效
        // 进入受伤状态
        private void OnEnterHurtState()
        {
            Debug.Log("[StateManager] 进入受伤状态");
            // 这里可以播放受伤动画、音效、屏幕震动等
        }
        
        // 进入死亡状态
        private void OnEnterDeadState()
        {
            Debug.Log("[StateManager] 进入死亡状态");
            // 死亡逻辑
        }
    
    // 敌人列表 - 可以直接在inspector中加入敌人object的名称，并设置相应伤害数值
    public List<EnemyConfig> enemyList = new List<EnemyConfig>()
    {
        new EnemyConfig { enemyName = "Human", damageValue = 1 },
        new EnemyConfig { enemyName = "Car", damageValue = 5 },
    };

    private void ProcessCollision(GameObject other) 根据敌人名称进行碰撞监测 //伤害类代码不应该写在这里的，但是写都写了

3. BirdHealthManager.cs
   设置每只鸟不同的血条，绑定在每只鸟的game object上
   Start, TakeDamage, Heal都会发送事件
   OnHealthChanged?.Invoke(currentHealth, maxHealth);

   Heal() Die()都是公共方法，Heal可以治疗至满血 // to do, take value
   
4. HealthUIHearts.cs
    血条由场景下的healthContainer和UIManager单独控制
    重要方法，根据当前角色当前血量和最大值生成血条
    private void BindTo(BirdHealthManager hm)
    {
        UnbindHealth();

        boundHM = hm;
        if (boundHM != null)
        {
            // 订阅并立即刷新一次（包含 max 变化）
            boundHM.OnHealthChanged += HandleHealthChanged;
            HandleHealthChanged(boundHM.currentHealth, boundHM.maxHealth);
        }
   }

5. RestartCurrentLevel.cs
    公共方法 RestartCurrentLevel.RestartLevel(); 可以重新加载当前关卡

---

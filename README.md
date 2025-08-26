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

## 1. `PlayerController.cs` （绑定在 Bird 父结构）
负责角色切换与技能释放。  
由于只有 3 个角色，使用简单的暴力切换方式。

```csharp
// 三个角色
public GameObject characterA;
public GameObject characterB;
public GameObject characterC;

// 三个能力脚本，拖入对应技能的脚本
public MonoBehaviour abilityA;
public MonoBehaviour abilityB;
public MonoBehaviour abilityC;

private void ShowCharacter(int index) // 0=A, 1=B, 2=C
```

- 在角色切换时，会发布事件：
```csharp
OnCharacterSwitchedHealth?.Invoke(hm);
```
→ 供 UI 重新刷新血条显示。

---

## 2. `PlayerStateManager.cs`
负责角色状态管理，支持 **Normal / Hurt / Dead**。

```csharp
public enum PlayerState
{
    Normal,
    Hurt,
    Dead
}
```

状态切换方法：
```csharp
ChangeState(PlayerState.Hurt);
```

### 状态进入回调
```csharp
private void OnEnterHurtState()
{
    Debug.Log("[StateManager] 进入受伤状态");
    // 可在此播放受伤动画、音效、屏幕震动等
}

private void OnEnterDeadState()
{
    Debug.Log("[StateManager] 进入死亡状态");
    // 死亡逻辑
}
```

### 敌人配置（Inspector 可直接修改）
```csharp
public List<EnemyConfig> enemyList = new List<EnemyConfig>()
{
    new EnemyConfig { enemyName = "Human", damageValue = 1 },
    new EnemyConfig { enemyName = "Car", damageValue = 5 },
};
```

碰撞检测（根据敌人名称匹配）：  
```csharp
private void ProcessCollision(GameObject other)
{
    // 根据名称判断敌人并造成伤害
}
```
> 注：伤害逻辑最好不要写在这里，目前先放在此。

---

## 3. `BirdHealthManager.cs`
- 绑定在 **每只鸟的 GameObject** 上。  
- 管理该角色的血量（当前值 / 最大值）。  
- 在 `Start`、`TakeDamage`、`Heal` 等方法里都会发送事件：

```csharp
OnHealthChanged?.Invoke(currentHealth, maxHealth);
```

公共方法：
- `Heal()`：治疗至满血（TODO: 支持传入数值）。  
- `Die()`：处理死亡逻辑。

---

## 4. `HealthUIHearts.cs`
血条 UI 控制，挂在场景下的 `healthContainer` / `UIManager`。  
监听当前角色的 `BirdHealthManager`，动态生成心形血条。

关键方法：
```csharp
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
```

- 根据 `current / max` 动态绘制心心血条。  
- 切换角色时退订旧 HM，订阅新 HM。

---

## 5. `RestartCurrentLevel.cs`
用于关卡重载。  

公共方法：
```csharp
RestartCurrentLevel.RestartLevel();
```
→ 异步重新加载当前关卡。


---

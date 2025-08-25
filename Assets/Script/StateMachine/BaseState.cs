using System;
using UnityEngine;

/*
基础状态抽象类

具体的状态从这里继承。
*/
public abstract class BaseState : IState
{
    [Header("External Config")]
    public StateMachine stateMachine;
    public GameObject owner;

    [Header("State Config")]
    private Enum[] canTransitionToStates;



    // 构造函数
    // stateMachine: 状态机引用
    // owner: 拥有者游戏对象
    public BaseState(StateMachine stateMachine, GameObject owner)
    {
        this.stateMachine = stateMachine;
        this.owner = owner;
    }

    // 进入状态
    public virtual void OnEnter()
    {
        return;
    }

    // 状态更新
    public virtual void OnUpdate()
    {
        return;
    }

    // 物理更新
    public virtual void OnFixedUpdate()
    {
        return;
    }

    // 退出状态
    public virtual void OnExit()
    {
        return;
    }

    // 设置可以转换到的状态
    // states: 可以转换到的状态数组
    public void SetCanTransitionToStates(Enum[] states)
    {
        canTransitionToStates = states;
    }

    // 检查状态转换条件
    // targetState: 目标状态类型
    public virtual bool CanTransitionTo(Enum targetState)
    {
        if (canTransitionToStates != null)
        {
            foreach (Enum state in canTransitionToStates)
            {
                if (state.Equals(targetState))
                {
                    return true;
                }
            }
        }
        return false;
    }

    // 请求状态转换
    // T: 目标状态类型
    public void RequestTransition(Enum targetStateType)
    {
        stateMachine.ChangeState(targetStateType);
    }
}

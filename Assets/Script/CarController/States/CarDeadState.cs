using System;
using UnityEngine;

/*
Car Move状态

实现了随机选择方向行驶，行驶出屏幕后死亡
*/
public class CarDeadState : BaseState
{
    // 构造函数，设置可转换状态，设置黑板
    public CarDeadState(StateMachine stateMachine, GameObject owner) : base(stateMachine, owner)
    {
        SetCanTransitionToStates(new Enum[]
        { 
        });
    }

    public override void OnEnter()
    {
        GameObject.Destroy(owner);
    }

    public override void OnUpdate()
    {
        
    }

    public override void OnFixedUpdate()
    {
        
    }

    public override void OnExit()
    {
        
    }
}
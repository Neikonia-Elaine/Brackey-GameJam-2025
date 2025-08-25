using UnityEngine;
using System;

/*
状态接口。规范状态的行为。
具体的状态从BaseState继承。
*/
public interface IState
{
    // 进入状态时调用
    void OnEnter();

    // 逻辑更新
    void OnUpdate();

    // 物理更新
    void OnFixedUpdate();

    // 退出状态时调用
    void OnExit();

    // 检查是否可以转换到目标状态
    bool CanTransitionTo(Enum targetState);
}

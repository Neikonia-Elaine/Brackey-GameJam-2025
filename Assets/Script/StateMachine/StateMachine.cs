using System;
using System.Collections.Generic;
using UnityEngine;

/*
状态机管理器

负责状态的创建、切换和管理。
*/
public class StateMachine : MonoBehaviour
{
    [Header("Debug Config")]
    public bool enableDebugLog = true;

    [Header("State Machine Config")]
    public BlackBoard blackBoard;
    private Enum currentStateType;
    private Dictionary<Enum, BaseState> stateCache = new Dictionary<Enum, BaseState>();
    private bool isInitialized = false;
    private bool isRunning = false;


    // 初始化状态机并设置初始状态
    // stateType: 初始状态类型
    public void Initialize(Enum stateType, BlackBoard blackBoard)
    {
        currentStateType = stateType;
        this.blackBoard = blackBoard;

        if (enableDebugLog)
        {
            Debug.Log($"[StateMachine] StateMachine initialized, initial state: {currentStateType}");
        }

        isInitialized = true;
    }

    public void RunStateMachine()
    {
        if (!isInitialized)
        {
            Debug.LogWarning($"[StateMachine] StateMachine not initialized");
            return;
        }

        isRunning = true;
        stateCache[currentStateType]?.OnEnter();
    }

    private void Update()
    {
        if (!isRunning)
        {
            return;
        }

        stateCache[currentStateType]?.OnUpdate();
    }

    private void FixedUpdate()
    {
        if (!isRunning)
        {
            return;
        }

        stateCache[currentStateType]?.OnFixedUpdate();
    }

    // 切换到指定状态
    // stateType: 目标状态类型
    public bool ChangeState(Enum stateType)
    {
        if (currentStateType != null && currentStateType == stateType)
        {
            if (enableDebugLog)
                Debug.LogWarning($"[StateMachine] Attempt to switch to the same state: {stateType}");
            return false;
        }

        if (currentStateType != null && !stateCache[currentStateType].CanTransitionTo(stateType))
        {
            if (enableDebugLog)
                Debug.LogWarning($"[StateMachine] State transition rejected: {currentStateType} -> {stateType}");
            return false;
        }

        Enum previousState = null;
        if (currentStateType != null)
        {
            previousState = currentStateType;
            stateCache[currentStateType]?.OnExit();
        }

        currentStateType = stateType;
        stateCache[currentStateType]?.OnEnter();

        if (enableDebugLog)
            Debug.Log($"[StateMachine] State switched: {previousState?.ToString() ?? "None"} -> {currentStateType}");

        return true;
    }

    // 添加状态
    // stateType: 状态类型
    // state: 状态实例
    public void AddState(Enum stateType, BaseState state)
    {
        if (stateCache.ContainsKey(stateType))
        {
            Debug.LogWarning($"[StateMachine] State already exists: {stateType.ToString()}");
            return;
        }

        stateCache[stateType] = state;
    }

    // 检查是否处于指定状态
    // T: 状态类型
    public bool IsInState(Enum stateType)
    {
        return currentStateType != null && currentStateType == stateType;
    }
}

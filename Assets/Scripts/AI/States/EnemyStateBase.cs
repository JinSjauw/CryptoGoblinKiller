using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.LowLevel;
using UnityHFSM;

public class EnemyStateBase : State<EnemyState, StateEvent>
{
    protected readonly Enemy enemy;
    protected readonly NavMeshAgent agent;
    protected readonly Animator animator;

    protected bool requestedExit;
    protected float exitTime;

    protected readonly Action<State<EnemyState, StateEvent>> onEnter;
    protected readonly Action<State<EnemyState, StateEvent>> onLogic;
    protected readonly Action<State<EnemyState, StateEvent>> onExit;
    protected readonly Func<State<EnemyState, StateEvent>, bool> canExit;

    public EnemyStateBase(bool needsExitTime, Enemy enemy, float exitTime = 0.1f,
        Action<State<EnemyState, StateEvent>> onEnter = null,
        Action<State<EnemyState, StateEvent>> onLogic = null,
        Action<State<EnemyState, StateEvent>> onExit = null,
        Func<State<EnemyState, StateEvent>, bool> canExit = null)
    {
        this.enemy = enemy;
        this.exitTime = exitTime;
        this.onEnter = onEnter;
        this.onLogic = onLogic;
        this.onExit = onExit;
        this.canExit = canExit;
        this.needsExitTime = needsExitTime;

        agent = this.enemy.GetComponent<NavMeshAgent>();
        animator = this.enemy.GetComponentInChildren<Animator>();
    }

    public override void OnEnter()
    {
        base.OnEnter();
        requestedExit = false;
        onEnter?.Invoke(this);
    }

    public override void OnLogic()
    {
        base.OnLogic();
        if (requestedExit && timer.Elapsed >= exitTime)
        {
            fsm.StateCanExit();
        }
    }

    public override void OnExitRequest()
    {
        base.OnExitRequest();
        if (!needsExitTime || canExit != null && canExit(this))
        {
            fsm.StateCanExit();
        }
        else
        {
            requestedExit = true;
        }
    }
}

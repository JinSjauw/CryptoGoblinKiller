using System;
using UnityEngine;
using UnityHFSM;

public class MeleeState : EnemyStateBase
{
    private Action<State<EnemyState, StateEvent>> _onAttack;
    private float _attackDelay;
    private float _attackTimer;
    
    public MeleeState(bool needsExitTime, Enemy enemy, Action<State<EnemyState, StateEvent>> onAttack, float ExitTime = 0.33f) : base(needsExitTime, enemy)
    {
        _onAttack = onAttack;
        _attackDelay = enemy.AttackSpeed;
        _attackTimer = _attackDelay;
    }

    public override void OnEnter()
    {
        agent.isStopped = true;
        base.OnEnter();
    }

    public override void OnLogic()
    {
        base.OnLogic();
        //Do a timer?
        
        _attackTimer += Time.deltaTime;
        if (_attackTimer >= _attackDelay)
        {
            _onAttack?.Invoke(this);
            _attackTimer = 0;
            animator.Play("melee", 0, 0);
            //Debug.Log("Attacked!");
        }
    }
}

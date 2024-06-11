using System;
using UnityEngine;

public class GoToObjectiveState : EnemyStateBase
{
    private Transform _target;
    private Func<Transform> _selectObjectiveTarget;

    private float _moveSpeed;
    
    public GoToObjectiveState(bool needsExitTime, Enemy enemy, Func<Transform> selectObjectivePoint) : base(needsExitTime, enemy)
    {
        _selectObjectiveTarget = selectObjectivePoint;
        _moveSpeed = enemy.MoveSpeed;
    }

    public override void OnEnter()
    {
        base.OnEnter();
        animator.Play("walk");
        agent.isStopped = false;
        agent.speed = _moveSpeed;
        _target = _selectObjectiveTarget();
    }

    public override void OnLogic()
    {
        base.OnLogic();
        
        if (!requestedExit)
        {
            Vector3 targetPosition = _target.position;
            agent.SetDestination(new Vector3(targetPosition.x, 0, targetPosition.z));
        }
        
        if(agent.remainingDistance <= agent.stoppingDistance)
        {
            fsm.StateCanExit();

            if (_target.TryGetComponent(out ObjectivePoint point))
            {
                if (!point.IsSieger(enemy) && (point.IsFull() || point.IsDestroyed()))
                {
                    enemy.GoToNextObjective();
                }
            }
        }
    }
}

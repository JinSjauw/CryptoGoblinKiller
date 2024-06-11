using UnityEngine;

public class ChaseState : EnemyStateBase
{
    private Transform _target;
    private float _chaseSpeed;
    
    public ChaseState(bool needsExitTime, Enemy enemy, Transform target) : base(needsExitTime, enemy)
    {
        _target = target;
        _chaseSpeed = enemy.ChaseSpeed;
    }

    public override void OnEnter()
    {
        base.OnEnter();
        agent.enabled = true;
        agent.isStopped = false;
        agent.speed = _chaseSpeed;
        animator.Play("run");
    }

    public override void OnLogic()
    {
        base.OnLogic();
        if (!requestedExit)
        {
            Vector3 playerPosition = _target.position;
            agent.SetDestination(new Vector3(playerPosition.x, 0, playerPosition.z));
        }
        else if(agent.remainingDistance <= agent.stoppingDistance)
        {
            fsm.StateCanExit();
        }
    }
}

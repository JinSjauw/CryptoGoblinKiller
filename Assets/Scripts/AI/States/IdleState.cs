
public class IdleState : EnemyStateBase
{
    public IdleState(bool needsExitTime, Enemy enemy) : base(needsExitTime, enemy)
    {
        
    }

    public override void OnEnter()
    {
        base.OnEnter();
        agent.isStopped = true;
        animator.StopPlayback();
    }
}

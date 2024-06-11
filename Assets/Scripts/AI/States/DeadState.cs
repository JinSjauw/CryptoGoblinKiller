public class DeadState : EnemyStateBase
{
    public DeadState(bool needsExitTime, Enemy enemy) : base(needsExitTime, enemy)
    {
    }

    public override void OnEnter()
    {
        base.OnEnter();
        agent.isStopped = true;
        animator.Play("die");
        enemy.ReturnToPool();
    }
}

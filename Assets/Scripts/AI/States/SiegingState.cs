using UnityEngine;

public class SiegingState : EnemyStateBase
{
    private ObjectivePoint _objectivePoint;
    private HealthComponent _healthComponent;
    private Vector3 _targetPosition;

    private float _attackTimer;
    private float _attackSpeed;
    private float _attackDamage;

    private bool _facingObjective;
    
    public SiegingState(bool needsExitTime, Enemy enemy) : base(needsExitTime, enemy)
    {
        /*_objectivePoint = objective.GetComponent<ObjectivePoint>();
        _healthComponent = objective.GetComponent<HealthComponent>();*/
        _attackSpeed = enemy.AttackSpeed;
        _attackDamage = enemy.MeleeDamage;
    }

    public override void OnEnter()
    {
        base.OnEnter();
        
        //Debug.Log("Getting Siege Position");
        //Get Target Position
        _objectivePoint = enemy.ObjectiveTarget.GetComponent<ObjectivePoint>();
        _healthComponent = _objectivePoint.GetComponent<HealthComponent>();
        _targetPosition = _objectivePoint.GetSiegePosition(enemy);
        agent.SetDestination(new Vector3(_targetPosition.x, enemy.transform.position.y, _targetPosition.z));
    }

    public override void OnLogic()
    {
        base.OnLogic();
        
        if(!requestedExit && agent.remainingDistance <= agent.stoppingDistance)
        {
            //fsm.StateCanExit();
            if (!_facingObjective)
            {
                animator.Play("melee");
                _facingObjective = true;
            }
            
            enemy.transform.LookAt(_objectivePoint.transform);
            //Attack.
            _attackTimer += Time.deltaTime;
            
            if (_attackTimer >= _attackSpeed)
            {
                //Look At target.
                _attackTimer = 0;
                _healthComponent.TakeDamage(_attackDamage);
                //Debug.Log("Sieged!");
            }
        }
    }

    public override void OnExit()
    {
        base.OnExit();
        _facingObjective = false;
    }
}

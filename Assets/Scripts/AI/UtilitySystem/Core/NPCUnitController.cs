using System;
using UnityEngine;
using UnityEngine.AI;
using UtilitySystem.Core;

namespace AI.Core
{
    public class NPCUnitController : MonoBehaviour
    {
        //[SerializeField] private Transform debugTargetPosition;
        //[SerializeField] private BossScript bossScript;
        [SerializeField] private Animator animator;
        
        private NPCUnit _npcUnit;
        private NavMeshAgent _agent;

        private Transform _target;
        
        private Action _onActionComplete;
        private Action _onWaitComplete;

        private bool _isWaiting;
        private float _waitTimer;


        #region Unity Functions

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _npcUnit = GetComponent<NPCUnit>();

            _target = _npcUnit.PointTarget;
            
            Move(_target.position, _npcUnit.MoveSpeed);
        }

        private void Update()
        {
            //Move(_target.position, _npcUnit.MoveSpeed);
        }

        private void OnTriggerStay(Collider other)
        {
            if (!Physics.Linecast(other.transform.position, transform.position, LayerMask.GetMask("Wall")))
            {
                //visible so do smth
                Debug.Log("I can see you!");
                _target = _npcUnit.PlayerTarget;
                Move(_target.position, _npcUnit.MoveSpeed);
            }
            else if(_target != _npcUnit.PointTarget)
            {
                _target = _npcUnit.PointTarget;
                Move(_target.position, _npcUnit.MoveSpeed);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            _target = _npcUnit.PointTarget;
            Move(_target.position, _npcUnit.MoveSpeed);
        }

        #endregion
        
        #region Base Actions

        private Vector3 SelectTarget()
        {
            //Check whether player is near otherwise just go to point if he is not in range/visible for too long.
            //If you lose sight of the player do chase for a little bit.

            return Vector3.zero;
        }
        
        private void Move(Vector3 targetPosition, float moveSpeed, float stoppingDistance = 1.5f)
        {
            _agent.speed = moveSpeed;
            _agent.stoppingDistance = stoppingDistance;
            _agent.SetDestination(targetPosition);
        }
        

        #endregion
        
        #region Utility AI Actions
        
        public void Chase(float moveSpeed, Action onCompleted)
        {
            //Play Move animation
            _agent.speed = moveSpeed;
            _agent.stoppingDistance = 1.5f;

            //_agent.SetDestination(_playerTarget.position);
            _onActionComplete = onCompleted;
        }
        
        public void Wait(float time, Action onCompleted)
        {
            animator.Play("Idle");
            _isWaiting = true;
            _waitTimer = time;
            _onWaitComplete = onCompleted;
        }
        
        #endregion
    }
}

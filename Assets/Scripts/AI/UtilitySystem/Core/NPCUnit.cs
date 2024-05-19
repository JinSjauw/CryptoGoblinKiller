using System;
using AI.Core;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace UtilitySystem.Core
{
    [RequireComponent(typeof(AIBrain), typeof(NPCUnitController))]
    public class NPCUnit : MonoBehaviour
    {
        //NPC AGENT
        [FormerlySerializedAs("state")] [SerializeField] private NPCStates _state;

        //[SerializeField] private GameEventChannel eventChannel;
        
        [SerializeField] private float thinkTime = .75f;
        //Actions List
        [SerializeField] private AIAction[] _availableActions;

        [SerializeField] private float _moveSpeed;
        
        //AI
        private AIBrain _aiBrain;
        private NPCUnitController _controller;
        private NavMeshAgent _agent;

        private Transform _playerTarget;
        [SerializeField] private Transform _pointTarget;
        
        private bool _isRunning;

        public NPCUnitController Controller => _controller;
        public Transform PlayerTarget => _playerTarget;
        public Transform PointTarget => _pointTarget;
        public NPCStates State => _state;

        public float MoveSpeed => _moveSpeed;

        private void Awake()
        {
            Copy(_availableActions);
            
            _aiBrain = GetComponent<AIBrain>();
            _controller = GetComponent<NPCUnitController>();
            _agent = GetComponent<NavMeshAgent>();
            
            //#TODO Inject a cached version of this from the Manager Script.
            _playerTarget = FindObjectOfType<PlayerController>().transform;
            
            //_controller.Initialize(_agent, _playerTarget);
            
            StartAI();
        }

        private void Start()
        {
            //Placeholder ai kickstart
            _aiBrain.DecideBestAction(_availableActions, ExecuteBestAction);
            //eventChannel.BossFightStart += StartAI;
            //eventChannel.BossDeath += Stop;
        }

        /*private void Stop(object sender, EventArgs e)
        {
            _IsRunning = false;
            _Agent.enabled = false;
            state = NPCStates.NONE;
            GetComponent<Collider>().enabled = false;
        }*/

        private void StartAI()
        {
            _isRunning = true;
            _aiBrain.DecideBestAction(_availableActions, ExecuteBestAction);
        }

        private void Update()
        {
            //Controller.ControllerUpdate(_state);
        }

        private void Copy(AIAction[] actions)
        {
            for (int i = 0; i < actions.Length; i++)
            {
                AIAction action = actions[i];
                action = Instantiate(action);
                actions[i] = action;
                for (int j = 0; j < action.considerations.Length; j++)
                {
                    Consideration consideration = action.considerations[j];
                    consideration = Instantiate(consideration);
                    action.considerations[j] = consideration;
                }
            }
        }
        
        private void ExecuteBestAction(AIAction bestAction)
        {
            /*if (bestAction.GetType() == typeof(Chase))
            {
                //Debug.Log("Chase Action is the best!");
                state = NPCStates.MOVING;
            }
            else
            {
                state = NPCStates.BUSY;
            }*/
            //Give it an delegate OnComplete. So It knows to start another decision making process
            bestAction.Execute(this, ActionCompleted);
        }
        
        private void ActionCompleted()
        {
            //CompletedAction
            _state = NPCStates.IDLE;
            SelectNextAction();
            //Start next Cycle;
            //_Controller.Wait(thinkTime, SelectNextAction);
        }

        private void SelectNextAction()
        {
            if(!_isRunning) return;

            //state = NPCStates.THINKING;
            _aiBrain.DecideBestAction(_availableActions, ExecuteBestAction);
        }
    }
    
    //Combine this with a finite state machine
    //States Idle, Thinking, Busy, Done.
}

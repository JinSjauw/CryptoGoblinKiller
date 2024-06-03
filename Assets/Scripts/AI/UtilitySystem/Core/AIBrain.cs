using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UtilitySystem.Core;
using Random = UnityEngine.Random;

namespace AI.Core
{
    //Considers all the actions
    public class AIBrain : MonoBehaviour
    {
        [SerializeField] private float actionScoreDifference;
        //[SerializeField] private float chaseIncrementBias;
        
        private AIAction _BestAction;
        private AIAction _LastAction;
        private NPCUnit _NpcUnit;
        //private int consecutiveAttackCounter;
        
        //public AIAction bestAction { get => _bestAction; }
        
        // Start is called before the first frame update
        void Awake()
        {
            _NpcUnit = GetComponent<NPCUnit>();
        }

        public void DecideBestAction(AIAction[] actions, Action<AIAction> onDecided)
        {
            float score = 0f;
            //int nextBestActionIndex = 0;
            _LastAction = _BestAction;
            
            for (int i = 0; i < actions.Length; i++)
            {
                float currentActionScore = ScoreAction(actions[i]);

                AIAction action = actions[i];
                
                if (_LastAction != null && action.GetType() == _LastAction.GetType())
                {
                    //Debug.Log("Same as Last Action! --");
                    currentActionScore -= .2f;
                }
                
                /*if (action.GetType() == typeof(Chase))
                {
                    currentActionScore += (consecutiveAttackCounter * chaseIncrementBias);
                    consecutiveAttackCounter = 0;
                }*/
                
                if (currentActionScore > score)
                {
                    //nextBestActionIndex = i;
                    score = actions[i].Score;
                   
                }
                
                //Debug.Log("Unit: " + _NpcUnit.name + "Action: " + actions[i].name + " Score: " + actions[i].Score);
            }
            
            List<AIAction> bestActions = actions.ToList().FindAll(a => a.Score >= score - actionScoreDifference);
            _BestAction = bestActions[Random.Range(0, bestActions.Count)];
            
            onDecided(_BestAction);
        }
        
        //Get the score of an action
        public float ScoreAction(AIAction action)
        {
            float lumpedScore = 1f;
            for (int i = 0; i < action.considerations.Length; i++)
            {
                float considerationScore = action.considerations[i].ScoreConsideration(_NpcUnit);
                lumpedScore *= considerationScore;

                if (lumpedScore == 0)
                {
                    action.Score = 0;
                    return action.Score;
                }
            }

            //Average Scheme the lumped score
            float averageScore = lumpedScore;
            float modFactor = 1 - (1 / action.considerations.Length);
            float makeupValue = (1 - averageScore) * modFactor;
            action.Score = averageScore + (makeupValue * averageScore);
            
            return action.Score;
        }
    }
}

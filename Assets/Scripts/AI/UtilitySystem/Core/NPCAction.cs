using System;
using UnityEngine;
using UtilitySystem.Core;

namespace AI.Core
{
    public abstract class AIAction : ScriptableObject
    {
        public string Name;
        private float m_Score;
        
        //Needs to know unitData
        
        public float Score
        {
            get => m_Score;
            set => m_Score = Mathf.Clamp01(value);
        }
        
        [Header("Action Considerations")]
        public Consideration[] considerations;
        
        public virtual void Awake() { Score = 0; }
        public abstract void Execute(NPCUnit npcUnit, Action onCompleted);
    }
}


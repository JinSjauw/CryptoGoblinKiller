using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilitySystem.Core;

namespace AI.Core
{
    public abstract class Consideration : ScriptableObject
    {
        [SerializeField] protected AnimationCurve responseCurve;
        /*[SerializeField] protected float min = 1;
        [SerializeField] protected float max;*/
        private float _score;

        public string Name;

        public float score
        {
            get => _score;
            set => _score = Mathf.Clamp01(value);
        }

        public virtual void Awake()
        {
            score = 0;
        }

        public void Copy()
        {
            
        }
        
        public abstract float ScoreConsideration(NPCUnit unit);
    }
}


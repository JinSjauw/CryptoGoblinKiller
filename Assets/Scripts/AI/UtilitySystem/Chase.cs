using System;
using AI.Core;
using UnityEngine;
using UtilitySystem.Core;

namespace UtilitySystem
{
    [CreateAssetMenu(menuName = "UtilityAI/Actions/Chase")]
    public class Chase : AIAction
    {
        [SerializeField] private float _moveSpeed;
        public override void Execute(NPCUnit npcUnit, Action onCompleted)
        {
            //npcUnit.Controller.Chase(_moveSpeed, onCompleted);
        }
    }
}

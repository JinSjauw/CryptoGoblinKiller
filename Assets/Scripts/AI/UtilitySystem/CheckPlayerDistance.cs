using AI.Core;
using UnityEngine;
using UtilitySystem.Core;

[CreateAssetMenu(menuName = "UtilityAI/Considerations/CheckPlayerDistance")]
public class CheckPlayerDistance : Consideration
{
    public override float ScoreConsideration(NPCUnit unit)
    {
        return 1;
    }
}

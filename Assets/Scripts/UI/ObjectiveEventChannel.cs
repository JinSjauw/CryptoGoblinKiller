using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Event Channels/Objective Event Channel")]
public class ObjectiveEventChannel : ScriptableObject
{
    public UnityAction<int> HealthChangedEvent;
    public UnityAction DestructionEvent;

    public void OnHealthChanged(int health)
    {
        HealthChangedEvent?.Invoke(health);
    }

    public void OnDestructionEvent()
    {
        DestructionEvent?.Invoke();
    }
}

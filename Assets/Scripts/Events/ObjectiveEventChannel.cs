using System;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Event Channels/Objective Event Channel")]
public class ObjectiveEventChannel : ScriptableObject
{
    public UnityAction<float, float,Guid> HealthChangedEvent;
    public UnityAction DestructionEvent;
    public UnityAction<Guid> PointInitEvent;
    public UnityAction LoseEvent;

    public void OnHealthChanged(float health, float maxHealth, Guid id)
    {
        HealthChangedEvent?.Invoke(health, maxHealth,id);
    }

    public void OnDestructionEvent()
    {
        DestructionEvent?.Invoke();
    }

    public void InitPoint(Guid pointID)
    {
        PointInitEvent?.Invoke(pointID);
    }

    public void OnLose()
    {
        LoseEvent?.Invoke();
    }
}

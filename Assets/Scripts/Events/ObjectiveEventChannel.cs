using System;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Event Channels/Objective Event Channel")]
public class ObjectiveEventChannel : ScriptableObject
{
    public UnityAction<float, float, Shape> HealthChangedEvent;
    public UnityAction DestructionEvent;
    public UnityAction<Guid, Shape> PointInitEvent;
    public UnityAction LoseEvent;

    public void OnHealthChanged(float health, float maxHealth, Shape id)
    {
        HealthChangedEvent?.Invoke(health, maxHealth,id);
    }

    public void OnDestructionEvent()
    {
        DestructionEvent?.Invoke();
    }

    /*public void InitPoint(Guid pointID, Shape shapeID)
    {
        PointInitEvent?.Invoke(pointID, shapeID);
    }*/

    public void OnLose()
    {
        LoseEvent?.Invoke();
    }
}

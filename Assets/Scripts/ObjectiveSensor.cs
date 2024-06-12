using UnityEngine;
using UnityEngine.Events;

public class ObjectiveSensor : MonoBehaviour
{
    public UnityAction<Transform> OnObjectiveEnter;
    public UnityAction<Transform> OnObjectiveExit;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out ObjectivePoint point))
        {
            OnObjectiveEnter?.Invoke(point.transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out ObjectivePoint point))
        {
            OnObjectiveExit?.Invoke(point.transform);
        }
    }
}

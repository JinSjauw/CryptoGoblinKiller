using UnityEngine;
using UnityEngine.Events;

public class PlayerSensor : MonoBehaviour
{
    public UnityAction<Transform> OnPlayerEnter;
    public UnityAction<Vector3> OnPlayerExit;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerController player))
        {
            OnPlayerEnter?.Invoke(player.transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out PlayerController player))
        {
            OnPlayerExit?.Invoke(player.transform.position);
        }
    }
}

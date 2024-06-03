using UnityEngine;
using UnityEngine.Events;

public class SensorRange : MonoBehaviour
{
    public UnityAction<Transform> OnEnterRange;
    public UnityAction OnExitRange;
    
    private SphereCollider _collider;

    private void Awake()
    {
        _collider = GetComponent<SphereCollider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        OnEnterRange.Invoke(other.transform);
    }

    private void OnTriggerExit(Collider other)
    {
        OnExitRange.Invoke();
    }

    public void SetRadius(float radius)
    {
        _collider.radius = radius;
    }
}

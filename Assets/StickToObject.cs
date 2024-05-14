using UnityEngine;

public class StickToObject : MonoBehaviour
{
    private Transform _target;
    private Transform _lastParent;

    public void Stick(Transform target)
    {
        _lastParent = transform.parent;
        transform.SetParent(target);
    }

    public void UnStick()
    {
        transform.SetParent(_lastParent);
        transform.localScale = Vector3.one;
    }
}

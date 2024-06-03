using System;
using UnityEngine;

public class StickToObject : MonoBehaviour
{
    //private Transform _target;
    [SerializeField] private Transform _lastParent;

    private void Awake()
    {
        _lastParent = transform.parent;
    }

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

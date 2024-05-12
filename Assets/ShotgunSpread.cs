using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ShotgunSpread : MonoBehaviour
{
    [SerializeField] private float _spreadFactor;
    [SerializeField] private float _horizontalSpreadMultiplier;
    [SerializeField] private float _verticalSpreadMultiplier;
    
    private List<Transform> _pelletSpawnPoints;
    
    private void Awake()
    {
        _pelletSpawnPoints = new List<Transform>();

        foreach (Transform child in transform)
        {
            _pelletSpawnPoints.Add(child);
        }
        
        CreateSpread();
    }

    private void OnValidate()
    {
        if (_pelletSpawnPoints != null)
        {
            CreateSpread();
        }
    }

    private void CreateSpread()
    {
        Vector3 centerPosition = transform.position;
        //Check distance from center (dist scalar).
        //Apply the direction from center to child with a small scaling(spreadFactor).
        foreach (Transform point in _pelletSpawnPoints)
        {
            Vector3 pointPosition = point.position;
            point.forward = transform.forward;
            float distance = Vector3.Distance(pointPosition, centerPosition);
            Vector3 direction = (pointPosition - centerPosition).normalized;
            Vector3 spreadDirection = new Vector3(direction.x * _horizontalSpreadMultiplier, direction.y * _verticalSpreadMultiplier, 0);
            Vector3 spreadAngle = point.forward + (spreadDirection * distance) * _spreadFactor;
            point.forward = spreadAngle;
        }   
    }

    public List<Transform> GetShotgunSpread()
    {
        return _pelletSpawnPoints;
    }
}

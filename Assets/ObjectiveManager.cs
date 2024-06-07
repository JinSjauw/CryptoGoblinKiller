using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveManager : MonoBehaviour
{
    [SerializeField] private ObjectiveEventChannel _objectiveEventChannel;
    [SerializeField] private List<ObjectivePoint> _objectivePoints;

    private bool _IsInitialized;
    private int _destroyedPoints;
    
    private void Awake()
    {
        _IsInitialized = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!_IsInitialized)
        {
            _IsInitialized = true;
            //Bind ID's Here
            foreach (ObjectivePoint point in _objectivePoints)
            {
                _objectiveEventChannel.InitPoint(point.ID);
                point.ObjectiveHealthChangedEvent += OnPointHealthChanged;
                point.ObjectiveDestructionEvent += OnPointDestroyed;
            }
        }
    }

    private void OnPointDestroyed()
    {
        _destroyedPoints++;
        _objectiveEventChannel.OnDestructionEvent();

        if (_destroyedPoints >= _objectivePoints.Count)
        {
            //Game Over
            _objectiveEventChannel.OnLose();
        }
    }

    private void OnPointHealthChanged(float health, float maxHealth, Guid id)
    {
        _objectiveEventChannel.OnHealthChanged(health, maxHealth, id);
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

public class PoolInitializer : MonoBehaviour
{
    [Header("Prefabs to Pool")] 
    [Header("Projectile Prefabs")] 
    [SerializeField] private List<GameObject> _projectilePrefabs;
    [SerializeField] private int _amount;
    
    private ObjectPool _objectPool;

    private void Awake()
    {
        _objectPool = GetComponent<ObjectPool>();
        
        foreach (GameObject prefab in _projectilePrefabs)
        {
            for (int i = 0; i < _amount; i++)
            {
                GameObject prefabObject = _objectPool.GetObject(prefab);
                _objectPool.ReturnGameObject(prefabObject);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

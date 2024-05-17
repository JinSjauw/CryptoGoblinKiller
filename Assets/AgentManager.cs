using System.Collections.Generic;
using UnityEngine;

public class AgentManager : MonoBehaviour
{
    [SerializeField] private Transform _agentPrefab;
    [SerializeField] private List<Transform> _spawnPoints;

    private ObjectPool _objectPool;

    #region Unity Functions

    private void Awake()
    {
        _objectPool = FindObjectOfType<ObjectPool>();
        
        //Find Object pool, otherwise create one?
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    #endregion

    #region Private Regions

    private void SpawnAgent()
    {
        
    }

    private void SpawnWave(int amount)
    {
                
    }

    #endregion
}
